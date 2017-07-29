using System;

namespace Z1Torrent.Interfaces {

    public interface ITorrentClient : IDisposable {

        void ManageTorrent(Metafile meta);
        IMetafile ManageFromFile(string path);

    }

}
