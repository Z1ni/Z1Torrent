using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BencodeLib {

    public class BencodeWriter {

        private readonly List<byte> _bytes;

        public byte[] Bytes => _bytes.ToArray();

        public BencodeWriter() {
            _bytes = new List<byte>();
        }

        public void Write(IBencodeItem item) {

            switch (item) {

                case BencodeInteger bInt:
                    _bytes.AddRange(Encoding.UTF8.GetBytes("i"));
                    _bytes.AddRange(Encoding.UTF8.GetBytes(bInt.ToString()));
                    _bytes.AddRange(Encoding.UTF8.GetBytes("e"));
                    break;

                case BencodeByteString bStr:
                    byte[] bStrBytes = bStr;
                    _bytes.AddRange(Encoding.UTF8.GetBytes(bStrBytes.Length + ":"));
                    _bytes.AddRange(bStrBytes);
                    break;

                case BencodeList bList:
                    _bytes.AddRange(Encoding.UTF8.GetBytes("l"));
                    foreach (var listItem in bList) {
                        Write(listItem);
                    }
                    _bytes.AddRange(Encoding.UTF8.GetBytes("e"));
                    break;

                case BencodeDictionary bDict:
                    _bytes.AddRange(Encoding.UTF8.GetBytes("d"));
                    // Bencoded dictionaries must appear in sorted order by key
                    var orderedDict = bDict.OrderBy(i => i.Key);
                    foreach (var dictItem in orderedDict) {
                        // Write key
                        Write(new BencodeByteString(dictItem.Key));
                        // Write value
                        Write(dictItem.Value);
                    }
                    _bytes.AddRange(Encoding.UTF8.GetBytes("e"));
                    break;

                default:
                    throw new InvalidDataException($"Unknown Bencode item type '{item.GetType().GetTypeInfo().Name}'");
            }
        }

    }

}
