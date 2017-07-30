using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Z1Torrent.PeerWire.Messages;

namespace Z1Torrent.Test {

    // Test message creation and parsing
    public class MessageTest {

        [Fact]
        public void ChokeMessage_ParseValid() {
            var payload = new byte[0];
            var msg = new ChokeMessage();
            msg.Unpack(payload);
        }

        [Fact]
        public void ChokeMessage_CreateValid() {
            var msg = new ChokeMessage();
            Assert.Equal(new byte[0], msg.Pack());
        }

        [Fact]
        public void UnchokeMessage_ParseValid() {
            var payload = new byte[0];
            var msg = new UnchokeMessage();
            msg.Unpack(payload);
        }

        [Fact]
        public void UnchokeMessage_CreateValid() {
            var msg = new UnchokeMessage();
            Assert.Equal(new byte[0], msg.Pack());
        }

        [Fact]
        public void InterestedMessage_ParseValid() {
            var payload = new byte[0];
            var msg = new InterestedMessage();
            msg.Unpack(payload);
        }

        [Fact]
        public void InterestedMessage_CreateValid() {
            var msg = new InterestedMessage();
            Assert.Equal(new byte[0], msg.Pack());
        }

        [Fact]
        public void NotInterestedMessage_ParseValid() {
            var payload = new byte[0];
            var msg = new NotInterestedMessage();
            msg.Unpack(payload);
        }

        [Fact]
        public void NotInterestedMessage_CreateValid() {
            var msg = new NotInterestedMessage();
            Assert.Equal(new byte[0], msg.Pack());
        }

        [Fact]
        public void HaveMessage_ParseValid() {
            var payload = new byte[] {
                0x49, 0x96, 0x02, 0xD2
            };
            var msg = new HaveMessage();
            msg.Unpack(payload);
            Assert.Equal(1234567890, msg.PieceIndex);
        }

        [Fact]
        public void HaveMessage_CreateValid() {
            var msg = new HaveMessage(1234567890);;
            Assert.Equal(new byte[] { 0x49, 0x96, 0x02, 0xD2 }, msg.Pack());
        }

        [Fact]
        public void RequestMessage_ParseValid() {
            // Index: 54321
            // Begin: 42
            // Length: 16 KiB (16384 bytes)
            var payload = new byte[] {
                0x00, 0x00, 0xD4, 0x31, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x40, 0x00
            };
            var msg = new RequestMessage();
            msg.Unpack(payload);
            Assert.Equal(54321, msg.Index);
            Assert.Equal(42, msg.Begin);
            Assert.Equal(16384, msg.Length);
        }

        [Fact]
        public void RequestMessage_CreateValid() {
            var msg = new RequestMessage(54321, 42, 16384);
            Assert.Equal(new byte[] {
                0x00, 0x00, 0xD4, 0x31, 0x00, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x40, 0x00
            }, msg.Pack());
        }

        [Fact]
        public void PieceMessage_ParseValid() {
            // Index: 54321
            // Begin: 42
            // Block: 4 bytes of data (0xAABBCCDD)
            var payload = new byte[] {
                0x00, 0x00, 0xD4, 0x31, 0x00, 0x00, 0x00, 0x2A, 0xAA, 0xBB, 0xCC, 0xDD
            };
            var msg = new PieceMessage();
            msg.Unpack(payload);
            Assert.Equal(54321, msg.Index);
            Assert.Equal(42, msg.Begin);
            Assert.Equal(4, msg.Block.Length);
            Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, msg.Block);
        }

        [Fact]
        public void PieceMessage_CreateValid() {
            var msg = new PieceMessage(54321, 42, new byte[] { 0xAA, 0xBB, 0xCC, 0xDD });
            Assert.Equal(new byte[] {
                0x00, 0x00, 0xD4, 0x31, 0x00, 0x00, 0x00, 0x2A, 0xAA, 0xBB, 0xCC, 0xDD
            }, msg.Pack());
        }

    }

}
