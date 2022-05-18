using MP3Info.ArtExport;
using MP3Info.Hash;
using MP3Info.Normalise;
using MP3Info.Rename;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace MP3Info
{
    class TrackFixer : ITrackListProcessor
    {
        private readonly TrackRenamer trackRenamer;
        private readonly ArtExporter artExporter;
        private readonly TrackHashWriter hashBuilder;
        private readonly NormaliseTrack normaliseTrack;
        private readonly bool whatif;
        private readonly IFileSystem fileSystem;

        public TrackFixer(IFileSystem fileSystem, bool whatif, bool force)
        {
            this.trackRenamer = new TrackRenamer(fileSystem, whatif);
            this.artExporter = new ArtExporter(fileSystem, whatif);
            this.hashBuilder = new TrackHashWriter(whatif, force);
            this.normaliseTrack = new NormaliseTrack(whatif);
            this.whatif = whatif;
            this.fileSystem = fileSystem;
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

            new EmptyDirectoryRemover(fileSystem, whatif).processDirectory(root);
        }
    }
}
