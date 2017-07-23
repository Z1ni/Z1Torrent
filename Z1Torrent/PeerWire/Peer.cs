using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Z1Torrent.PeerWire {

    public class Peer : IPeer {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public byte[] PeerId { get; }
        public IPAddress Address { get; }
        public short Port { get; }

        public bool AmChoking { get; private set; }
        public bool AmInterested { get; private set; }
        public bool PeerChoking { get; private set; }
        public bool PeerInterested { get; private set; }

        private ITorrentClient _client;
        private IMetafile _metafile;
        private IPeerConnection _connection;
        private ManualResetEvent _mre;
        private Thread _messageThread;

        public Peer(ITorrentClient client, IMetafile meta, IPAddress address, short port) {
            Address = address;
            Port = port;
            _client = client;
            _metafile = meta;
            // Initial status
            AmChoking = true;
            AmInterested = false;
            PeerChoking = true;
            PeerInterested = false;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public override bool Equals(object obj) {
            if (!(obj is Peer)) return false;
            var peer = (Peer)obj;

            var peerIdEq = false;
            if (PeerId == null && peer.PeerId == null) {
                peerIdEq = true;
            } else if ((PeerId != null && peer.PeerId == null) || (PeerId == null && peer.PeerId != null)) {
                peerIdEq = false;
            } else {
                peerIdEq = PeerId.SequenceEqual(peer.PeerId);
            }

            return (
                peerIdEq &&
                Address.Equals(peer.Address) &&
                Port == peer.Port
            );
        }

        public override int GetHashCode() {
            unchecked {
                var hash = 17;
                hash = hash * 23 + PeerId.GetHashCode();
                hash = hash * 23 + Address.GetHashCode();
                hash = hash * 23 + Port.GetHashCode();
                return hash;
            }
        }

        public void Dispose() {
            StopMessageLoop();
        }

        public override string ToString() {
            return $"[peer: {Address}:{Port}]";
        }

        public async Task StartMessageLoopAsync() {
            Log.Debug($"Starting message thread for {this}");
            _mre = new ManualResetEvent(false);
            _connection = new PeerConnection(_client, _metafile, this);
            await _connection.ConnectAsync();
            // Create message thread
            _messageThread = new Thread(MessageLoop) {
                Name = $"peer-{Address}-{Port}"
            };
            _messageThread.Start();
        }

        public void StopMessageLoop() {
            Log.Debug($"Stopping {this} message thread");
            _mre.Set();
            _messageThread.Join(3000);
        }

        private void MessageLoop() {

            while (_mre.WaitOne(50)) {
                // TODO: Message logic
                // TODO: Read messages
            }

            Log.Debug($"Message loop for {this} stopped");
        }
    }

}
