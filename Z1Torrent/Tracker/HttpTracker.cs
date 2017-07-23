using BencodeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using NLog;
using Z1Torrent.PeerWire;

namespace Z1Torrent.Tracker {

    public class HttpTracker : ITracker {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Uri Uri { get; internal set; }
        public bool IsAnnounced { get; private set; }

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

        public HttpTracker(TorrentClient client, HttpClient httpClient, string url) {
            _torrentClient = client;
            Uri = new Uri(url);
            _httpClient = httpClient;
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
                Log.Warn($"Tracker announce failed: {failReason}");
                // TODO: Don't throw exception, handle better
                throw new Exception($"Tracker announce failed: {failReason}");
            }

            // Get possible warning message
            var warnMsg = resp.Get<BencodeByteString>("warning message");
            if (warnMsg != null) {
                Log.Warn($"Tracker warning: {warnMsg}");
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

            var peers = new List<Peer>();

            // Read IPv4 peers
            var rawPeers = resp.Get<BencodeByteString>("peers");
            if (rawPeers != null) {
                var peerReader = new BinaryReader(new MemoryStream(rawPeers));
                var peerBytes = ((byte[])rawPeers).Length;
                if (peerBytes % 6 != 0) {
                    // Invalid peer data
                    throw new InvalidDataException("Compact peer data was not in multiple of 6");
                }
                var peerCount = peerBytes / 6;

                for (var i = 0; i < peerCount; i++) {
                    var ip = new IPAddress(peerReader.ReadBytes(4));
                    var port = BitConverter.ToInt16(peerReader.ReadBytes(2).Reverse().ToArray(), 0);
                    peers.Add(new Peer(_torrentClient, meta, ip, port));
                }
            }

            // Read IPv6 peers
            var raw6Peers = resp.Get<BencodeByteString>("peers6");
            if (raw6Peers != null) {
                var peer6Reader = new BinaryReader(new MemoryStream(raw6Peers));
                var peer6Bytes = ((byte[])raw6Peers).Length;
                if (peer6Bytes % 18 != 0) {
                    // Invalid peer data
                    throw new InvalidDataException("Compact peer6 data was not in multiple of 18");
                }
                var peer6Count = peer6Bytes / 18;

                for (var i = 0; i < peer6Count; i++) {
                    var ip = new IPAddress(peer6Reader.ReadBytes(16));
                    var port = BitConverter.ToInt16(peer6Reader.ReadBytes(2).Reverse().ToArray(), 0);
                    peers.Add(new Peer(_torrentClient, meta, ip, port));
                }
            }

            meta.AddPeers(peers);

            IsAnnounced = true;
        }

    }

}
