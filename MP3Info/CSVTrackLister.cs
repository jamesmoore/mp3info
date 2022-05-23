using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MP3Info
{
    class CSVTrackLister : ITrackListProcessor
    {
        private readonly AppContext appContext;

        public CSVTrackLister(AppContext appContext)
        {
            this.appContext = appContext;
        }

        public void ProcessTracks(IEnumerable<Track> tracks, string root)
        {
            using var writer = new StreamWriter(appContext.OutputFile);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(tracks);
        }
    }
}
