﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire.Interfaces;

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

        private IMetafile _metafile;
        private IPeerConnectionFactory _peerConnFactory;
        private IPeerConnection _connection;
        private ManualResetEvent _mre;
        private Thread _messageThread;

        public Peer(IPeerConnectionFactory peerConnFactory, IMetafile meta, IPAddress address, short port) {
            Address = address;
            Port = port;
            _peerConnFactory = peerConnFactory;
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
            _mre.Dispose();
        }

        public override string ToString() {
            return $"[peer: {Address}:{Port}]";
        }

        public void StartMessageLoop() {
            Log.Debug($"Starting message thread for {this}");
            _mre = new ManualResetEvent(false);
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

            _connection = _peerConnFactory.CreatePeerConnection(_metafile, this);
            _connection.ConnectAsync().GetAwaiter().GetResult();

            // TODO: Send keep-alive messages

            while (!_mre.WaitOne(50)) {
                // TODO: Message logic
                // Read one received message
                var msg = _connection.ReceiveMessageAsync();
            }

            Log.Debug($"Message loop for {this} stopped");
        }
    }

}
