namespace MP3Info
{
    public interface IDirectoryProcessor
    {
        int ProcessList(ITrackListProcessor processor);
    }
}