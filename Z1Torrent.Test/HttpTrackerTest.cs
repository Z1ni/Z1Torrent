using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
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
            var tracker = torrent.Trackers.First();

            // TODO: Mock tracker response
            //await tracker.AnnounceAsync(torrent, AnnounceEvent.Started);
        }

    }

}
