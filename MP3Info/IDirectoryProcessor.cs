namespace MP3Info
{
    public interface IDirectoryProcessor
    {
        int ProcessList(string path, ITrackListProcessor processor, bool whatif = false);
    }
}