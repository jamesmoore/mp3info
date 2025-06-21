using MP3Info.Hash;
using System;
using System.CommandLine;
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

            var pathArgument = new Argument<string>("path")
            {
                Description = "Path to mp3 directory",
            };

            var validateCommand = new Command("validate", "Validate hash in mp3 comments.");
            validateCommand.Arguments.Add(pathArgument);
            var verboseOption = new Option<bool>("--verbose", "-v")
            {
                Description = "Show all validate results (not just failures)."
            };
            validateCommand.Options.Add(verboseOption);
            validateCommand.SetAction((p) => processor.ProcessList(p.GetValue(pathArgument), new TrackHashValidator(p.GetValue(verboseOption))));

            var listDupesCommand = new Command("listdupes", "List duplicate mp3 files.");
            listDupesCommand.Arguments.Add(pathArgument);
            listDupesCommand.SetAction((p) => processor.ProcessList(p.GetValue(pathArgument), new ListDupes()));

            var trackFixCommand = new Command("fix", "Fix names, export art, add hash.");
            trackFixCommand.Arguments.Add(pathArgument);
            var whatifOption = new Option<bool>("--whatif", "-w")
            {
                Description = "List what would have their art exported without actually exporting.",
            };
            trackFixCommand.Options.Add(whatifOption);
            var forceOption = new Option<bool>("--force", "-f")
            {
                Description = "Force overwriting invalid hashes.",
            };
            trackFixCommand.Options.Add(forceOption);
            trackFixCommand.SetAction((p) => processor.ProcessList(p.GetValue(pathArgument), new TrackFixer(fileSystem, p.GetValue(whatifOption), p.GetValue(forceOption)), p.GetValue(whatifOption)));

            var listCommand = new Command("list", "List mp3 metadata to csv.");
            listCommand.Arguments.Add(pathArgument);
            var outfileOption = new Option<string>("--outfile", "-o")
            {
                Description = "Output file name.",
                DefaultValueFactory = _ => "mp3info.csv",
            };
            listCommand.Options.Add(outfileOption);
            listCommand.SetAction((p) => processor.ProcessList(p.GetValue(pathArgument), new CSVTrackLister(p.GetValue(outfileOption))));

            var parent = new RootCommand()
            {
               validateCommand,
               listDupesCommand,
               trackFixCommand,
               listCommand,
            };

            var result = parent.Parse(args);

            return await result.InvokeAsync();
        }
    }
}