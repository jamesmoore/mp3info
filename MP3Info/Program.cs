using MP3Info.Hash;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace MP3Info
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            var fileSystem = new FileSystem();
            var processor = new DirectoryProcessor(fileSystem);
            return await new Program().Main(args, processor, fileSystem);
        }

        public async Task<int> Main(string[] args, IDirectoryProcessor processor, IFileSystem fileSystem)
        {
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

            var result = await parent.InvokeAsync(args);
            return result;
        }
    }
}