namespace Z1Torrent {

    public class File {

        public string Name { get; internal set; }
        public long Size { get; internal set; }
        public string Path { get; internal set; }

        public File(string name, long size) {
            Name = name;
            Size = size;
        }

        public File(string name, long size, string path) {
            Name = name;
            Size = size;
            Path = path;
        }

    }

}
