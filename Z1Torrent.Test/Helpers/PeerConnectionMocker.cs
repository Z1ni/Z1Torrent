using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;

namespace Z1Torrent.Test.Helpers {

    public static class PeerConnectionMocker {

        public static IPeerConnectionFactory CreatePeerConnectionFactoryWithResponse(byte[] response) {
            var config = new Config();
            var mockClient = new Mock<ITcpClient>();
            // Mock TcpClient responses
            mockClient
                .Setup(s => s.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockClient
                .Setup(s => s.ReadBytesAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(response.Length))
                .Callback<byte[], int, int>((buf, offset, count) => {
                    Array.Copy(response, buf, response.Length);
                });
            return new PeerConnectionFactory(config, mockClient.Object);
        }

    }

}
