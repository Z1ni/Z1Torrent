using System.Net;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.Factories {

    public class PeerFactory : IPeerFactory {

        private IPeerConnectionFactory _peerConnFactory;

        public PeerFactory(IPeerConnectionFactory peerConnFactory) {
            _peerConnFactory = peerConnFactory;
        }

        public IPeer CreatePeer(IMetafile meta, IPAddress address, short port) {
            return new Peer(_peerConnFactory, meta, address, port);
        }

    }

}
