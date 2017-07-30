using System;
using System.IO;
using System.Linq;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class PieceMessage : IMessage {

        public int Id => 7;

        public int Index { get; private set; }
        public int Begin { get; private set; }
        public byte[] Block { get; private set; }

        public PieceMessage() { }

        public PieceMessage(int index, int begin, byte[] block) {
            Index = index;
            Begin = begin;
            Block = block;
        }

        public byte[] Pack() {
            var data = new byte[8 + Block.Length];
            var writer = new BinaryWriter(new MemoryStream(data));
            writer.Write(BitConverter.GetBytes(Index).Reverse().ToArray());
            writer.Write(BitConverter.GetBytes(Begin).Reverse().ToArray());
            writer.Write(Block);
            return data;
        }

        public void Unpack(byte[] data) {
            var reader = new BinaryReader(new MemoryStream(data));
            Index = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            Begin = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            Block = reader.ReadBytes(data.Length - 8);
        }
    }

}
