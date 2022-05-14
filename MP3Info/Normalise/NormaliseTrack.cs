namespace MP3Info.Normalise
{
    class NormaliseTrack : ITrackProcessor
    {
        private readonly bool whatif;

        public NormaliseTrack(bool whatif)
        {
            this.whatif = whatif;
        }

        public void ProcessTrack(Track track, string root)
        {
            track.Normalise(whatif);
        }
    }
}
