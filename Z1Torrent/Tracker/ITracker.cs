using System;
using System.Threading.Tasks;

namespace Z1Torrent.Tracker {

    public enum AnnounceEvent {
        Started,
        Stopped,
        Completed
    }

    public interface ITracker {

        Uri Uri { get; }
        bool IsAnnounced { get; }

        Task AnnounceAsync(Metafile meta, AnnounceEvent ev);

    }

}
