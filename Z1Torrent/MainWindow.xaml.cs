using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLog;
using Z1Torrent.Factories;
using Z1Torrent.PeerWire;
using Z1Torrent.PeerWire.Interfaces;

namespace Z1Torrent {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

	    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public MainWindow() {
			InitializeComponent();
            Log.Debug("MainWindow init");

            var metafile = App.TorrentClient.ManageFromFile(@"../../../Z1Torrent.Test/TestData/debian-9.0.0-amd64-netinst.iso.torrent");
            var testPeer = new Peer(new PeerConnectionFactory(App.Config, new TcpClientAdapter()), metafile, IPAddress.Loopback, 8999);
            metafile.AddPeers(new List<IPeer> {
                testPeer
            });

            testPeer.StartMessageLoop();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show(this, $"My peer ID is: {Encoding.UTF8.GetString(App.Config.PeerId)}");
        }
    }
}
