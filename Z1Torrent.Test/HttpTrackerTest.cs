﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.Test.Helpers;
using Z1Torrent.Tracker;

namespace Z1Torrent.Test {

    public class HttpTrackerTest {

        private TorrentClient _client;
        private Metafile _torrent;

        public HttpTrackerTest() {
            _client = new TorrentClient();
            _torrent = new Metafile(_client, @"TestData\AdCouncil-Adoption-DangerDad-30_CLSD_archive.torrent");
        }

        [Fact]
        public async Task AnnounceAsync_ValidTorrent() {
            // Mock tracker with given response
            var trackerHttpClient = new HttpClient(new ValidTrackerResponseHandler(new byte[] {
                0x64, 0x38, 0x3A, 0x63, 0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x69,
                0x31, 0x65, 0x31, 0x30, 0x3A, 0x64, 0x6F, 0x77, 0x6E, 0x6C, 0x6F, 0x61,
                0x64, 0x65, 0x64, 0x69, 0x30, 0x65, 0x31, 0x30, 0x3A, 0x69, 0x6E, 0x63,
                0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x69, 0x30, 0x65, 0x38, 0x3A,
                0x69, 0x6E, 0x74, 0x65, 0x72, 0x76, 0x61, 0x6C, 0x69, 0x31, 0x37, 0x34,
                0x37, 0x65, 0x31, 0x32, 0x3A, 0x6D, 0x69, 0x6E, 0x20, 0x69, 0x6E, 0x74,
                0x65, 0x72, 0x76, 0x61, 0x6C, 0x69, 0x38, 0x37, 0x33, 0x65, 0x35, 0x3A,
                0x70, 0x65, 0x65, 0x72, 0x73, 0x36, 0x3A, 0x80, 0x42, 0x00, 0x01, 0x1A,
                0xE1, 0x65
            }));
            var tracker = new HttpTracker(_client, trackerHttpClient, _torrent.Trackers.First().Uri.ToString());
            await tracker.AnnounceAsync(_torrent, AnnounceEvent.Started);

            Assert.Equal(1, _torrent.Peers.Count);
            Assert.Equal(IPAddress.Parse("128.66.0.1"), _torrent.Peers.First().Address);
        }

        [Fact]
        public async Task AnnounceAsync_ParseIPv6Peers() {
            var trackerHttpClient = new HttpClient(new ValidTrackerResponseHandler(new byte[] {
                0x64, 0x38, 0x3A, 0x63, 0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x69,
                0x31, 0x65, 0x31, 0x30, 0x3A, 0x64, 0x6F, 0x77, 0x6E, 0x6C, 0x6F, 0x61,
                0x64, 0x65, 0x64, 0x69, 0x30, 0x65, 0x31, 0x30, 0x3A, 0x69, 0x6E, 0x63,
                0x6F, 0x6D, 0x70, 0x6C, 0x65, 0x74, 0x65, 0x69, 0x30, 0x65, 0x38, 0x3A,
                0x69, 0x6E, 0x74, 0x65, 0x72, 0x76, 0x61, 0x6C, 0x69, 0x31, 0x37, 0x34,
                0x37, 0x65, 0x31, 0x32, 0x3A, 0x6D, 0x69, 0x6E, 0x20, 0x69, 0x6E, 0x74,
                0x65, 0x72, 0x76, 0x61, 0x6C, 0x69, 0x38, 0x37, 0x33, 0x65, 0x35, 0x3A,
                0x70, 0x65, 0x65, 0x72, 0x73, 0x36, 0x3A, 0x80, 0x42, 0x00, 0x01, 0x1A,
                0xE1, 0x36, 0x3A, 0x70, 0x65, 0x65, 0x72, 0x73, 0x36, 0x31, 0x38, 0x3A,
                0x20, 0x01, 0x0D, 0xB8, 0x01, 0x00, 0x00, 0x00, 0xB0, 0x85, 0x2F, 0x53,
                0xFE, 0xED, 0xAF, 0x6F, 0x1A, 0xE1, 0x65
            }));
            var tracker = new HttpTracker(_client, trackerHttpClient, _torrent.Trackers.First().Uri.ToString());
            await tracker.AnnounceAsync(_torrent, AnnounceEvent.Started);

            Assert.Equal(2, _torrent.Peers.Count);
            Assert.Equal(IPAddress.Parse("128.66.0.1"), _torrent.Peers.First().Address);
            Assert.Equal(IPAddress.Parse("2001:db8:100:0:b085:2f53:feed:af6f"), _torrent.Peers.Last().Address);
        }

    }

}
