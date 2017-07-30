namespace Z1Torrent.Interfaces {

    public interface IConfig {

        string ClientNameVersion { get; }
        byte[] PeerId { get; }
        short ListenPort { get; }

    }

}
