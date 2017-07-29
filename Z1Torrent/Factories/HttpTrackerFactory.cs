using System.Net.Http;
using Z1Torrent.Interfaces;
using Z1Torrent.Tracker;

namespace Z1Torrent.Factories {

    public class HttpTrackerFactory : IHttpTrackerFactory {

        private readonly IConfig _config;
        private readonly IPeerConnectionFactory _peerConnFactory;

        public HttpTrackerFactory(IConfig config, IPeerConnectionFactory peerConnFactory) {
            _config = config;
            _peerConnFactory = peerConnFactory;
        }

        public HttpTracker CreateHttpTracker(string url) {
            return new HttpTracker(_config, _peerConnFactory, url);
        }

        public HttpTracker CreateHttpTrackerWithHttpClient(HttpClient httpClient, string url) {
            return new HttpTracker(_config, _peerConnFactory, httpClient, url);
        }
    }

}
