using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire.Interfaces {

    public interface IExtendedMessage {

        int Id { get; }

        byte[] Pack();
        void Unpack(byte[] data);

    }

}
