namespace MP3Info.Normalise
{
    class NormaliseTrack(bool whatif) : ITrackProcessor
    {
		public void ProcessTrack(Track track, string root)
        {
            track.Normalise(whatif);
        }
    }
}
