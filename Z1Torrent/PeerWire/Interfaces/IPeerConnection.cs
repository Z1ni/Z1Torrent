﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z1Torrent.PeerWire.ExtendedMessages;

namespace Z1Torrent.PeerWire.Interfaces {

    public interface IPeerConnection {

        HandshakeMessage PeerHandshake { get; }
        ExtendedHandshakeMessage ExtendedPeerHandshake { get; }
        DateTime LastMessageSentTime { get; }
        DateTime LastMessageReceivedTime { get; }

        Task ConnectAsync();
        void Disconnect();

        Task SendMessageAsync(IMessage msg);
        Task<T> ReceiveMessageAsync<T>() where T : IMessage;
        Task<IMessage> ReceiveMessageAsync();


    }

}
