using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Test.Helpers;
using Z1Torrent.Tracker;

namespace Z1Torrent.Test {

    public class HttpTrackerTest {

        private TorrentClient _client;

        public HttpTrackerTest() {
            _client = new TorrentClient();
        }

        [Fact]
        public async Task AnnounceAsync_ValidTorrent() {

            var torrent = Metafile.FromFile(_client, @"TestData\AdCouncil-Adoption-DangerDad-30_CLSD_archive.torrent");
            // Create a mock HttpClient
            var trackerHttpClient = new HttpClient(new ValidTrackerResponseHandler());
            // Create a mock tracker
            var tracker = new HttpTracker(_client, trackerHttpClient, torrent.Trackers.First().Uri.ToString());

            await tracker.AnnounceAsync(torrent, AnnounceEvent.Started);
        }

    }

}
