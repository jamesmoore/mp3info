using System.Collections.Generic;
using System.Linq;
using TagLib.Id3v2;

namespace MP3Info
{
    public static class TagLibExtensions
    {
        private const string HashKey = "hash";

        public static Tag GetId3v2Tag(this TagLib.File file)
        {
            return (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
        }

        public static IEnumerable<UserTextInformationFrame> GetUserTextInformationFrames(this Tag tag)
        {
            return tag.GetFrames().OfType<UserTextInformationFrame>();
        }

        public static string GetHash(this TagLib.File file)
        {
            if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
            {
                var custom = file.GetId3v2Tag();
                var hashTextFields = custom.GetUserTextInformationFrames().Where(p => p.Description == HashKey).ToList();
                return hashTextFields.FirstOrDefault()?.Text.FirstOrDefault();
            }
            else if (file.TagTypes.HasFlag(TagLib.TagTypes.Xiph))
            {
                var flagtag = file.GetTag(TagLib.TagTypes.Xiph) as TagLib.Ogg.XiphComment;
                var hashFields = flagtag.GetField(HashKey);
                return hashFields.FirstOrDefault(p => string.IsNullOrWhiteSpace(p) == false);
            }
            else
            {
                return null;
            }
        }

        public static void WriteHash(this TagLib.File tagFile, string hash)
        {
            if (tagFile.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
            {
                var custom = tagFile.GetId3v2Tag();

                var hashTextFields = custom.GetUserTextInformationFrames().Where(p => p.Description == HashKey).ToList();
                foreach (var frame in hashTextFields)
                {
                    custom.RemoveFrame(frame);
                }

                var newHash = new UserTextInformationFrame(HashKey)
                {
                    Text = new string[] { hash }
                };
                custom.AddFrame(newHash);
            }
            else if (tagFile.TagTypes.HasFlag(TagLib.TagTypes.Xiph))
            {
                var flagtag = tagFile.GetTag(TagLib.TagTypes.Xiph) as TagLib.Ogg.XiphComment;
                flagtag.SetField(HashKey, new string[] { hash });
                if (flagtag.Comment != null && (flagtag.Comment == hash || flagtag.Comment.Replace("-", "/") == hash))
                {
                    flagtag.Comment = null;
                }
            }
        }
    }
}
