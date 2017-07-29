using Z1Torrent.Interfaces;

namespace Z1Torrent.Factories {

    public class MetafileFactory : IMetafileFactory {

        private readonly IHttpTrackerFactory _httpTrackerFactory;

        public MetafileFactory(IHttpTrackerFactory httpTrackerFactory) {
            _httpTrackerFactory = httpTrackerFactory;
        }

        public IMetafile CreateMetafileFromFile(string path) {
            return new Metafile(_httpTrackerFactory, path);
        }

    }

}
