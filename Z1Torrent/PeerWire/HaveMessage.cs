using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class HaveMessage : IMessage {

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
