using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire.Interfaces {

    public interface IPeerConnection {

        Task ConnectAsync();
        void Disconnect();

        Task SendMessageAsync(IMessage msg);
        Task<T> ReceiveMessageAsync<T>() where T : IMessage;
        Task<IMessage> ReceiveMessageAsync();


    }

}
