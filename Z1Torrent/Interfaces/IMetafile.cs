using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;
using Z1Torrent.Tracker;

namespace Z1Torrent.Interfaces {

    public interface IMetafile {

        string Title { get; }
        long Size { get; }
        byte[] InfoHash { get; }
        long Uploaded { get; }
        long Downloaded { get; }
        long Left { get; }
        bool Private { get; }
        DateTimeOffset? CreatedAt { get; }
        string Comment { get; }
        string CreatedBy { get; }
        List<ITracker> Trackers { get; }
        int PieceLength { get; }
        List<Piece> Pieces { get; }
        List<File> Files { get; }
        List<IPeer> Peers { get; }

        void AddPeers(IEnumerable<IPeer> peers);

    }

}
