using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using NLog;
using Z1Torrent.Factories;
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
        public static IConfig Config { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Log.Info("Starting up");

            SetupDependencies();
            TorrentClient = Container.Resolve<ITorrentClient>();
            Config = Container.Resolve<IConfig>();

            Log.Info("Application setup done");
        }

        private static void SetupDependencies() {
            Log.Debug("Setting up dependencies");
            var builder = new ContainerBuilder();
            // TODO: Check if other classes can be singletons
            builder.RegisterType<Config>().As<IConfig>().SingleInstance();
            builder.RegisterType<MetafileFactory>().As<IMetafileFactory>();
            builder.RegisterType<HttpTrackerFactory>().As<IHttpTrackerFactory>();
            builder.RegisterType<TcpClientAdapter>().As<ITcpClient>();
            builder.RegisterType<PeerConnectionFactory>().As<IPeerConnectionFactory>();
            builder.RegisterType<PeerFactory>().As<IPeerFactory>();
            builder.RegisterType<TorrentClient>().As<ITorrentClient>();
            Container = builder.Build();
        }

        private void App_OnExit(object sender, ExitEventArgs e) {
            TorrentClient.Dispose();
        }
    }

}
