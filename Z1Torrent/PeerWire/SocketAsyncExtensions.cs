using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire {

    public static class SocketAsyncExtensions {

        public static Task ConnectTaskAsync(this Socket socket, EndPoint endPoint) {
            return Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endPoint, null);
        }

        public static Task DisconnectTaskAsync(this Socket socket) {
            // Set arg1 to true to enable socket reuse
            return Task.Factory.FromAsync(socket.BeginDisconnect, socket.EndDisconnect, true, null);
        }

        public static Task<int> SendTaskAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags) {
            void Nop(IAsyncResult i) { }
            var ar = socket.BeginSend(buffer, offset, size, flags, Nop, socket);
            return Task.Factory.FromAsync(ar, socket.EndSend);
        }

        public static Task<int> ReceiveTaskAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags flags) {
            void Nop(IAsyncResult i) { }
            var ar = socket.BeginReceive(buffer, offset, size, flags, Nop, socket);
            return Task.Factory.FromAsync(ar, socket.EndReceive);
        }

    }

}
