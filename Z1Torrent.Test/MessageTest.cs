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

    }

}
