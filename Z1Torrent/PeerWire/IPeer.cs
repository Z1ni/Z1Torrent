﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Z1Torrent.PeerWire {

    public interface IPeer : IDisposable {

        byte[] PeerId { get; }
        IPAddress Address { get; }
        short Port { get; }
        bool AmChoking { get; }
        bool AmInterested { get; }
        bool PeerChoking { get; }
        bool PeerInterested { get; }

        Task StartMessageLoopAsync();
        void StopMessageLoop();

    }

}
