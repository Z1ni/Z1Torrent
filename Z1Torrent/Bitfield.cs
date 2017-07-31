using System;

namespace Z1Torrent {

    public class Bitfield {

        private byte[] _bitfield;
        private uint? _haveCount = null;

        public uint PieceCount { get; }
        public uint HavePieceCount {
            get {
                if (_haveCount == null) {
                    // Calculate piece count only when we don't have it cached
                    _haveCount = CalculatePopCount();
                }
                return _haveCount.GetValueOrDefault();
            }
        }

        public byte[] BitfieldData {
            get => _bitfield;
            set {
                if (value.Length * 8 < PieceCount) {
                    throw new ArgumentOutOfRangeException(nameof(value), @"Bitfield too small");
                }
                _bitfield = value;
                _haveCount = CalculatePopCount();
            }
        }

        public Bitfield(uint pieceCount) {
            PieceCount = pieceCount;
        }

        public Bitfield(uint pieceCount, byte[] bitfield) {
            if (bitfield.Length * 8 < pieceCount) {
                throw new ArgumentOutOfRangeException(nameof(bitfield), @"Bitfield too small");
            }
            PieceCount = pieceCount;
            BitfieldData = bitfield;
        }

        public bool HasPiece(int pieceIndex) {
            var byteIdx = pieceIndex % 8;
            var bitIdx = pieceIndex - (pieceIndex / 8) * 8;
            return (BitfieldData[byteIdx] >> bitIdx) == 1;
        }

        public void SetPieceStatus(int pieceIndex, bool got) {
            if (pieceIndex > PieceCount) {
                // Can't set status for nonexistent pieces
                throw new ArgumentOutOfRangeException(nameof(pieceIndex));
            }
            var byteIdx = pieceIndex % 8;
            var bitIdx = pieceIndex - (pieceIndex / 8) * 8;
            var x = (byte)(got ? 1 : 0);
            if (got) {
                _haveCount++;
            }
            else {
                _haveCount--;
            }
            BitfieldData[byteIdx] = (byte)(BitfieldData[byteIdx] & ~(1 << bitIdx) | (x << bitIdx));
        }

        private uint CalculatePopCount() {
            // This algorithm should be more efficient when a value is expected to have few nonzero bits,
            // as is the case with bitfields. See https://en.wikipedia.org/wiki/Hamming_weight
            uint totalCount = 0;
            uint count = 0;
            foreach (var b in _bitfield) {
                var tmp = b;
                for (count = 0; tmp > 0; count++) {
                    tmp &= (byte)(tmp - 1);
                }
                totalCount += count;
            }
            return totalCount;
        }
    }

}
