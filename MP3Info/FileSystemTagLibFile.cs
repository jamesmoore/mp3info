using System;
using System.IO;
using System.IO.Abstractions;
using static TagLib.File;

namespace MP3Info
{
    public class FileSystemTagLibFile : IFileAbstraction
    {
        private readonly IFileSystem fileSystem;

        public string Name { get; private set; }

        public Stream ReadStream => fileSystem.File.Open(Name, FileMode.Open, FileAccess.Read, FileShare.Read);

        public Stream WriteStream => fileSystem.File.Open(Name, FileMode.Open, FileAccess.ReadWrite);

        public FileSystemTagLibFile(IFileSystem fileSystem, string path)
        {
            this.fileSystem = fileSystem;
            Name = path ?? throw new ArgumentNullException(nameof(path));
        }

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
