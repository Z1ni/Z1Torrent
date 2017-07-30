using System;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class BitfieldMessage : IMessage {

        public int Id => 5;

        public byte[] Bitfield { get; private set; }

        public byte[] Pack() {
            throw new NotImplementedException();
        }

        public void Unpack(byte[] data) {
            Bitfield = data;
        }
    }

}
