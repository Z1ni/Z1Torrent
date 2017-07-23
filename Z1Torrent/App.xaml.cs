using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using NLog;
using Z1Torrent.Interfaces;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Dependency injection
        private static IContainer Container { get; set; }
        public static ITorrentClient TorrentClient { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Log.Info("Starting up");

            SetupDependencies();
            TorrentClient = Container.Resolve<ITorrentClient>();

            Log.Info("Application setup done");
        }

        private static void SetupDependencies() {
            Log.Debug("Setting up dependencies");
            var builder = new ContainerBuilder();
            //builder.RegisterType<Metafile>().As<IMetafile>();
            builder.RegisterType<Peer>().As<IPeer>();
            builder.RegisterType<PeerConnection>().As<IPeerConnection>();
            builder.RegisterType<TorrentClient>().As<ITorrentClient>();
            Container = builder.Build();
        }

        private void App_OnExit(object sender, ExitEventArgs e) {
            TorrentClient.Dispose();
        }
    }

}
