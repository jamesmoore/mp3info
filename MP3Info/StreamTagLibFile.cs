using System.IO;

namespace MP3Info
{
    public class StreamTagLibFile : TagLib.File.IFileAbstraction
    {
        public StreamTagLibFile(string name, Stream stream)
        {
            Name = name;

            ReadStream = stream;
            WriteStream = stream;
        }

        public void CloseStream(Stream stream)
        {
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

    }
}
