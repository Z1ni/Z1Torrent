using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class PeerConnection : IPeerConnection {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;
        private readonly IMetafile _meta;
        private readonly IPeer _peer;
        private readonly ITcpClient _tcpClient;

        private readonly Queue<byte> _dataBuffer;

        public HandshakeMessage PeerHandshake { get; private set; }
        private bool _handshakeReceived = false;
        private bool _handshakeSent = false;

        public PeerConnection(IConfig config, ITcpClient tcpClient, IMetafile meta, IPeer peer) {
            _config = config;
            _meta = meta;
            _peer = peer;
            _tcpClient = tcpClient;
            _dataBuffer = new Queue<byte>();
        }

        public async Task ConnectAsync() {
            Log.Debug($"Connecting to {_peer}");
            await _tcpClient.ConnectAsync(_peer.Address, _peer.Port);
            // Send a handshake
            var ownHandshake = new HandshakeMessage(_meta.InfoHash, _config.PeerId);
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
            // Add message length if this is not handshake
            var dataBuf = new List<byte>();
            if (message.GetType() != typeof(HandshakeMessage)) {
                if (message.GetType() == typeof(KeepAliveMessage)) {
                    // Length of keep-alive is 0
                    dataBuf.AddRange(new byte[4]);
                } else {
                    // Otherwise length is 1 (id field size) + payload size
                    dataBuf.AddRange(BitConverter.GetBytes(1 + packed.Length));
                    dataBuf.Add((byte)message.Id);
                }
            }
            dataBuf.AddRange(packed);
            // TODO: Handle partial sends?
            var dataToSend = dataBuf.ToArray();
            await _tcpClient.WriteBytesAsync(dataToSend, 0, dataToSend.Length);
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
            // TODO: Have this in loop to ensure that at least something has been received?
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
                Log.Trace(BitConverter.ToString(handshake).Replace("-", ""));
                msg = new HandshakeMessage();
                msg.Unpack(handshake);
                PeerHandshake = (HandshakeMessage)msg;
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
                Log.Debug($"Got keep-alive message from {_peer}");
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
            //stream.ReadTimeout = 100;   // Timeout data read after 100 milliseconds to perform other tasks
            var received = await _tcpClient.ReadBytesAsync(buffer, 0, 2048);
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
