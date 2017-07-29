namespace Z1Torrent.Interfaces {

    public interface IConfig {

        byte[] PeerId { get; }
        short ListenPort { get; }

    }

}
