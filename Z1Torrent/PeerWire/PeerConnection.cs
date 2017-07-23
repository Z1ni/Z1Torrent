using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire {

    public class PeerConnection {

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
            await _socket.ConnectTaskAsync(_endpoint);
            // Send a handshake
            var ownHandshake = new HandshakeMessage(_meta.InfoHash, _torrentClient.PeerId);
            await SendMessageAsync(ownHandshake);
            // Receive a handshake
            var theirHandshake = await ReceiveMessageAsync();
            Debug.WriteLine("Got handshake");
        }

        public async Task SendMessageAsync(IMessage message) {
            var packed = message.Pack();
            // TODO: Handle partial sends?
            await _socket.SendTaskAsync(packed, 0, packed.Length, SocketFlags.None);
        }

        public async Task<IMessage> ReceiveMessageAsync() {
            IMessage msg = null;
            
            // Receive data
            // TODO: Don't receive if buffer has data
            await ReceiveToBufferAsync();

            if (!_handshakeReceived) {
                // This message must be a handshake
                while (_dataBuffer.Count < 68) {
                    // 68 bytes is the length of the BitTorrent protocol handshake
                    await ReceiveToBufferAsync();
                }
                var handshake = GetFromReceiveBuffer(68);
                msg = new HandshakeMessage();
                msg.Unpack(handshake);
            }

            // TODO: Parse more messages

            return msg;
        }

        private async Task ReceiveToBufferAsync() {
            var buffer = new byte[2048];
            var received = await _socket.ReceiveTaskAsync(buffer, 0, 2048, SocketFlags.None);
            Debug.WriteLine($"Received {received} bytes");
            for (var i = 0; i < received; i++) {
                _dataBuffer.Enqueue(buffer[i]);
            }
        }

        private byte[] GetFromReceiveBuffer(int count) {
            var buf = new byte[count];
            for (var i = 0; i < count; i++) {
                buf[i] = _dataBuffer.Dequeue();
            }
            return buf;
        }

    }

}
