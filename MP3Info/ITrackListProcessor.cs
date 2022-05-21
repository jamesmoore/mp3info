using System.Collections.Generic;

namespace MP3Info
{
    public interface ITrackListProcessor
    {
        void ProcessTracks(IEnumerable<Track> tracks, string root);
    }
}
