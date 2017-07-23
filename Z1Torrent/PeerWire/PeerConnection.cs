﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Z1Torrent.PeerWire {

    public class PeerConnection : IPeerConnection {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private ITorrentClient _torrentClient;
        private IMetafile _meta;
        private IPeer _peer;
        private IPEndPoint _endpoint;
        private TcpClient _tcpClient;

        private Queue<byte> _dataBuffer;

        private bool _handshakeReceived = false;
        private bool _handshakeSent = false;

        public PeerConnection(ITorrentClient torrentClient, IMetafile meta, IPeer peer) {
            _torrentClient = torrentClient;
            _meta = meta;
            _peer = peer;
            _endpoint = new IPEndPoint(_peer.Address, _peer.Port);
            _tcpClient = new TcpClient(); // new TcpClient(_endpoint);
            _dataBuffer = new Queue<byte>();
        }

        public async Task ConnectAsync() {
            Log.Debug($"Connecting to {_peer}");
            await _tcpClient.ConnectAsync(_peer.Address, _peer.Port);
            // Send a handshake
            var ownHandshake = new HandshakeMessage(_meta.InfoHash, _torrentClient.PeerId);
            await SendMessageAsync(ownHandshake);
            // Receive a handshake
            var theirHandshake = await ReceiveMessageAsync<HandshakeMessage>();
            Log.Debug($"Got handshake from {_peer}, reserved bytes: {BitConverter.ToString(theirHandshake.Reserved)}");
        }

        public void Disconnect() {
            Log.Debug($"Disconnecting from {_peer}");
            _tcpClient.Close();
            _tcpClient.Dispose();
        }

        public async Task SendMessageAsync(IMessage message) {
            // Don't send if handshake is not send and this message is not handshake
            if (!_handshakeSent && message.GetType() != typeof(HandshakeMessage)) {
                Log.Warn($"Tried to send {message.GetType().Name} before sending handshake, denied");
                return;
            }
            var packed = message.Pack();
            // TODO: Handle partial sends?
            var stream = _tcpClient.GetStream();
            await stream.WriteAsync(packed, 0, packed.Length);
            Log.Trace($"Sent {packed.Length} bytes to {_peer}");
            if (message is HandshakeMessage) {
                _handshakeSent = true;
            }
        }

        public async Task<T> ReceiveMessageAsync<T>() where T : IMessage {
            return (T)await ReceiveMessageAsync();
        }

        public async Task<IMessage> ReceiveMessageAsync() {
            IMessage msg = null;
            
            // Receive data
            // Don't receive if buffer has data
            if (_dataBuffer.Count == 0) {
                await ReceiveToBufferAsync();
            }

            if (!_handshakeReceived) {
                // This message must be a handshake
                // TODO: Don't use potentially infinite loop here
                while (_dataBuffer.Count < 68) {
                    // 68 bytes is the length of the BitTorrent protocol handshake
                    await ReceiveToBufferAsync();
                }
                var handshake = GetFromReceiveBuffer(68);
                msg = new HandshakeMessage();
                msg.Unpack(handshake);
                _handshakeReceived = true;
                return msg;
            }

            // TODO: Parse more messages
            // Check that we have enough bytes buffered (4 is the size of the smallest message; keep-alive
            while (_dataBuffer.Count < 4) {
                // Receive more data
                Log.Trace("Need more data, message length is missing");
                await ReceiveToBufferAsync();
            }
            // Now we have at least some data
            // Read length
            var lenBytes = GetFromReceiveBuffer(4).Reverse().ToArray(); // Reverse because big endian
            var msgLen = BitConverter.ToInt32(lenBytes, 0);
            if (msgLen == 0) {
                // Keep-alive
                return new KeepAliveMessage();
            }

            while (_dataBuffer.Count < msgLen) {
                // Receive more data
                Log.Trace("Need more data, message ID/payload is missing");
                await ReceiveToBufferAsync();
            }

            // Now we have message ID and possible payload
            int msgId = GetFromReceiveBuffer(1)[0];
            var payload = GetFromReceiveBuffer(msgLen - 1);
            switch (msgId) {

                case 0:
                    // Choke message
                    Log.Debug($"Got choke message from {_peer}");
                    break;

                case 1:
                    // Unchoke
                    Log.Debug($"Got unchoke message from {_peer}");
                    break;

                case 2:
                    // Interested
                    Log.Debug($"Got interested message from {_peer}");
                    break;

                case 3:
                    // Not interested
                    Log.Debug($"Got not interested message from {_peer}");
                    break;

                case 4:
                    // Have
                    msg = new HaveMessage();
                    msg.Unpack(payload);
                    Log.Debug($"Got have message from {_peer}, peer has piece {((HaveMessage)msg).PieceIndex}");
                    break;

                case 5:
                    // Bitfield
                    Log.Debug($"Got bitfield message from {_peer}");
                    msg = new BitfieldMessage();;
                    msg.Unpack(payload);
                    break;

                case 6:
                    // Request
                    Log.Debug($"Got request message from {_peer}");
                    break;

                case 7:
                    // Piece
                    Log.Debug($"Got piece message from {_peer}");
                    break;

                case 8:
                    // Cancel
                    Log.Debug($"Got cancel message from {_peer}");
                    break;

                case 9:
                    // Port
                    Log.Debug($"Got port message from {_peer}");;
                    break;

            }

            return msg;
        }

        private async Task ReceiveToBufferAsync() {
            var buffer = new byte[2048];
            var stream = _tcpClient.GetStream();
            //stream.ReadTimeout = 100;   // Timeout data read after 100 milliseconds to perform other tasks
            var received = await stream.ReadAsync(buffer, 0, 2048);
            Log.Trace($"Received {received} bytes from {_peer}");
            for (var i = 0; i < received; i++) {
                _dataBuffer.Enqueue(buffer[i]);
            }
            Log.Trace($"Data buffer size: {_dataBuffer.Count} bytes");
        }

        private byte[] GetFromReceiveBuffer(int count) {
            var buf = new byte[count];
            for (var i = 0; i < count; i++) {
                buf[i] = _dataBuffer.Dequeue();
            }
            Log.Trace($"Data buffer size: {_dataBuffer.Count} bytes");
            return buf;
        }

    }

}
