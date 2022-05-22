using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MP3Info
{
    class CSVTrackLister : ITrackListProcessor
    {
        private readonly string outfile;

        public CSVTrackLister(string outfile)
        {
            this.outfile = outfile;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            using var writer = new StreamWriter(outfile);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(tracks);
        }
    }
}
