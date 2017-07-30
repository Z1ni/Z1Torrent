using System;
using System.IO;
using System.Linq;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.Messages {

    public class RequestMessage : IMessage {

        public int Id => 6;

        public int Index { get; private set; }
        public int Begin { get; private set; }
        public int Length { get; private set; }

        public RequestMessage() { }

        public RequestMessage(int index, int begin, int length) {
            Index = index;
            Begin = begin;
            Length = length;
        }

        public byte[] Pack() {
            // TODO: Optimize
            var data = new byte[12];
            var writer = new BinaryWriter(new MemoryStream(data));
            writer.Write(BitConverter.GetBytes(Index).Reverse().ToArray());
            writer.Write(BitConverter.GetBytes(Begin).Reverse().ToArray());
            writer.Write(BitConverter.GetBytes(Length).Reverse().ToArray());
            writer.Flush();
            return data;
        }

        public void Unpack(byte[] data) {
            var reader = new BinaryReader(new MemoryStream(data));
            Index = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            Begin = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            Length = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
        }
    }

}
