using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class NotInterestedMessage : IMessage {

        public int Id => 3;

        public byte[] Pack() {
            // Not interested has no payload
            return new byte[0];
        }

        public void Unpack(byte[] data) {
            // Not interested has no payload
        }

    }

}
