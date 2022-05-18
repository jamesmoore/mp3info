using MP3Info.Hash;
using NLog;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Linq;

namespace MP3Info
{
    class Program
    {
        static int Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var processor = new Processor(fileSystem);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var pathArgument = new Argument<string>("path", "Path to mp3 directory");

            var validateCommand = new Command("validate", "Validate hash in mp3 comments.");
            validateCommand.AddArgument(pathArgument);
            validateCommand.AddOption(new Option<bool>(new string[] { "--verbose", "-v" }, "Show all validate results (not just failures)."));
            validateCommand.Handler = CommandHandler.Create((string path, bool v) => processor.ProcessList(path, new TrackHashValidator(v)));

            var listDupesCommand = new Command("listdupes", "List duplicate mp3 files.");
            listDupesCommand.AddArgument(pathArgument);
            listDupesCommand.Handler = CommandHandler.Create((string path) => processor.ProcessList(path, new ListDupes()));

            var trackFixCommand = new Command("fix", "Fix names, export art, add hash.");
            trackFixCommand.AddArgument(pathArgument);
            trackFixCommand.AddOption(new Option<bool>(new string[] { "--whatif", "-w" }, "List what would have their art exported without actually exporting."));
            trackFixCommand.AddOption(new Option<bool>(new string[] { "--force", "-f" }, "Force overwriting invalid hashes."));
            trackFixCommand.Handler = CommandHandler.Create((string path, bool w, bool f) => processor.ProcessList(path, new TrackFixer(fileSystem, w, f), w));

            var listCommand = new Command("list", "List mp3 metadata to csv.");
            listCommand.AddArgument(pathArgument);
            listCommand.AddOption(new Option<string>(new string[] { "--outfile", "-o" }, () => "mp3info.csv", "Output file name."));
            listCommand.Handler = CommandHandler.Create((string path, string outfile) => processor.ProcessList(path, new CSVTrackLister(outfile)));

            var parent = new RootCommand()
            {
               validateCommand,
               listDupesCommand,
               trackFixCommand,
               listCommand,
            };

            parent.InvokeAsync(args).Wait();
            return 0;
        }
    }

    class Processor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IFileSystem fileSystem;

        public Processor(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public int ProcessList(string path, ITrackListProcessor processor, bool whatif = false)
        {
            TagLib.Id3v2.Tag.DefaultVersion = 4;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            if (fileSystem.Directory.Exists(path) == false)
            {
                if (path.EndsWith("\""))
                {
                    var pathMinusQuote = path.Substring(0, path.Length - 1);
                    if (fileSystem.Directory.Exists(pathMinusQuote))
                    {
                        path = pathMinusQuote;
                    }
                    else
                    {
                        logger.Error($"{path} does not exist");
                        return 1;
                    }
                }
                else
                {
                    logger.Error($"{path} does not exist");
                    return 1;
                }
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