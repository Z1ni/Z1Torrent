using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class KeepAliveMessage : IMessage {

        public byte[] Pack() {
            return new byte[0];
        }

        public void Unpack(byte[] data) {
            // Do nothing, as keep-alive message is just 4 null bytes
        }

    }

}
