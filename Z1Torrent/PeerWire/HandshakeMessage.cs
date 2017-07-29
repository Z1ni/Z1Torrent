using System;
using System.IO;
using System.Text;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class HandshakeMessage : IMessage {

        public int Id => 0; // Handshake message has no ID, so this doesn't matter

        public string Protocol { get; private set; }
        public byte[] Reserved { get; private set; }
        public byte[] Infohash { get; private set; }
        public byte[] PeerId { get; private set; }

        public HandshakeMessage() { }

        public HandshakeMessage(byte[] infohash, byte[] peerid) {
            Infohash = infohash;
            PeerId = peerid;
            Protocol = "BitTorrent protocol";
            Reserved = new byte[8];
        }

        public byte[] Pack() {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write((byte)Protocol.Length);
            writer.Write(Protocol.ToCharArray());  // Use char arrays because BinaryWriter.Write(string) has its own length prefix
            writer.Write(Reserved);
            writer.Write(Infohash);
            writer.Write(PeerId);
            writer.Flush();
            var arr = stream.ToArray();
            writer.Dispose();
            return arr;
        }

        public void Unpack(byte[] data) {
            var pstr = "";
            var reserved = new byte[8];
            var infoHash = new byte[20];
            var peerId = new byte[20];
            try {
                var reader = new BinaryReader(new MemoryStream(data));
                var pstrlen = reader.ReadByte();
                pstr = Encoding.UTF8.GetString(reader.ReadBytes(pstrlen));
                reserved = reader.ReadBytes(8);
                infoHash = reader.ReadBytes(20);
                peerId = reader.ReadBytes(20);
            } catch (Exception ex) {
                throw new InvalidMessageException("Message parsing failed", ex);
            }

            if (pstr != "BitTorrent protocol") {
                throw new InvalidMessageException($"Unsupported protocol: \"{pstr}\"");
            }

            Protocol = pstr;
            Reserved = reserved;
            Infohash = infoHash;
            PeerId = peerId;
        }

    }

}
