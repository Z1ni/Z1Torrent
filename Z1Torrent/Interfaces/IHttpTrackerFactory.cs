using System.Net.Http;
using Z1Torrent.Tracker;

namespace Z1Torrent.Interfaces {

    public interface IHttpTrackerFactory {

        HttpTracker CreateHttpTracker(string url);
        HttpTracker CreateHttpTrackerWithHttpClient(HttpClient client, string url);

    }

}
