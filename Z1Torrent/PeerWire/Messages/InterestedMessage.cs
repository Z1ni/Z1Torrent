using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class InterestedMessage : IMessage {

        public int Id => 2;

        public byte[] Pack() {
            // Interested has no payload
            return new byte[0];
        }

        public void Unpack(byte[] data) {
            // Interested has no payload
        }

    }

}
