namespace BencodeLib {

    public class BencodeInteger : IBencodeItem {

        private readonly long _value;

        public BencodeInteger(long value) {
            _value = value;
        }

        public static implicit operator int(BencodeInteger bencInt) {
            return (int)bencInt._value;
        }

        public static implicit operator long(BencodeInteger bencInt) {
            return bencInt._value;
        }

        public override string ToString() {
            return _value.ToString();
        }
    }
}
