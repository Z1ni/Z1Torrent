using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Z1Torrent.PeerWire {

    public class PeerConnection {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private TorrentClient _torrentClient;
        private Metafile _meta;
        private Peer _peer;
        private IPEndPoint _endpoint;
        private Socket _socket;

        private Queue<byte> _dataBuffer;

        private bool _handshakeReceived = false;
        private bool _handshakeSent = false;

        public PeerConnection(TorrentClient torrentClient, Metafile meta, Peer peer) {
            _torrentClient = torrentClient;
            _meta = meta;
            _peer = peer;
            _endpoint = new IPEndPoint(_peer.Address, _peer.Port);
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _dataBuffer = new Queue<byte>();
        }

        public async Task ConnectAsync() {
            Log.Debug($"Connecting to {_peer}");
            await _socket.ConnectTaskAsync(_endpoint);
            // Send a handshake
            var ownHandshake = new HandshakeMessage(_meta.InfoHash, _torrentClient.PeerId);
            await SendMessageAsync(ownHandshake);
            // Receive a handshake
            var theirHandshake = await ReceiveMessageAsync<HandshakeMessage>();
            Log.Debug($"Got handshake from {_peer}, reserved bytes: {BitConverter.ToString(theirHandshake.Reserved)}");
        }

        public async Task DisconnectAsync() {
            Log.Debug($"Disconnecting from {_peer}");
            await _socket.DisconnectTaskAsync();
            // Close socket
            // TODO: Make this better?
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public async Task SendMessageAsync(IMessage message) {
            var packed = message.Pack();
            // TODO: Handle partial sends?
            var sentBytes = await _socket.SendTaskAsync(packed, 0, packed.Length, SocketFlags.None);
            Log.Trace($"Sent {sentBytes} bytes to {_peer}");
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
            // TODO: Don't receive if buffer has data
            await ReceiveToBufferAsync();

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
            }

            // TODO: Parse more messages

            return msg;
        }

        private async Task ReceiveToBufferAsync() {
            var buffer = new byte[2048];
            var received = await _socket.ReceiveTaskAsync(buffer, 0, 2048, SocketFlags.None);
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
