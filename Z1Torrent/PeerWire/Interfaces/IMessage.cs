using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire.Interfaces {

    public class InvalidMessageException : Exception {
        public InvalidMessageException(string msg) : base(msg) { }
        public InvalidMessageException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public interface IMessage {

        byte[] Pack();
        void Unpack(byte[] data);
    }

}
