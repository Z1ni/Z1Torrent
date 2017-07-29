using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.Interfaces {

    public interface IPeerConnectionFactory {
        IPeerConnection CreatePeerConnection(IMetafile meta, IPeer peer);
    }

}
