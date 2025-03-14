﻿using NLog;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace MP3Info
{
    public class DirectoryProcessor(IFileSystem fileSystem) : IDirectoryProcessor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

		public int ProcessList(string path, ITrackListProcessor processor, bool whatif = false)
        {
            TagLib.Id3v2.Tag.DefaultVersion = 4;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            while (path.EndsWith(fileSystem.Path.DirectorySeparatorChar))
            {
                path = path[0..^1];
            }

            if (fileSystem.Directory.Exists(path) == false)
            {
                logger.Error($"{path} does not exist");
                return 1;
            }

            var filetypes = new List<string>()
            {
                "*.mp3",
                "*.flac",
            };

            var files = filetypes.Select(p => fileSystem.Directory.GetFiles(path, p, System.IO.SearchOption.AllDirectories)).SelectMany(p => p).OrderBy(p => p).ToList();

            using (var loader = new CachedTrackLoader(fileSystem, new TrackLoader(fileSystem), whatif))
            {
                var tracks = files.Select(p => loader.GetTrack(p)).Where(p => p != null).OrderBy(p => p.AlbumArtist).ThenBy(p => p.Year).ThenBy(p => p.Album).ThenBy(p => p.Year).ThenBy(p => p.Disc).ThenBy(p => p.TrackNumber).ToList();
                loader.Flush();
                processor.ProcessTracks(tracks, path);
            }

            return 1;
        }
    }
}