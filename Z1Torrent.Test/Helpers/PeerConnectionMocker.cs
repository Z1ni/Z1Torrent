using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;

namespace Z1Torrent.Test.Helpers {

    public static class PeerConnectionMocker {

        public static IPeerConnectionFactory CreatePeerConnectionFactoryWithResponse(byte[] response) {
            return CreatePeerConnectionFactoryWithResponses(new List<byte[]> { response });
        }

        public static IPeerConnectionFactory CreatePeerConnectionFactoryWithResponses(IEnumerable<byte[]> responses) {
            var config = new Config();
            var mockClient = new Mock<ITcpClient>();
            var respList = responses.ToList();
            var curRespIdx = 0;
            // Mock TcpClient responses
            mockClient
                .Setup(s => s.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            mockClient
                .Setup(s => s.ReadBytesAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => Task.FromResult(respList[curRespIdx].Length))
                .Callback<byte[], int, int>((buf, offset, count) => {
                    // Get the next response from the list
                    Array.Copy(respList[curRespIdx], buf, respList[curRespIdx].Length);
                    curRespIdx++;
                });
            return new PeerConnectionFactory(config, mockClient.Object);
        }

        public static IPeerConnectionFactory CreatePeerConnectionFactoryWithSendValidation(IConfig config, IEnumerable<byte[]> sentData, IEnumerable<byte[]> responses) {
            var mockClient = new Mock<ITcpClient>();
            var reqList = sentData.ToList();
            var respList = responses.ToList();
            var curReqIdx = 0;
            var curRespIdx = 0;

            // TODO: Don't assert in callback?
            mockClient
                .Setup(s => s.ConnectAsync(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            // Send
            mockClient
                .Setup(s => s.WriteBytesAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Callback<byte[], int, int>((buf, offset, length) => {
                    Assert.Equal(reqList[curReqIdx], buf);
                    curReqIdx++;
                });
            // Receive
            mockClient
                .Setup(s => s.ReadBytesAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => Task.FromResult(respList[curRespIdx].Length))
                .Callback<byte[], int, int>((buf, offset, count) => {
                    // Get the next response from the list
                    Array.Copy(respList[curRespIdx], buf, respList[curRespIdx].Length);
                    curRespIdx++;
                });
            return new PeerConnectionFactory(config, mockClient.Object);
        }

    }

}
