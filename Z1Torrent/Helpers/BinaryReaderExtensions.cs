using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.Helpers {

    public static class BinaryReaderExtensions {

        public static int ReadInt32BE(this BinaryReader br) {
            var leData = new byte[4];
            br.BaseStream.Read(leData, 0, 4);
            if (BitConverter.IsLittleEndian) Array.Reverse(leData);
            return BitConverter.ToInt32(leData, 0);
        }

        public static short ReadInt16BE(this BinaryReader br) {
            var leData = new byte[2];
            br.BaseStream.Read(leData, 0, 2);
            if (BitConverter.IsLittleEndian) Array.Reverse(leData);
            return BitConverter.ToInt16(leData, 0);
        }

    }

}
