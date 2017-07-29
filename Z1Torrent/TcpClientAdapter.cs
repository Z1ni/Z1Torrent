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

        public Task<int> ReadBytesAsync(byte[] buffer, int offset, int count) {
            var stream = _client.GetStream();
            return stream.ReadAsync(buffer, offset, count);
        }

        public Task WriteBytesAsync(byte[] buffer, int offset, int count) {
            var stream = _client.GetStream();
            return stream.WriteAsync(buffer, offset, count);
        }
    }

}
