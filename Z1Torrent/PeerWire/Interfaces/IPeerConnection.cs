using System;
using System.Threading.Tasks;
using Z1Torrent.PeerWire.ExtendedMessages;
using Z1Torrent.PeerWire.Messages;

namespace Z1Torrent.PeerWire.Interfaces {

    public interface IPeerConnection : IDisposable {

        HandshakeMessage PeerHandshake { get; }
        ExtendedHandshakeMessage ExtendedPeerHandshake { get; }
        DateTime LastMessageSentTime { get; }
        DateTime LastMessageReceivedTime { get; }

        Task ConnectAsync();
        void Disconnect();

        Task SendMessageAsync(IMessage msg);
        Task<T> ReceiveMessageAsync<T>() where T : IMessage;
        Task<IMessage> ReceiveMessageAsync();

        event EventHandler<EventArgs> OnConnectionInitialized;
    }

}
