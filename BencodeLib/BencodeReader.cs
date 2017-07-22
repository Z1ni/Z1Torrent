using System.Collections.Generic;
using System.IO;

namespace BencodeLib {

    public class BencodeReader {

        private readonly BinaryReader _binReader;

        public BencodeReader(byte[] data) {
            var dataStream = new MemoryStream(data) {
                Position = 0
            };
            _binReader = new BinaryReader(dataStream); // new StreamReader(dataStream);
        }

        public IBencodeItem Read() {

            while (_binReader.PeekChar() != -1) {

                var c = (char)_binReader.PeekChar();
                bool foundClosing;

                switch (c) {
                    case 'i':
                        // Integer
                        // Read integer until 'e'
                        _binReader.Read();
                        var intChars = "";
                        while (_binReader.PeekChar() != -1) {
                            c = (char)_binReader.Read();
                            if (c == 'e') break;
                            intChars += c;
                        }
                        var intValue = int.Parse(intChars);
                        return new BencodeInteger(intValue);
                    case 'l':
                        // List
                        foundClosing = false;
                        _binReader.Read();
                        var items = new List<IBencodeItem>();
                        while (_binReader.PeekChar() != -1) {
                            if (_binReader.PeekChar() == 'e') {
                                // End of list
                                foundClosing = true;
                                _binReader.Read();
                                break;
                            }
                            items.Add(Read());
                        }
                        if (!foundClosing) {
                            throw new InvalidDataException("Non-closed list");
                        }
                        return new BencodeList(items);
                    case 'd':
                        // Dictionary
                        foundClosing = false;
                        _binReader.Read();
                        var dict = new Dictionary<string, IBencodeItem>();
                        while (_binReader.PeekChar() != -1) {
                            if (_binReader.PeekChar() == 'e') {
                                // End of dictionary
                                foundClosing = true;
                                _binReader.Read();
                                break;
                            }
                            // Read key
                            var dictKey = ReadByteString();
                            // Read value
                            var dictVal = Read();
                            dict.Add(dictKey, dictVal);
                        }
                        if (!foundClosing) {
                            throw new InvalidDataException("Non-closed dictionary");
                        }
                        return new BencodeDictionary(dict);
                    default:
                        // Might be number
                        if (char.IsDigit(c)) {
                            return ReadByteString();
                        }
                        // Not a number, unknown element
                        throw new InvalidDataException($"Unknown element '{c}'");
                }
            }
            return null;
        }

        private BencodeByteString ReadByteString() {

            var numChars = "";
            while (_binReader.PeekChar() != -1) {
                var ch = (char)_binReader.PeekChar();
                if (ch < '0' || ch > '9') break;
                _binReader.Read();
                numChars += ch;
            }

            if (string.IsNullOrEmpty(numChars)) {
                // Couldn't read string length
                throw new InvalidDataException("Invalid byte string length");
            }

            if ((char)_binReader.Read() != ':') {
                throw new InvalidDataException("Byte string length corrupted");
            }
            var byteStringLen = int.Parse(numChars);
            if (byteStringLen < 0) {
                throw new InvalidDataException("Byte string length must be greater or equal than 0");
            }
            var byteStringVal = new byte[byteStringLen];
            for (var i = 0; i < byteStringLen; i++) {
                byteStringVal[i] = _binReader.ReadByte(); // (byte)_streamReader.Read();
            }
            //_streamReader.BaseStream.Read(byteStringVal, 0, byteStringLen);
            return new BencodeByteString(byteStringVal);
        }

    }

}
