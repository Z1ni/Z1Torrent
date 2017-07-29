using System;
using Moq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;
using Z1Torrent.Test.Helpers;

namespace Z1Torrent.Test {

    public class PeerConnectionTest {

        private readonly IConfig _config;
        private readonly IMetafile _torrentFile;

        public PeerConnectionTest() {
            _config = new Config();
            var peerConnFact = new PeerConnectionFactory(_config, new TcpClientAdapter());
            var httpTrackerFact = new HttpTrackerFactory(_config, peerConnFact);
            var metafileFact = new MetafileFactory(httpTrackerFact);

            _torrentFile = metafileFact.CreateMetafileFromFile(@"TestData\debian-9.0.0-amd64-netinst.iso.torrent");
        }

        [Fact]
        public async Task Connection_HandshakeSuccessfulAsync() {
            // BitTorrent protocol
            // Reserved: 00 00 00 00 00 10 00 05
            // Infohash: D4ABEFDF19C5A9AB73CED389FACA97BDCBB2EF3F
            // Peer ID: "-qB33B0-GD.)-nGj(yfg" (qBittorrent 3.3.11)
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponse(new byte[] {
                0x13, 0x42, 0x69, 0x74, 0x54, 0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20,
                0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x00, 0x05, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB,
                0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F,
                0x2D, 0x71, 0x42, 0x33, 0x33, 0x42, 0x30, 0x2D, 0x47, 0x44, 0x2E, 0x29,
                0x2D, 0x6E, 0x47, 0x6A, 0x28, 0x79, 0x66, 0x67
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();

            var handshake = peerConn.PeerHandshake;
            Assert.Equal("BitTorrent protocol", handshake.Protocol);
            Assert.Equal(Encoding.ASCII.GetBytes("-qB33B0-GD.)-nGj(yfg"), handshake.PeerId);
            Assert.Equal(
                new byte[] {
                    0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB, 0x73, 0xCE, 0xD3, 0x89,
                    0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F
                },
                handshake.Infohash
            );
        }

        [Fact]
        public async Task Connection_UnsupportedProtocolAsync() {
            // Nonexisting protocol
            // Reserved: 00 00 00 00 00 00 00 00
            // Infohash: D4ABEFDF19C5A9AB73CED389FACA97BDCBB2EF3F
            // Peer ID: "-qB33B0-GD.)-nGj(yfg"
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponse(new byte[] {
                0x14, 0x4E, 0x6F, 0x6E, 0x65, 0x78, 0x69, 0x73, 0x74, 0x69, 0x6E, 0x67,
                0x20, 0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9,
                0xAB, 0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF,
                0x3F, 0x2D, 0x71, 0x42, 0x33, 0x33, 0x42, 0x30, 0x2D, 0x47, 0x44, 0x2E,
                0x29, 0x2D, 0x6E, 0x47, 0x6A, 0x28, 0x79, 0x66, 0x67
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await Assert.ThrowsAsync<InvalidMessageException>(async () => await peerConn.ConnectAsync());
        }

    }

}
