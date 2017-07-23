﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent {

    public interface ITorrentClient : IDisposable {

        byte[] PeerId { get; }
        short ListenPort { get; }

        void ManageTorrent(Metafile meta);

    }

}
