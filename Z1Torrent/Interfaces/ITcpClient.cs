using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.Interfaces {

    /// <summary>
    /// Interface for TcpClient for mocking
    /// </summary>
    public interface ITcpClient : IDisposable {

        Task ConnectAsync(IPAddress address, int port);
        void Close();
        Task<int> ReadBytesAsync(byte[] buffer, int offset, int count);
        Task WriteBytesAsync(byte[] buffer, int offset, int count);

    }

}
