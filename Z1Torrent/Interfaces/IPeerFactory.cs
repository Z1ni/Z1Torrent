using System.Net;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.Interfaces {

    public interface IPeerFactory {

        IPeer CreatePeer(IMetafile meta, IPAddress address, short port);

    }

}
