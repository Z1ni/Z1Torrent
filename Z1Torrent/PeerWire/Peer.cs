﻿using NLog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire.ExtendedMessages;
using Z1Torrent.PeerWire.Interfaces;
using Z1Torrent.PeerWire.Messages;

namespace Z1Torrent.PeerWire {

    public class Peer : IPeer {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public byte[] PeerId { get; private set; }
        public string ClientName { get; private set; }
        public IPAddress Address { get; }
        public short Port { get; }

        public bool AmChoking { get; private set; }
        public bool AmInterested { get; private set; }
        public bool PeerChoking { get; private set; }
        public bool PeerInterested { get; private set; }

        private IMetafile _metafile;
        private Bitfield _bitfield;

        private IPeerConnectionFactory _peerConnFactory;
        private IPeerConnection _connection;
        private ManualResetEvent _mre;
        private Thread _messageThread;

        // Pass through
        public event EventHandler<EventArgs> OnConnectionInitialized {
            add => _connection.OnConnectionInitialized += value;
            remove => _connection.OnConnectionInitialized -= value;
        }

        public event EventHandler<EventArgs> OnDisconnected;

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
            _bitfield = new Bitfield((uint)_metafile.Pieces.Count);
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
            _mre?.Set();
            _messageThread?.Join(3000);
            _connection?.Dispose();
        }

        private void MessageLoop() {

            _connection = _peerConnFactory.CreatePeerConnection(_metafile, this);
            try {
                _connection.ConnectAsync().GetAwaiter().GetResult();
            } catch (PeerConnectionException e) {
                // Peer connection was successful, but something else failed
                // Maybe the peer sent a wrong infohash
                Log.Warn(e, $"Connection to {this} was rejected");
                _connection.Dispose();
                // Fire an event
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            } catch (SocketException e) {
                // Connection failed
                Log.Warn(e, $"Connection to {this} failed");
                _connection.Dispose();
                // Fire an event
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Handshake has been completed
            PeerId = _connection.PeerHandshake.PeerId;
            // TODO: Try to parse peer client name from the peer id

            //_connection.SendMessageAsync(new InterestedMessage()).GetAwaiter().GetResult();

            // Keep-alive grace time is the minimum time to be waited after sending a keep-alive before it's possible to send another keep-alive
            var keepAliveGraceTimeStart = DateTime.Now;

            var wait = 0;

            while (!_mre.WaitOne(wait)) {

                // Check if we have to send keep-alive
                // TODO: Add intervals to config
                var now = DateTime.Now;
                if (now >= keepAliveGraceTimeStart + TimeSpan.FromSeconds(5) && now >= _connection.LastMessageSentTime + TimeSpan.FromMinutes(1)) {
                    // Send keep-alive
                    keepAliveGraceTimeStart = DateTime.Now;
                    Log.Trace($"Sending keep-alive to {this}");
                    try {
                        _connection.SendMessageAsync(new KeepAliveMessage()).GetAwaiter().GetResult();
                    } catch (IOException) {
                        // Send failed, connection should be closed
                        _connection.Disconnect();
                        Log.Debug($"Connection to {this} closed");
                        _mre.Set();
                        break;
                    }
                }

                // Read one received message
                wait = 0;
                IMessage msg = null;
                try {
                    msg = _connection.ReceiveMessageAsync().GetAwaiter().GetResult();
                } catch (EndOfStreamException) {
                    // Stream was closed, connection has been closed
                    Log.Debug($"Connection to {this} closed");
                    _mre.Set();
                    break;
                } catch (InactiveConnectionException) {
                    // Connection was deemed inactive and was closed
                    Log.Info($"Closed inactive connection to {this}");
                    _mre.Set();
                    break;
                }
                if (msg == null) {
                    // No message to read, wait 100ms and try again
                    wait = 100;
                    continue;
                }

                // TODO: Message logic
                switch (msg) {
                    case ChokeMessage _:
                        PeerChoking = true;
                        break;
                    case UnchokeMessage _:
                        PeerChoking = false;
                        break;
                    case InterestedMessage _:
                        PeerInterested = true;
                        break;
                    case NotInterestedMessage _:
                        PeerInterested = false;
                        break;

                    case BitfieldMessage b:
                        // TODO: Prevent multiple bitfield messages
                        _bitfield.BitfieldData = b.Bitfield;
                        Log.Debug($"Peer {this} has {_bitfield.HavePieceCount}/{_bitfield.PieceCount} pieces ({Math.Round((double)_bitfield.HavePieceCount / _bitfield.PieceCount * 100d, 2)} %)");
                        break;

                    case HaveMessage h:
                        // TODO: Check for non-initialized bitfield
                        _bitfield.SetPieceStatus(h.PieceIndex, true);
                        Log.Debug($"Peer {this} has {_bitfield.HavePieceCount}/{_bitfield.PieceCount} pieces ({Math.Round((double)_bitfield.HavePieceCount / _bitfield.PieceCount * 100d, 2)} %)");
                        break;

                    case ExtendedMessage m:
                        Log.Debug($"Got extended message {m.ExtendedMessageObj?.Id} from {this}");
                        switch (m.ExtendedMessageObj) {

                            case ExtendedHandshakeMessage e:
                                Log.Debug($"Got extended handshake message from {this}");
                                if (e.ClientNameVersion != null) {
                                    ClientName = e.ClientNameVersion;
                                    Log.Trace($"{this} ext: Client name: {ClientName}");
                                }
                                if (e.LocalListenPort != null) {
                                    // TODO: Save this data
                                    Log.Trace($"{this} ext: Local listen port: {e.LocalListenPort}");
                                }
                                if (e.YourIp != null) {
                                    // TODO: Save this data
                                    Log.Trace($"{this} ext: Your IP: {e.YourIp}");
                                }
                                if (e.MyIpv6 != null) {
                                    // TODO: Save this data
                                    Log.Trace($"{this} ext: My IPv6: {e.MyIpv6}");
                                }
                                if (e.MyIpv4 != null) {
                                    // TODO: Save this data
                                    Log.Trace($"{this} ext: My IPv4: {e.MyIpv4}");
                                }
                                if (e.ReqQ != null) {
                                    // TODO: Save this data
                                    Log.Trace($"{this} ext: ReqQ: {e.ReqQ}");
                                }
                                break;

                        }
                        break;
                }
            }

            Log.Debug($"Message loop for {this} stopped");

            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

}
