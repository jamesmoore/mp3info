using MP3Info.ArtExport;
using MP3Info.Hash;
using MP3Info.Normalise;
using MP3Info.Rename;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace MP3Info
{
    class TrackFixer(IFileSystem fileSystem, bool whatif, bool force) : ITrackListProcessor
    {
        private readonly TrackRenamer trackRenamer = new TrackRenamer(fileSystem, whatif);
        private readonly ArtExporter artExporter = new ArtExporter(fileSystem, whatif);
        private readonly TrackHashWriter hashBuilder = new TrackHashWriter(whatif, force);
        private readonly NormaliseTrack normaliseTrack = new NormaliseTrack(whatif);

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
