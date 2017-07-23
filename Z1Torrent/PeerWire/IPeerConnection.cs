using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire {

    public interface IPeerConnection {

        Task ConnectAsync();
        Task DisconnectAsync();

        Task SendMessageAsync(IMessage msg);
        Task<T> ReceiveMessageAsync<T>() where T : IMessage;
        
    }

}
