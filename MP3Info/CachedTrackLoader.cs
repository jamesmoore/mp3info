using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MP3Info
{
    class CachedTrackLoader : ITrackLoader, IDisposable
    {
        private const string Path = "cache.json";
        private readonly ITrackLoader inner;
        private readonly bool whatif;
        private readonly Dictionary<string, Track> cache;
        public CachedTrackLoader(ITrackLoader inner, bool whatif)
        {
            this.inner = inner;
            this.whatif = whatif;
            if (File.Exists(Path))
            {
                var cacheJson = File.ReadAllText(Path);
                var deserialised = JsonSerializer.Deserialize<List<Track>>(cacheJson);

                var grouped = deserialised.GroupBy(p => p.Filename).Where(p => p.Count() == 1);
                var tracks = grouped.SelectMany(p => p);
                cache = tracks.ToDictionary(p => p.Filename, p => p);
            }
            else
            {
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
            var serialized = JsonSerializer.Serialize(cache.Select(p => p.Value), new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
            File.WriteAllText(Path, serialized);
        }

        public Track GetTrack(string filename)
        {
            var fileInfo = new FileInfo(filename);

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
    }
}
