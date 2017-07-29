﻿using System;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire {

    public class BitfieldMessage : IMessage {

        public byte[] Bitfield { get; private set; }

        public byte[] Pack() {
            throw new NotImplementedException();
        }

        public void Unpack(byte[] data) {
            Bitfield = data;
        }
    }

}
