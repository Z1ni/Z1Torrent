using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;

namespace Z1Torrent.Test {

    public class PeerConnectionTest {

        private readonly IMetafile _torrentFile;
        private readonly IPeerConnectionFactory _peerConnFact;

        public PeerConnectionTest() {
            var mockClient = new Mock<ITcpClient>();
            // Mock TcpClient responses
            mockClient
                .Setup(s => s.ConnectAsync(IPAddress.Loopback, 12345))
                .Returns(Task.CompletedTask);
            mockClient
                .Setup(s => s.ReadBytesAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(10))
                .Callback<byte[], int, int>((buf, offset, count) => {
                    // TODO
                });

            var config = new Config();
            _peerConnFact = new PeerConnectionFactory(config, mockClient.Object);
            var httpTrackerFact = new HttpTrackerFactory(config, _peerConnFact);
            var metafileFact = new MetafileFactory(httpTrackerFact);

            _torrentFile = metafileFact.CreateMetafileFromFile(@"TestData\debian-9.0.0-amd64-netinst.iso.torrent");
        }

        [Fact]
        public async Task Connection_HandshakeSuccessfulAsync() {
            // TODO
            var peerConn = _peerConnFact.CreatePeerConnection(_torrentFile, new Peer(_peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            // Setup mock
            /*_peerConnection
                .Setup(c => ConnectAsync)*/

            //await _peerConnection.Object.ConnectAsync();
        }

    }

}
