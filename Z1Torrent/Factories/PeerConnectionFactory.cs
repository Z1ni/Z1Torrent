using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.Factories {

    public class PeerConnectionFactory : IPeerConnectionFactory {

        //private ITorrentClient _torrentClient;
        private readonly IConfig _config;
        private readonly ITcpClient _tcpClient;

        public PeerConnectionFactory(IConfig config, ITcpClient tcpClient) {
            //_torrentClient = torrentClient;
            _config = config;
            _tcpClient = tcpClient;
        }

        public IPeerConnection CreatePeerConnection(IMetafile meta, IPeer peer) {
            return new PeerConnection(_config, _tcpClient, meta, peer);
        }

    }

}
