using Microsoft.Extensions.DependencyInjection;
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
        private readonly IDirectoryProcessor processor;
        private readonly IFileSystem fileSystem;
        private readonly IServiceProvider serviceProvider;
        private readonly AppContext appContext;

        static async Task<int> Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<Program>()
                .AddSingleton<IDirectoryProcessor, DirectoryProcessor>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<AppContext>()
                .AddSingleton<TrackHashValidator>()
                .AddSingleton<ListDupes>()
                .AddSingleton<CSVTrackLister>()
                .AddSingleton<TrackFixer>()
                .BuildServiceProvider();

            var program = serviceProvider.GetService<Program>();

            return await program.Start(args);
        }

        public Program(
            IDirectoryProcessor processor,
            IFileSystem fileSystem,
            IServiceProvider serviceProvider,
            AppContext appContext)
        {
            this.processor = processor;
            this.fileSystem = fileSystem;
            this.serviceProvider = serviceProvider;
            this.appContext = appContext;
        }

        public async Task<int> Start(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var pathArgument = new Argument<string>("path", "Path to mp3 directory");

            var validateCommand = new Command("validate", "Validate hash in mp3 comments.");
            validateCommand.AddArgument(pathArgument);
            validateCommand.AddOption(new Option<bool>(new string[] { "--verbose", "-v" }, "Show all validate results (not just failures)."));
            validateCommand.Handler = CommandHandler.Create((string path, bool v) =>
            {
                appContext.Verbose = v;
                appContext.Path = path;
                return processor.ProcessList(serviceProvider.GetService<TrackHashValidator>());
            });

            var listDupesCommand = new Command("listdupes", "List duplicate mp3 files.");
            listDupesCommand.AddArgument(pathArgument);
            listDupesCommand.Handler = CommandHandler.Create((string path) =>
            {
                appContext.Path = path;
                return processor.ProcessList(serviceProvider.GetService<ListDupes>());
            });

            var trackFixCommand = new Command("fix", "Fix names, export art, add hash.");
            trackFixCommand.AddArgument(pathArgument);
            trackFixCommand.AddOption(new Option<bool>(new string[] { "--whatif", "-w" }, "List what would have their art exported without actually exporting."));
            trackFixCommand.AddOption(new Option<bool>(new string[] { "--force", "-f" }, "Force overwriting invalid hashes."));
            trackFixCommand.Handler = CommandHandler.Create((string path, bool w, bool f) =>
            {
                appContext.WhatIf = w;
                appContext.Force = f;
                appContext.Path = path;
                return processor.ProcessList(serviceProvider.GetService<TrackFixer>());
            });

            var listCommand = new Command("list", "List mp3 metadata to csv.");
            listCommand.AddArgument(pathArgument);
            listCommand.AddOption(new Option<string>(new string[] { "--outfile", "-o" }, () => "mp3info.csv", "Output file name."));
            listCommand.Handler = CommandHandler.Create((string path, string outfile) =>
            {
                appContext.Path = path;
                appContext.OutputFile = outfile;
                return processor.ProcessList(serviceProvider.GetService<CSVTrackLister>());
            });

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