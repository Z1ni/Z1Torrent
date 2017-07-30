using System;
using System.Linq;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class HaveMessage : IMessage {

        public int Id => 4;

        public int PieceIndex { get; private set; }

        public HaveMessage() { }

        public HaveMessage(int pieceIndex) {
            PieceIndex = pieceIndex;
        }

        public byte[] Pack() {
            // Reversion because big endian
            return BitConverter.GetBytes(PieceIndex).Reverse().ToArray();
        }

        public void Unpack(byte[] data) {
            // Data contains big endian int32 piece index
            PieceIndex = BitConverter.ToInt32(data.Reverse().ToArray(), 0);
        }
    }

}
