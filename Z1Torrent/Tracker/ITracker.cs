using System;
using System.Threading.Tasks;
using Z1Torrent.Interfaces;

namespace Z1Torrent.Tracker {

    public enum AnnounceEvent {
        Started,
        Stopped,
        Completed
    }

    public interface ITracker {

        Uri Uri { get; }
        bool IsAnnounced { get; }

        Task AnnounceAsync(IMetafile meta, AnnounceEvent ev);

    }

}
