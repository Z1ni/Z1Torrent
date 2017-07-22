using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent {

    public class Piece {

        public long Number { get; internal set; }
        public long Size { get; internal set; }
        public byte[] Hash { get; internal set; }

        public Piece(byte[] hash) {
            Hash = hash;
        }

    }

}
