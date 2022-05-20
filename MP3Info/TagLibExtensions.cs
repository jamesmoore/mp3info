using System.Collections.Generic;
using System.Linq;
using TagLib.Id3v2;

namespace MP3Info
{
    public static class TagLibExtensions
    {
        public static Tag GetId3v2Tag(this TagLib.File file)
        {
            return (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
        }

        public static IEnumerable<UserTextInformationFrame> GetUserTextInformationFrames(this Tag tag)
        {
            return tag.GetFrames().OfType<UserTextInformationFrame>();
        }
    }
}
