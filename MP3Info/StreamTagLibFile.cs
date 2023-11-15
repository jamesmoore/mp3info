﻿using System.IO;

namespace MP3Info
{
    public class StreamTagLibFile(string name, Stream stream) : TagLib.File.IFileAbstraction
    {
		public void CloseStream(Stream stream)
        {
        }

		public string Name { get; private set; } = name;

		public Stream ReadStream { get; private set; } = stream;

		public Stream WriteStream { get; private set; } = stream;

	}
}
