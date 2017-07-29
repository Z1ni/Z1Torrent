using NLog;
using System.Collections.Generic;
using Z1Torrent.Interfaces;

namespace Z1Torrent {

    public class TorrentClient : ITorrentClient {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public List<IMetafile> ManagedTorrents { get; private set; }

        private IMetafileFactory _metafileFactory;

        public TorrentClient(IMetafileFactory metafileFactory) {
            _metafileFactory = metafileFactory;
            // Initialize
            ManagedTorrents = new List<IMetafile>();
        }

        public void ManageTorrent(Metafile meta) {
            if (ManagedTorrents.Contains(meta)) return;
            ManagedTorrents.Add(meta);
        }

        public IMetafile ManageFromFile(string path) {
            // Read metafile
            var meta = _metafileFactory.CreateMetafileFromFile(path);
            if (ManagedTorrents.Contains(meta)) return null;
            ManagedTorrents.Add(meta);
            return meta;
        }

        public void Dispose() {
            Log.Debug("Disposing TorrentClient");
            // Close peer connections
            // Announce stopping for all managed torrents
            foreach (var torrent in ManagedTorrents) {
                foreach (var peer in torrent.Peers) {
                    peer.StopMessageLoop();
                }
                /*foreach (var tracker in torrent.Trackers) {
                    if (tracker.IsAnnounced) {
                        tracker.AnnounceAsync(torrent, AnnounceEvent.Stopped).GetAwaiter().GetResult();
                    }
                }*/
            }
        }
    }

}
