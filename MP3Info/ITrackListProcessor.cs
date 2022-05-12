using System.Collections.Generic;

namespace MP3Info
{
    interface ITrackListProcessor
    {
        void ProcessTracks(IEnumerable<Track> tracks, string root);
    }
}
