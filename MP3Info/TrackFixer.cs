using System.Collections.Generic;

namespace MP3Info
{
    class TrackFixer : ITrackListProcessor
    {
        private readonly TrackRenamer trackRenamer;
        private readonly ArtExporter artExporter;
        private readonly HashBuilder hashBuilder;
        private readonly NormaliseTrack normaliseTrack;
        private readonly bool whatif;

        public TrackFixer(bool whatif, bool force)
        {
            this.trackRenamer = new TrackRenamer(whatif);
            this.artExporter = new ArtExporter(whatif);
            this.hashBuilder = new HashBuilder(whatif, force);
            this.normaliseTrack = new NormaliseTrack(whatif);
            this.whatif = whatif;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            foreach (var track in tracks)
            {
                hashBuilder.ProcessTracks(track, root);
                normaliseTrack.ProcessTracks(track, root);
                trackRenamer.ProcessTracks(track, root);
                artExporter.ProcessTracks(track, root);
            }

            new EmptyDirectoryRemover(whatif).processDirectory(root);
        }
    }
}
