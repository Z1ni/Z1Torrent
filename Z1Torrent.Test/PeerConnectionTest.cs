using System;
using System.Collections.Generic;
using Moq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;
using Z1Torrent.PeerWire.Messages;
using Z1Torrent.Test.Helpers;

namespace Z1Torrent.Test {

    public class PeerConnectionTest {

        private readonly IConfig _config;
        private readonly IMetafile _torrentFile;

        private static byte[] _validHandshakeResponse = {
            0x13, 0x42, 0x69, 0x74, 0x54, 0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20,
            0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x10, 0x00, 0x05, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB,
            0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F,
            0x2D, 0x71, 0x42, 0x33, 0x33, 0x42, 0x30, 0x2D, 0x47, 0x44, 0x2E, 0x29,
            0x2D, 0x6E, 0x47, 0x6A, 0x28, 0x79, 0x66, 0x67
        };
        private static byte[] _validHandshakeResponseNoExtMsg = {
            0x13, 0x42, 0x69, 0x74, 0x54, 0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20,
            0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x05, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB,
            0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F,
            0x2D, 0x71, 0x42, 0x33, 0x33, 0x42, 0x30, 0x2D, 0x47, 0x44, 0x2E, 0x29,
            0x2D, 0x6E, 0x47, 0x6A, 0x28, 0x79, 0x66, 0x67
        };

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
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponse(_validHandshakeResponse);
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

        [Fact]
        public async Task ReceiveMessageAsync_ChokeReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,    // Handshake
                new byte[] {                // Choke
                    0x00, 0x00, 0x00, 0x01, 0x00
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            var msg = await peerConn.ReceiveMessageAsync();
            Assert.IsType<ChokeMessage>(msg);
        }

        [Fact]
        public async Task ReceiveMessageAsync_UnchokeReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,    // Handshake
                new byte[] {                // Unchoke
                    0x00, 0x00, 0x00, 0x01, 0x01
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            var msg = await peerConn.ReceiveMessageAsync();
            Assert.IsType<UnchokeMessage>(msg);
        }

        [Fact]
        public async Task ReceiveMessageAsync_InterestedReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,    // Handshake
                new byte[] {                // Interested
                    0x00, 0x00, 0x00, 0x01, 0x02
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            var msg = await peerConn.ReceiveMessageAsync();
            Assert.IsType<InterestedMessage>(msg);
        }

        [Fact]
        public async Task ReceiveMessageAsync_NotInterestedReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,    // Handshake
                new byte[] {                // Not interested
                    0x00, 0x00, 0x00, 0x01, 0x03
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            var msg = await peerConn.ReceiveMessageAsync();
            Assert.IsType<NotInterestedMessage>(msg);
        }

        [Fact]
        public async Task ReceiveMessageAsync_HaveReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,    // Handshake
                new byte[] {                // Have, piece ID 1234567890 (0x499602D2)
                    0x00, 0x00, 0x00, 0x05, 0x04, 0x49, 0x96, 0x02, 0xD2
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            var msg = await peerConn.ReceiveMessageAsync();
            Assert.IsType<HaveMessage>(msg);
            var haveMsg = (HaveMessage)msg;
            Assert.Equal(1234567890, haveMsg.PieceIndex);
        }

        [Fact]
        public async Task ReceiveMessageAsync_ExtendedHandshakeReceivedAsync() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponses(new List<byte[]> {
                _validHandshakeResponse,
                /* Extended handshake
                 * p: 8999
                 * v: "qBittorrent v3.3.1"
                 * yourip: 127.0.0.1
                 * reqq: 500
                 */
                new byte[] {
                    0x00, 0x00, 0x00, 0xCE, 0x14,
                    0x00, 0x64, 0x31, 0x32, 0x3A, 0x63, 0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x5F, 0x61, 0x67,
                    0x6F, 0x69, 0x34, 0x35, 0x65, 0x31, 0x3A, 0x6D, 0x64, 0x31, 0x31, 0x3A, 0x6C, 0x74, 0x5F, 0x64,
                    0x6F, 0x6E, 0x74, 0x68, 0x61, 0x76, 0x65, 0x69, 0x37, 0x65, 0x31, 0x30, 0x3A, 0x73, 0x68, 0x61,
                    0x72, 0x65, 0x5F, 0x6D, 0x6F, 0x64, 0x65, 0x69, 0x38, 0x65, 0x31, 0x31, 0x3A, 0x75, 0x70, 0x6C,
                    0x6F, 0x61, 0x64, 0x5F, 0x6F, 0x6E, 0x6C, 0x79, 0x69, 0x33, 0x65, 0x31, 0x32, 0x3A, 0x75, 0x74,
                    0x5F, 0x68, 0x6F, 0x6C, 0x65, 0x70, 0x75, 0x6E, 0x63, 0x68, 0x69, 0x34, 0x65, 0x31, 0x31, 0x3A,
                    0x75, 0x74, 0x5F, 0x6D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x69, 0x32, 0x65, 0x36, 0x3A,
                    0x75, 0x74, 0x5F, 0x70, 0x65, 0x78, 0x69, 0x31, 0x65, 0x65, 0x31, 0x33, 0x3A, 0x6D, 0x65, 0x74,
                    0x61, 0x64, 0x61, 0x74, 0x61, 0x5F, 0x73, 0x69, 0x7A, 0x65, 0x69, 0x32, 0x33, 0x32, 0x39, 0x37,
                    0x65, 0x31, 0x3A, 0x70, 0x69, 0x38, 0x39, 0x39, 0x39, 0x65, 0x34, 0x3A, 0x72, 0x65, 0x71, 0x71,
                    0x69, 0x35, 0x30, 0x30, 0x65, 0x31, 0x3A, 0x76, 0x31, 0x39, 0x3A, 0x71, 0x42, 0x69, 0x74, 0x74,
                    0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20, 0x76, 0x33, 0x2E, 0x33, 0x2E, 0x31, 0x31, 0x36, 0x3A,
                    0x79, 0x6F, 0x75, 0x72, 0x69, 0x70, 0x34, 0x3A, 0x7F, 0x00, 0x00, 0x01, 0x65
                }
            });
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
            await peerConn.ReceiveMessageAsync();   // Receive extended handshake
            Assert.NotNull(peerConn.ExtendedPeerHandshake);
            var extHs = peerConn.ExtendedPeerHandshake;

            Assert.Equal("qBittorrent v3.3.11", extHs.ClientNameVersion);
            Assert.Equal(8999, extHs.LocalListenPort);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), extHs.YourIp);
            Assert.Equal(500, extHs.ReqQ);
        }

        [Fact]
        public async Task SendMessageAsync_HandshakeAsync() {
            var config = new Config();
            var expectedHandshakeData = new List<byte>();
            expectedHandshakeData.AddRange(new byte[] {
                0x13, 0x42, 0x69, 0x74, 0x54, 0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20,
                0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x00, 0x00, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB,
                0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F
            });
            expectedHandshakeData.AddRange(config.PeerId);

            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithSendValidation(
                config,
                // Expected sent bytes
                new List<byte[]> {
                    expectedHandshakeData.ToArray()
                },
                // Mocked responses
                new List<byte[]> {
                    _validHandshakeResponseNoExtMsg
                }
            );
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
        }

        [Fact]
        public async Task SendMessageAsync_ExtendedHandshakeAsync() {
            var config = new Config();

            // TODO: Make this nicer

            var expectedHandshakeData = new List<byte>();
            expectedHandshakeData.AddRange(new byte[] {
                0x13, 0x42, 0x69, 0x74, 0x54, 0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20,
                0x70, 0x72, 0x6F, 0x74, 0x6F, 0x63, 0x6F, 0x6C, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x00, 0x00, 0xD4, 0xAB, 0xEF, 0xDF, 0x19, 0xC5, 0xA9, 0xAB,
                0x73, 0xCE, 0xD3, 0x89, 0xFA, 0xCA, 0x97, 0xBD, 0xCB, 0xB2, 0xEF, 0x3F
            });
            expectedHandshakeData.AddRange(config.PeerId);
            var expectedExtHandshakeData = new List<byte>();
            expectedExtHandshakeData.AddRange(new byte[] {
                0x00, 0x00, 0x00, 0xFF, 0x14,
                0x00, 0x64, 0x31, 0x3A, 0x6D, 0x64, 0x65, 0x31, 0x3A, 0x76
            });
            expectedExtHandshakeData.AddRange(Encoding.UTF8.GetBytes(config.ClientNameVersion.Length.ToString()));
            expectedExtHandshakeData.Add((byte)':');
            expectedExtHandshakeData.AddRange(Encoding.UTF8.GetBytes(config.ClientNameVersion));
            expectedExtHandshakeData.Add(0x65);
            // Set length
            expectedExtHandshakeData[3] = (byte)(expectedExtHandshakeData.Count - 4);

            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithSendValidation(
                config,
                // Expected sent bytes
                new List<byte[]> {
                    expectedHandshakeData.ToArray(),
                    expectedExtHandshakeData.ToArray()
                },
                // Mocked responses
                new List<byte[]> {
                    _validHandshakeResponse,
                    new byte[] {
                        0x00, 0x00, 0x00, 0xCE, 0x14,
                        0x00, 0x64, 0x31, 0x32, 0x3A, 0x63, 0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x5F, 0x61, 0x67,
                        0x6F, 0x69, 0x34, 0x35, 0x65, 0x31, 0x3A, 0x6D, 0x64, 0x31, 0x31, 0x3A, 0x6C, 0x74, 0x5F, 0x64,
                        0x6F, 0x6E, 0x74, 0x68, 0x61, 0x76, 0x65, 0x69, 0x37, 0x65, 0x31, 0x30, 0x3A, 0x73, 0x68, 0x61,
                        0x72, 0x65, 0x5F, 0x6D, 0x6F, 0x64, 0x65, 0x69, 0x38, 0x65, 0x31, 0x31, 0x3A, 0x75, 0x70, 0x6C,
                        0x6F, 0x61, 0x64, 0x5F, 0x6F, 0x6E, 0x6C, 0x79, 0x69, 0x33, 0x65, 0x31, 0x32, 0x3A, 0x75, 0x74,
                        0x5F, 0x68, 0x6F, 0x6C, 0x65, 0x70, 0x75, 0x6E, 0x63, 0x68, 0x69, 0x34, 0x65, 0x31, 0x31, 0x3A,
                        0x75, 0x74, 0x5F, 0x6D, 0x65, 0x74, 0x61, 0x64, 0x61, 0x74, 0x61, 0x69, 0x32, 0x65, 0x36, 0x3A,
                        0x75, 0x74, 0x5F, 0x70, 0x65, 0x78, 0x69, 0x31, 0x65, 0x65, 0x31, 0x33, 0x3A, 0x6D, 0x65, 0x74,
                        0x61, 0x64, 0x61, 0x74, 0x61, 0x5F, 0x73, 0x69, 0x7A, 0x65, 0x69, 0x32, 0x33, 0x32, 0x39, 0x37,
                        0x65, 0x31, 0x3A, 0x70, 0x69, 0x38, 0x39, 0x39, 0x39, 0x65, 0x34, 0x3A, 0x72, 0x65, 0x71, 0x71,
                        0x69, 0x35, 0x30, 0x30, 0x65, 0x31, 0x3A, 0x76, 0x31, 0x39, 0x3A, 0x71, 0x42, 0x69, 0x74, 0x74,
                        0x6F, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x20, 0x76, 0x33, 0x2E, 0x33, 0x2E, 0x31, 0x31, 0x36, 0x3A,
                        0x79, 0x6F, 0x75, 0x72, 0x69, 0x70, 0x34, 0x3A, 0x7F, 0x00, 0x00, 0x01, 0x65
                    }
                }
            );
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));
            await peerConn.ConnectAsync();
        }

        [Fact]
        public async Task Connection_SendsEvent() {
            var peerConnFact = PeerConnectionMocker.CreatePeerConnectionFactoryWithResponse(_validHandshakeResponse);
            var peerConn = peerConnFact.CreatePeerConnection(_torrentFile, new Peer(peerConnFact, _torrentFile, IPAddress.Loopback, 12345));

            // Check that OnConnectionInitialized is raised
            await Assert.RaisesAsync<EventArgs>(
                h => peerConn.OnConnectionInitialized += h,
                h => peerConn.OnConnectionInitialized -= h,
                async () => await peerConn.ConnectAsync());
        }

    }

}
