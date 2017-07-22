using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BencodeLib;
using Z1Torrent.Helpers;
using Z1Torrent.PeerWire;

namespace Z1Torrent.Tracker {

    public class HttpTracker : ITracker {

        public Uri Uri { get; internal set; }
        public int Interval { get; internal set; }
        public int MinInterval { get; internal set; }

        private TorrentClient _torrentClient;
        private HttpClient _httpClient;

        private string _trackerId;

        public HttpTracker(TorrentClient client, string url) {
            _torrentClient = client;
            Uri = new Uri(url);
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Announce torrent state to the tracker
        /// </summary>
        /// <param name="meta">Torrent to announce</param>
        /// <param name="ev">Event to announce</param>
        /// <returns></returns>
        public async Task AnnounceAsync(Metafile meta, AnnounceEvent ev) {

            if (meta == null) throw new ArgumentNullException(nameof(meta));

            if (meta.InfoHash == null) throw new ArgumentNullException(nameof(meta.InfoHash));

            var infohashUrl = HttpUtility.UrlEncode(meta.InfoHash);
            var peerIdUrl = HttpUtility.UrlEncode(_torrentClient.PeerId);

            var builder = new UriBuilder(Uri);

            var query = HttpUtility.ParseQueryString("");
            query.Add("info_hash", infohashUrl);
            query.Add("peer_id", peerIdUrl);
            query.Add("port", _torrentClient.ListenPort.ToString());
            query.Add("uploaded", meta.Uploaded.ToString());
            query.Add("downloaded", meta.Downloaded.ToString());
            query.Add("left", meta.Left.ToString());
            query.Add("compact", "1");
            query.Add("event", ev.ToString().ToLower());
            // Add tracker ID if we have it
            if (!string.IsNullOrEmpty(_trackerId)) {
                query.Add("trackerid", _trackerId);
            }
            // Get compact response
            var url = Uri + "?" + HttpUtility.UrlDecode(query.ToString());

            // TODO: Handle errors
            // Get announce response from tracker
            var response = await _httpClient.GetByteArrayAsync(url);

            // Parse reponse
            var respReader = new BencodeReader(response);
            var resp = respReader.Read() as BencodeDictionary;

            if (resp == null) {
                throw new InvalidDataException("Couldn't parse tracker response");
            }

            // Get possible failure reason
            var failReason = resp.Get<BencodeByteString>("failure reason");
            if (failReason != null) {
                // Failed
                Debug.WriteLine($"Tracker announce failed: {failReason}");
                // TODO: Don't throw exception, handle better
                throw new Exception($"Tracker announce failed: {failReason}");
            }

            // Get possible warning message
            var warnMsg = resp.Get<BencodeByteString>("warning message");
            if (warnMsg != null) {
                Debug.WriteLine($"Tracker warning: {warnMsg}");
            }

            // Get tracker id and save it
            var newTrackerId = resp.Get<BencodeByteString>("tracker id");
            if (newTrackerId != null) {
                _trackerId = newTrackerId;
            }

            // Get interval(s)
            Interval = resp.Get<BencodeInteger>("interval");
            var newMinInterval = resp.Get<BencodeInteger>("min interval");
            MinInterval = newMinInterval ?? Interval;

            // Get (in)complete numbers
            var complete = resp.Get<BencodeInteger>("complete");
            var incomplete = resp.Get<BencodeInteger>("incomplete");

            // TODO: Read IPv6 peers
            var rawPeers = resp.Get<BencodeByteString>("peers");
            var peerReader = new BinaryReader(new MemoryStream(rawPeers));
            var peerBytes = ((byte[])rawPeers).Length;
            if (peerBytes % 6 != 0) {
                // Invalid peer data
                throw new InvalidDataException("Compact peer data was not in multiple of 6");
            }
            var peerCount = peerBytes / 6;

            var peers = new List<Peer>();
            for (var i = 0; i < peerCount; i++) {
                var ip = new IPAddress(peerReader.ReadInt32());
                var port = peerReader.ReadInt16BE();
                peers.Add(new Peer(ip, port));
            }

        }

    }

}
