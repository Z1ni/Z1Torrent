using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class BitfieldMessage : IMessage {

        public int Id => 5;

        public byte[] Bitfield { get; private set; }

        public BitfieldMessage() { }

        public BitfieldMessage(byte[] bitfield) {
            Bitfield = bitfield;
        }

        public byte[] Pack() {
            return Bitfield;
        }

        public void Unpack(byte[] data) {
            Bitfield = data;
        }
    }

}
