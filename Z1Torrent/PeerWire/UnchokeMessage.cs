using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class UnchokeMessage : IMessage {

        public int Id => 1;

        public byte[] Pack() {
            // Unchoke has no payload
            return new byte[0];
        }

        public void Unpack(byte[] data) {
            // Unchoke has no payload
        }

    }

}
