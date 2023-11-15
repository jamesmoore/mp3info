using System;
using System.IO;
using System.IO.Abstractions;
using static TagLib.File;

namespace MP3Info
{
    public class FileSystemTagLibFile(IFileSystem fileSystem, string path) : IFileAbstraction
    {

		public string Name { get; private set; } = path ?? throw new ArgumentNullException(nameof(path));

		public Stream ReadStream => fileSystem.File.Open(Name, FileMode.Open, FileAccess.Read, FileShare.Read);

        public Stream WriteStream => fileSystem.File.Open(Name, FileMode.Open, FileAccess.ReadWrite);

		public void CloseStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Close();
        }
    }
}
