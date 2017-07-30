using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BencodeLib;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent.PeerWire.ExtendedMessages {

    public class ExtendedHandshakeMessage : IExtendedMessage {

        public int Id => 0;

        public int? LocalListenPort { get; private set; }
        public string ClientNameVersion { get; private set; }
        public IPAddress YourIp { get; private set; }
        public IPAddress MyIpv6 { get; private set; }
        public IPAddress MyIpv4 { get; private set; }
        public int? ReqQ { get; private set; }   // TODO: Rename

        public ExtendedHandshakeMessage() { }

        public ExtendedHandshakeMessage(int? listenPort = null, string clientNameVer = null, IPAddress yourIp = null,
            IPAddress myIpv6 = null, IPAddress myIpv4 = null, int? reqq = null) {
            LocalListenPort = listenPort;
            ClientNameVersion = clientNameVer;
            YourIp = yourIp;
            MyIpv6 = myIpv6;
            MyIpv4 = myIpv4;
            ReqQ = reqq;
        }

        public byte[] Pack() {

            var bencodeWriter = new BencodeWriter();
            var root = new BencodeDictionary {
                // TODO: Support messages
                {"m", new BencodeDictionary()}
            };

            // Add other handshake information if available
            if (LocalListenPort != null) {
                root.Add("p", (long)LocalListenPort);
            }
            if (ClientNameVersion != null) {
                root.Add("v", ClientNameVersion);
            }
            if (YourIp != null) {
                root.Add("yourip", YourIp.GetAddressBytes());
            }
            if (MyIpv6 != null) {
                root.Add("ipv6", MyIpv6.GetAddressBytes());
            }
            if (MyIpv4 != null) {
                root.Add("ipv4", MyIpv4.GetAddressBytes());
            }
            if (ReqQ != null) {
                root.Add("reqq", (long)ReqQ);
            }

            bencodeWriter.Write(root);
            return bencodeWriter.Bytes;
        }

        public void Unpack(byte[] data) {
            // Extended handshake message payload is a bencoded dict
            var bencodeReader = new BencodeReader(data);
            var root = bencodeReader.Read();
            var dict = root as BencodeDictionary;
            if (dict == null) {
                throw new InvalidMessageException("Extended message handshake was invalid");
            }
            var supportedMessages = dict.Get<BencodeDictionary>("m");

            var localPort = dict.Get<BencodeInteger>("p");
            if (localPort != null) {
                LocalListenPort = localPort;
            }

            var versionStr = dict.Get<BencodeByteString>("v");
            if (versionStr != null) {
                ClientNameVersion = versionStr;
            }

            var yourIpBytes = dict.Get<BencodeByteString>("yourip");
            if (yourIpBytes != null) {
                // Create IPAddress instance for given bytes
                YourIp = new IPAddress(yourIpBytes);
            }

            var myIpv6 = dict.Get<BencodeByteString>("ipv6");
            if (myIpv6 != null) {
                MyIpv6 = new IPAddress(myIpv6);
            }

            var myIpv4 = dict.Get<BencodeByteString>("ipv4");
            if (myIpv4 != null) {
                MyIpv4 = new IPAddress(myIpv4);
            }

            var reqq = dict.Get<BencodeInteger>("reqq");
            if (reqq != null) {
                ReqQ = reqq;
            }
        }
    }

}
