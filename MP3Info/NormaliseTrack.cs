using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace MP3Info
{
    class NormaliseTrack : ITrackProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly bool whatif;

        public NormaliseTrack(bool whatif)
        {
            this.whatif = whatif;
        }

        public void ProcessTracks(Track track, string root)
        {
            track.Normalise(logger, whatif);
        }
    }
}
