using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire {

    public class KeepAliveMessage : IMessage {

        public byte[] Pack() {
            return new byte[4];
        }

        public void Unpack(byte[] data) {
            // Do nothing, as keep-alive message is just 4 null bytes
        }

    }

}
