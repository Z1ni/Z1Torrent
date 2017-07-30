using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Z1Torrent.Interfaces;

namespace Z1Torrent {

    /// <summary>
    /// Wrapper for TcpClient to ease testing
    /// </summary>
    public class TcpClientAdapter : ITcpClient {

        private TcpClient _client;

        public TcpClientAdapter() {
            _client = new TcpClient();
        }

        public Task ConnectAsync(IPAddress address, int port) {
            return _client.ConnectAsync(address, port);
        }

        public void Close() {
            _client.Close();
        }

        public void Dispose() {
            _client.Dispose();
        }

        /*public NetworkStream GetStream() {
            return _client.GetStream();
        }*/

        /// <summary>
        /// Reads data from stream to the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer to write to</param>
        /// <param name="offset">Offset in given buffer</param>
        /// <param name="count">How many bytes to read from the stream</param>
        /// <returns>Modifies given buffer and returns amount of bytes read. Returns 0 on EOS and -1 on no available data</returns>
        public async Task<int> ReadBytesAsync(byte[] buffer, int offset, int count) {
            var stream = _client.GetStream();
            if (!stream.DataAvailable) {
                if (!_client.Connected) {
                    // The connection has been closed
                    return 0;
                }
                // If there's no data available, don't block
                return -1;
            }
            return await stream.ReadAsync(buffer, offset, count);
        }

        public async Task WriteBytesAsync(byte[] buffer, int offset, int count) {
            var stream = _client.GetStream();
            stream.WriteTimeout = 3000;
            await stream.WriteAsync(buffer, offset, count);
        }
    }

}
