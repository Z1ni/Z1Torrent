using System.Text;
using Xunit;

namespace BencodeLib.Test {

    public class BencodeWriterTest {

        [Fact]
        public void Write_ValidSimpleDictionary() {

            const string expected = "d4:key1i42e10:second_keyi-54123e8:someListl11:test stringi948723eee";

            var item = new BencodeDictionary {
                {"key1", 42},
                {"second_key", -54123},
                {"someList",
                    new BencodeList {
                        new BencodeByteString("test string"),
                        new BencodeInteger(948723)
                    }
                }
            };

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_PositiveInteger() {
            const string expected = "i7462834e";

            var item = new BencodeInteger(7462834);

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_NegativeInteger() {
            const string expected = "i-271934e";

            var item = new BencodeInteger(-271934);

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_MultiLevelDict() {
            const string expected = "d7:level_1d7:level_2d7:level_3d7:level_4d8:treasurei1337eeeeee";

            var item = new BencodeDictionary {
                { "level_1", new BencodeDictionary {
                        { "level_2", new BencodeDictionary {
                                { "level_3", new BencodeDictionary {
                                        { "level_4", new BencodeDictionary {
                                                { "treasure", 1337 }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_SimpleList() {
            const string expected = "li1ei2e5:threei-4ee";

            var item = new BencodeList {
                new BencodeInteger(1),
                new BencodeInteger(2),
                new BencodeByteString("three"),
                new BencodeInteger(-4)
            };

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_EmptyString_StringConstructor() {
            const string expected = "0:";

            var item = new BencodeByteString("");

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_EmptyString_ByteArrayConstructor() {
            const string expected = "0:";

            var item = new BencodeByteString(new byte[0]);

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_EmptyList() {
            const string expected = "le";

            var writer = new BencodeWriter();
            writer.Write(new BencodeList());

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_EmptyDictionary() {
            const string expected = "de";

            var writer = new BencodeWriter();
            writer.Write(new BencodeDictionary());

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }

        [Fact]
        public void Write_DictionaryKeysInOrder() {
            const string expected = "d5:alphai1e4:betai2e5:deltai3e7:epsiloni4ee";

            var item = new BencodeDictionary {
                { "delta", 3 },
                { "alpha", 1 },
                { "epsilon", 4 },
                { "beta", 2 }
            };

            var writer = new BencodeWriter();
            writer.Write(item);

            Assert.Equal(Encoding.UTF8.GetBytes(expected), writer.Bytes);
        }
    }

}
