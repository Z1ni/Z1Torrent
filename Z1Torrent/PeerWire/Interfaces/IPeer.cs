using System;
using System.Net;

namespace Z1Torrent.PeerWire.Interfaces {

    public interface IPeer : IDisposable {

        byte[] PeerId { get; }
        string ClientName { get; }
        IPAddress Address { get; }
        short Port { get; }
        bool AmChoking { get; }
        bool AmInterested { get; }
        bool PeerChoking { get; }
        bool PeerInterested { get; }

        void StartMessageLoop();
        void StopMessageLoop();

        event EventHandler<EventArgs> OnConnectionInitialized;
        event EventHandler<EventArgs> OnDisconnected;
    }

}
