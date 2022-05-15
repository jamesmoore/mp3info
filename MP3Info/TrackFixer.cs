using MP3Info.Hash;
using MP3Info.Normalise;
using MP3Info.Rename;
using System.Collections.Generic;

namespace MP3Info
{
    class TrackFixer : ITrackListProcessor
    {
        private readonly TrackRenamer trackRenamer;
        private readonly ArtExporter artExporter;
        private readonly TrackHashWriter hashBuilder;
        private readonly NormaliseTrack normaliseTrack;
        private readonly bool whatif;

        public TrackFixer(bool whatif, bool force)
        {
            this.trackRenamer = new TrackRenamer(whatif);
            this.artExporter = new ArtExporter(whatif);
            this.hashBuilder = new TrackHashWriter(whatif, force);
            this.normaliseTrack = new NormaliseTrack(whatif);
            this.whatif = whatif;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            foreach (var track in tracks)
            {
                hashBuilder.ProcessTrack(track, root);
                normaliseTrack.ProcessTrack(track, root);
                trackRenamer.ProcessTrack(track, root);
                artExporter.ProcessTrack(track, root);
            }

            new EmptyDirectoryRemover(whatif).processDirectory(root);
        }
    }
}
