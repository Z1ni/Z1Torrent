using System;
using System.Text;
using Z1Torrent.Interfaces;

namespace Z1Torrent {

    public class Config : IConfig {

        private const string ClientName = "Z1Torrent";
        private const string Version = "0.0.1 alpha";

        private const string ClientVersion = "0001";    // TODO: Create this from version info

        public string ClientNameVersion => $"{ClientName} {Version}";
        public byte[] PeerId { get; }
        public short ListenPort { get; }

        public Config() {
            // Generate peer ID
            var strPeerId = $"-Z1{ClientVersion}-";
            const string chars = "0123456789abcdefghijklmnopqrstuvwABCDEFGHIJKLMNOPQRSTUVW";
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
