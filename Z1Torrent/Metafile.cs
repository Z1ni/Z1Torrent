﻿using BencodeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Z1Torrent.PeerWire;
using Z1Torrent.Tracker;

namespace Z1Torrent {

    public class Metafile {
        public string Title { get; internal set; }
        public long Size { get; internal set; }
        public byte[] InfoHash { get; internal set; }
        public long Uploaded { get; internal set; }
        public long Downloaded { get; internal set; }
        public long Left => Size - Downloaded;
        public bool Private { get; internal set; }

        public DateTimeOffset? CreatedAt { get; internal set; }
        public string Comment { get; internal set; }
        public string CreatedBy { get; internal set; }
        public List<ITracker> Trackers { get; internal set; }

        public int PieceLength { get; internal set; }
        public List<Piece> Pieces { get; internal set; }
        public List<File> Files { get; internal set; }

        private TorrentClient _client;

        private List<Peer> _peers = new List<Peer>();

        public static Metafile FromFile(TorrentClient client, string path) {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var metafileData = System.IO.File.ReadAllBytes(path);

            var reader = new BencodeReader(metafileData);
            var rootItem = reader.Read();

            if (rootItem == null) {
                throw new InvalidDataException("Invalid metafile");
            }
            if (rootItem.GetType() != typeof(BencodeDictionary)) {
                // Root item must be a dictionary
                throw new InvalidDataException("Metafile root item must be a dictionary");
            }
            var root = rootItem as BencodeDictionary;

            var newMetafile = new Metafile {_client = client ?? throw new ArgumentNullException(nameof(client))};
            
            // ReSharper disable once PossibleNullReferenceException
            var creationBitem = root.Get("creation date");
            if (creationBitem != null) {
                newMetafile.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(root.Get<BencodeInteger>("creation date"));
            } else {
                newMetafile.CreatedAt = null;
            }
            newMetafile.CreatedBy = root.Get<BencodeByteString>("created by");
            newMetafile.Comment = root.Get<BencodeByteString>("comment");
            newMetafile.Title = root.Get<BencodeByteString>("title");

            // Get tracker(s)
            var trackers = new List<ITracker>();

            // Check announce-list
            // announce-list is a list of list of strings
            var announceList = root.Get<BencodeList>("announce-list");
            if (announceList != null) {
                foreach (var list in announceList) {
                    if (list == null || announceList.GetType() != typeof(BencodeList) || list.GetType() != typeof(BencodeList)) {
                        Debug.WriteLine("announce-list format is invalid");
                        break;
                    }
                    var strList = list as BencodeList;
                    // ReSharper disable once PossibleNullReferenceException
                    var url = strList[0] as BencodeByteString;
                    // TODO: Check URL and tracker type (HTTP, UDP)
                    trackers.Add(new HttpTracker(client, url));
                }
            }
            // Check announce
            if (announceList == null || trackers.Count == 0) {
                // announce is only checked if announce-list doesn't exist
                var announce = root.Get<BencodeByteString>("announce");
                if (announce != null) {
                    // TODO: Check URL and tracker type (HTTP, UDP)
                    trackers.Add(new HttpTracker(client, announce));
                }
            }

            newMetafile.Trackers = trackers;

            // Get info
            var info = root.Get<BencodeDictionary>("info");
            if (info == null) {
                throw new InvalidDataException("Missing info dictionary");
            }

            var pieceLength = info.Get<BencodeInteger>("piece length");
            byte[] piecesRaw = info.Get<BencodeByteString>("pieces");

            if (pieceLength == null || piecesRaw == null) {
                // Invalid torrent, missing fields
                throw new InvalidDataException("Missing piece information");
            }
            if (piecesRaw.Length % 20 != 0) {
                // piece string must be a multiple of 20 bytes (SHA1 hash)
                throw new InvalidDataException("Invalid piece data");
            }
            var pieceStream = new MemoryStream(piecesRaw);
            var pieceHash = new byte[20];
            var pieces = new List<Piece>();
            while (pieceStream.Position < piecesRaw.Length) {
                pieceStream.Read(pieceHash, 0, 20);
                pieces.Add(new Piece(pieceHash));
            }

            newMetafile.PieceLength = pieceLength;
            newMetafile.Pieces = pieces;

            // Get optional private value
            var priv = info.Get<BencodeInteger>("private");
            if (priv == null || priv != 1) {
                newMetafile.Private = false;
            } else {
                newMetafile.Private = true;
            }

            // Get file information
            var files = new List<File>();

            var filesList = info.Get<BencodeList>("files");
            if (filesList == null) {
                // Single file mode
                var fileName = info.Get<BencodeByteString>("name");
                var fileSize = info.Get<BencodeInteger>("length");
                if (fileName == null || fileSize == null) {
                    throw new InvalidDataException("No files specified");
                }
                files.Add(new File(fileName, fileSize));
            } else {
                // Multiple file mode
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var bencodeItem in filesList) {
                    var fileEntry = (BencodeDictionary)bencodeItem;
                    var fileSize = fileEntry.Get<BencodeInteger>("length");
                    var filePath = fileEntry.Get<BencodeList>("path");
                    var fileName = filePath.Last() as BencodeByteString;
                    var strPath = "";
                    for (var i = 0; i < filePath.Count; i++) {
                        if (i != 0) strPath += @"\";
                        strPath += (BencodeByteString)filePath[i];
                    }
                    files.Add(new File(fileName, fileSize, strPath));
                }
            }
            newMetafile.Files = files;

            // Calculate total size
            var totalSize = files.Sum(f => f.Size);

            // Set piece sizes
            for (var i = 0; i < pieces.Count; i++) {
                if (i < pieces.Count - 1) {
                    pieces[i].Size = pieceLength;
                } else {
                    pieces[i].Size = totalSize - (pieces.Count - 1) * pieceLength;
                }
            }

            newMetafile.Size = totalSize;

            // Calculate info hash
            var writer = new BencodeWriter();
            writer.Write(info);
            using (var sha1 = new SHA1Managed()) {
                newMetafile.InfoHash = sha1.ComputeHash(writer.Bytes);
            }

            return newMetafile;
        }

        /// <summary>
        /// Add peers and discard duplicates
        /// </summary>
        /// <param name="peers"></param>
        public void AddPeers(IEnumerable<Peer> peers) {
            foreach (var peer in peers) {
                if (_peers.Contains(peer)) continue;
                _peers.Add(peer);
            }
        }

    }
}
