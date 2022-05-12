using System.IO;

namespace MP3Info
{
    public class FileBytesAbstraction : TagLib.File.IFileAbstraction
    {
        public FileBytesAbstraction(string name, MemoryStream memoryStream)
        {
            Name = name;

            ReadStream = memoryStream;
            WriteStream = memoryStream;
        }

        public void CloseStream(Stream stream)
        {
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

    }
}
