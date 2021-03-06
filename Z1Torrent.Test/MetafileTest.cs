﻿using System;
using System.IO;
using System.Linq;
using Xunit;
using Z1Torrent.Factories;
using Z1Torrent.Interfaces;

namespace Z1Torrent.Test {

    public class MetafileTest {

        private readonly IConfig _config = new Config();
        private IMetafileFactory _metafileFactory;

        public MetafileTest() {
            var peerConnFact = new PeerConnectionFactory(_config, new TcpClientAdapter());
            var httpTrackerFact = new HttpTrackerFactory(_config, peerConnFact);
            _metafileFactory = new MetafileFactory(httpTrackerFact);
        }

        [Fact]
        public void FromFile_ValidMetafile() {

            var mf = _metafileFactory.CreateMetafileFromFile(@"TestData\AdCouncil-Adoption-DangerDad-30_CLSD_archive.torrent");

            Assert.Equal("This content hosted at the Internet Archive at https://archive.org/details/AdCouncil-Adoption-DangerDad-30_CLSD\nFiles may have changed, which prevents torrents from downloading correctly or completely; please check for an updated torrent at https://archive.org/download/AdCouncil-Adoption-DangerDad-30_CLSD/AdCouncil-Adoption-DangerDad-30_CLSD_archive.torrent\nNote: retrieval usually requires a client that supports webseeding (GetRight style).\nNote: many Internet Archive torrents contain a 'pad file' directory. This directory and the files within it may be erased once retrieval completes.\nNote: the file AdCouncil-Adoption-DangerDad-30_CLSD_meta.xml contains metadata about this torrent's contents.", mf.Comment);
            Assert.Equal("ia_make_torrent", mf.CreatedBy);
            Assert.NotNull(mf.CreatedAt);
            Assert.Equal(1482440734, mf.CreatedAt.Value.ToUnixTimeSeconds());
            Assert.Equal(2, mf.Trackers.Count);
            Assert.Equal("http://bt1.archive.org:6969/announce", mf.Trackers.First().Uri.ToString());
            Assert.Equal("http://bt2.archive.org:6969/announce", mf.Trackers.Last().Uri.ToString());
            Assert.Equal(1, mf.Pieces.Count);
            Assert.Equal(new byte[] { 0x83, 0x72, 0x53, 0x2D, 0x36, 0x11, 0x50, 0xA8, 0x68, 0x52, 0x0E, 0xC8, 0x2E, 0x0F, 0x0C, 0x1C, 0x6B, 0xE2, 0x46, 0xEC }, mf.Pieces.First().Hash);
            Assert.Equal(2, mf.Files.Count);
        }

        [Fact]
        public void FromFile_ValidMetafile2() {
            var mf = _metafileFactory.CreateMetafileFromFile(@"TestData\debian-9.0.0-amd64-netinst.iso.torrent");
            // TODO
        }

        [Fact]
        public void FromFile_NullFilePath() {
            Assert.Throws<ArgumentNullException>(() => _metafileFactory.CreateMetafileFromFile(null));
        }

        [Fact]
        public void FromFile_NonExistent() {
            Assert.Throws<FileNotFoundException>(() => _metafileFactory.CreateMetafileFromFile(@"TestData\nonexistant-metainfo-file.torrent"));
        }

        [Fact]
        public void FromFile_EmptyFile() {
            Assert.Throws<InvalidDataException>(() => _metafileFactory.CreateMetafileFromFile(@"TestData\EmptyMetainfo.torrent"));
        }

        [Fact]
        public void FromFile_InvalidStructure() {
            Assert.Throws<InvalidDataException>(() => _metafileFactory.CreateMetafileFromFile(@"TestData\InvalidStructure.torrent"));
        }

        [Fact]
        public void FromFile_InfohashCorrect() {
            var mf = _metafileFactory.CreateMetafileFromFile(@"TestData\AdCouncil-Adoption-DangerDad-30_CLSD_archive.torrent");
            byte[] infoHash = {
                0x00, 0xD7, 0x80, 0x6B, 0x08, 0x93, 0xA6, 0x9D, 0x36, 0xFB, 0x56, 0x26,
                0x6B, 0x57, 0x2A, 0x47, 0xC8, 0x71, 0xF1, 0x64
            };
            Assert.Equal(infoHash, mf.InfoHash);
        }

        [Fact]
        public void FromFile_InvalidAnnounceList() {
            var mf = _metafileFactory.CreateMetafileFromFile(@"TestData\InvalidAnnounceList.torrent");
            Assert.Equal(1, mf.Trackers.Count);
            Assert.Equal("http://tracker2.example.com/announce", mf.Trackers.First().Uri.ToString());
        }

        [Fact]
        public void FromFile_AnnounceListOverrides() {
            // announce-list must override announce if present
            var mf = _metafileFactory.CreateMetafileFromFile(@"TestData\AnnounceList.torrent");
            Assert.Equal(1, mf.Trackers.Count);
            Assert.Equal("http://tracker.example.com/announce", mf.Trackers.First().Uri.ToString());
        }
    }
}
