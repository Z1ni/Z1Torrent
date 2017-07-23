using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
