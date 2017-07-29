using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent {

    public class Config : IConfig {

        private const string ClientVersion = "0001";

        public byte[] PeerId { get; }
        public short ListenPort { get; }

        public Config() {
            // Generate peer ID
            var strPeerId = $"-Z1{ClientVersion}-";
            const string chars = "0123456789";
            var rnd = new Random();
            for (var i = 0; i < 12; i++) {
                strPeerId += chars[rnd.Next(chars.Length)];
            }
            PeerId = Encoding.ASCII.GetBytes(strPeerId);

            // Select listen port
            // TODO: Select a free port
            ListenPort = 6881;
        }

    }

}
