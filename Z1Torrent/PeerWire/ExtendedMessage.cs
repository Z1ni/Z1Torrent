using BencodeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Z1Torrent.PeerWire.ExtendedMessages;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class ExtendedMessage : IMessage {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public int Id => 20;

        public uint ExtendedMessageId { get; private set; }
        public IExtendedMessage ExtendedMessageObj { get; private set; }

        public ExtendedMessage() { }

        public ExtendedMessage(uint extMsgId, IExtendedMessage extMessage) {
            ExtendedMessageId = extMsgId;
            ExtendedMessageObj = extMessage;
        }

        public byte[] Pack() {
            var data = new List<byte>();
            data.Add((byte)ExtendedMessageId);          // Extended message ID
            data.AddRange(ExtendedMessageObj.Pack());   // Extended message payload
            return data.ToArray();
        }

        public void Unpack(byte[] data) {
            var reader = new BinaryReader(new MemoryStream(data));

            ExtendedMessageId = reader.ReadByte();
            Log.Trace($"Extended message ID: {ExtendedMessageId}");

            byte[] extMsgPayload = reader.ReadBytes(data.Length - 1);

            // Create IExtendedMessage from the payload
            switch (ExtendedMessageId) {
                case 0:
                    // Handshake
                    ExtendedMessageObj = new ExtendedHandshakeMessage();
                    ExtendedMessageObj.Unpack(extMsgPayload);
                    break;
            }
        }

    }

}
