using System;
using System.Text;

namespace BencodeLib {

    public class BencodeByteString : IBencodeItem {

        private readonly byte[] _bytes;

        public BencodeByteString(string str) {
            _bytes = Encoding.UTF8.GetBytes(str);
        }

        public BencodeByteString(byte[] bytes) {
            _bytes = bytes;
        }

        public override string ToString() {
            var utfStr = "[no string repr]";
            try {
                utfStr = this;
            }
            catch (Exception) {
                // ignored
            }
            return $"BencodeByteString, {_bytes.Length} bytes: \"{utfStr}\"";
        }

        public static implicit operator byte[](BencodeByteString bs) {
            return bs?._bytes;
        }

        public static implicit operator string(BencodeByteString bs) {
            return bs == null ? null : Encoding.UTF8.GetString(bs._bytes, 0, bs._bytes.Length);
        }

    }
}
