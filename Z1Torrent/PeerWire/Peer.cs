using System.Linq;
using System.Net;

namespace Z1Torrent.PeerWire {

    public class Peer {

        public byte[] PeerId { get; }
        public IPAddress Address { get; }
        public short Port { get; }

        public Peer(IPAddress address, short port) {
            Address = address;
            Port = port;
        }

        public override bool Equals(object obj) {
            if (!(obj is Peer)) return false;
            var peer = (Peer)obj;

            return (
                PeerId.SequenceEqual(peer.PeerId) &&
                Address.Equals(peer.Address) &&
                Port == peer.Port
            );
        }

        public override int GetHashCode() {
            unchecked {
                var hash = 17;
                hash = hash * 23 + PeerId.GetHashCode();
                hash = hash * 23 + Address.GetHashCode();
                hash = hash * 23 + Port.GetHashCode();
                return hash;
            }
        }
    }

}
