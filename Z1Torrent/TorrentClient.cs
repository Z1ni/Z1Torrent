using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Z1Torrent.Tracker;

namespace Z1Torrent {

    public class TorrentClient : IDisposable {

        private const string ClientVersion = "0001";

        public byte[] PeerId { get; }
        public short ListenPort { get; }

        public List<Metafile> ManagedTorrents { get; private set; }

        public TorrentClient() {
            // Initialize
            // Generate peer ID
            var strPeerId = $"-Z1{ClientVersion}-";
            const string chars = "0123456789";
            var rnd = new Random();
            for (var i = 0; i < 12; i++) {
                strPeerId += chars[rnd.Next(chars.Length)];
            }
            PeerId = Encoding.ASCII.GetBytes(strPeerId);

            // Select listen port
            // TODO: Select a free port
            ListenPort = 6881;

            ManagedTorrents = new List<Metafile>();
        }

        public void ManageTorrent(Metafile meta) {
            if (ManagedTorrents.Contains(meta)) return;
            ManagedTorrents.Add(meta);
        }

        public void Dispose() {
            // Announce stopping for all managed torrents
            foreach (var torrent in ManagedTorrents) {
                foreach (var tracker in torrent.Trackers) {
                    if (tracker.IsAnnounced) {
                        tracker.AnnounceAsync(torrent, AnnounceEvent.Stopped).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }

}
