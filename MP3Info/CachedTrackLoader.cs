using NLog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

namespace MP3Info
{
    public class CachedTrackLoader : ITrackLoader, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ITrackLoader inner;
        private readonly bool whatif;
        private readonly IFileSystem fileSystem;
        private readonly Dictionary<string, Track> cache;

        public string GetPath() => $"data{fileSystem.Path.DirectorySeparatorChar}cache.json";

        public CachedTrackLoader(IFileSystem fileSystem, ITrackLoader inner, bool whatif)
        {
            this.inner = inner;
            this.whatif = whatif;
            this.fileSystem = fileSystem;

            var path = GetPath();
            if (fileSystem.File.Exists(path))
            {
                logger.Debug($"Reading cache at {path}");
                var cacheJson = fileSystem.File.ReadAllText(path);
                var deserialised = JsonSerializer.Deserialize<List<TrackDTO>>(cacheJson);

                var grouped = deserialised.GroupBy(p => p.Filename).Where(p => p.Count() == 1);
                var tracks = grouped.SelectMany(p => p);
                cache = tracks.ToDictionary(p => p.Filename, p => TrackDTOToTrack(p));
            }
            else
            {
                logger.Debug("Initalising new cache");
                cache = new Dictionary<string, Track>();
            }
        }

        public void Dispose()
        {
            if (whatif == false)
            {
                Flush();
            }
        }

        public void Flush()
        {
            var path = GetPath();
            logger.Debug($"Flushing cache to {path}");
            var serialized = JsonSerializer.Serialize(cache.Select(p => TrackToTrackDTO(p.Value)), new JsonSerializerOptions()
            {
                WriteIndented = true,
            });

            string path1 = fileSystem.FileInfo.FromFileName(path).Directory.FullName;
            if (fileSystem.Directory.Exists(path1) == false)
            {
                fileSystem.Directory.CreateDirectory(path1);
            }
            fileSystem.File.WriteAllText(path, serialized);
        }

        public Track GetTrack(string filename)
        {
            var fileInfo = fileSystem.FileInfo.FromFileName(filename);

            if (cache.ContainsKey(filename) && cache[filename].LastUpdated == fileInfo.LastWriteTime)
            {
                return cache[filename];
            }
            else
            {
                var info = inner.GetTrack(filename);
                if (info != null)
                {
                    cache[filename] = info;
                }
                return info;
            }
        }

        private Track TrackDTOToTrack(TrackDTO trackDTO)
        {
            return new Track(fileSystem)
            {
                Album = trackDTO.Album,
                Artist = trackDTO.Artist,
                AlbumArtist = trackDTO.AlbumArtist,
                Comment = trackDTO.Comment,
                Disc = trackDTO.Disc,
                DiscCount = trackDTO.DiscCount,
                Filename = trackDTO.Filename,
                Hash = trackDTO.Hash,
                LastUpdated = trackDTO.LastUpdated,
                Pictures = trackDTO.Pictures,
                TagTypes = trackDTO.TagTypes,
                Title = trackDTO.Title,
                TrackCount = trackDTO.TrackCount,
                TrackNumber = trackDTO.TrackNumber,
                Year = trackDTO.Year,
            };
        }

        private TrackDTO TrackToTrackDTO(Track track)
        {
            return new TrackDTO()
            {
                Album = track.Album,
                Artist = track.Artist,
                AlbumArtist = track.AlbumArtist,
                Comment = track.Comment,
                Disc = track.Disc,
                DiscCount = track.DiscCount,
                Filename = track.Filename,
                Hash = track.Hash,
                LastUpdated = track.LastUpdated,
                Pictures = track.Pictures,
                TagTypes = track.TagTypes,
                Title = track.Title,
                TrackCount = track.TrackCount,
                TrackNumber = track.TrackNumber,
                Year = track.Year,
            };
        }
    }

    internal class TrackDTO
    {
        public string AlbumArtist { get; set; }
        public string Artist { get; set; }
        public uint Year { get; set; }
        public string Album { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
        public uint TrackNumber { get; set; }
        public uint TrackCount { get; set; }
        public string Title { get; set; }
        public int Pictures { get; set; }
        public string Filename { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Comment { get; set; }
        public string Hash { get; set; }

        public TagLib.TagTypes TagTypes { get; set; }
    }
}
