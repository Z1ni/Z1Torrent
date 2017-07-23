using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.PeerWire;

namespace Z1Torrent.Test {

    public class PeerConnectionTest {

        private TorrentClient _torrentClient = new TorrentClient();
        private Metafile _torrentFile;

        public PeerConnectionTest() {
            _torrentFile = new Metafile(_torrentClient, @"TestData\debian-9.0.0-amd64-netinst.iso.torrent");
        }

        [Fact]
        public async Task Connection_HandshakeSuccessfulAsync() {
            // TOOD: Mock peer connection
            //var conn = new PeerConnection(_torrentClient, _torrentFile, new Peer(_torrentClient, _torrentFile, IPAddress.Loopback, 8999));
            //await conn.ConnectAsync();
        }

    }

}
