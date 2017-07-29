using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class ChokeMessage : IMessage {

        public int Id => 0;

        public byte[] Pack() {
            // Choke has no payload
            return new byte[0];
        }

        public void Unpack(byte[] data) {
            // Choke has no payload
        }

    }

}
