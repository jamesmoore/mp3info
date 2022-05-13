using TagLib;

namespace MP3Info.Normalise
{
    public interface INormaliseTrack
    {
        bool CanBeNormalised(Track track);
        void Normalise(File file);
    }
}
