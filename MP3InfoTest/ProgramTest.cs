using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MP3Info;
using MP3Info.Hash;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;

namespace MP3InfoTest
{
    [TestClass]
    public class ProgramTest
    {
        [DataTestMethod]
        [DataRow(@"fix c:\temp", 1)]
        [DataRow(@"validate c:\temp", 1)]
        [DataRow(@"list c:\temp", 1)]
        [DataRow(@"listdupes c:\temp", 1)]
        [DataRow(@"aaaa c:\temp", 0)]
        public async Task Test_Program(string commandLine, int processorExecutionCount)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            const string testFileName = @"c:\temp\mp3s\testfile.mp3";
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var appContext = new MP3Info.AppContext();

            var directoryProcessor = new Mock<IDirectoryProcessor>();
            directoryProcessor.Setup(p => p.ProcessList(It.IsAny<ITrackListProcessor>())).Returns(1);

            var mock = new Mock<IServiceProvider>();
            mock.Setup(p => p.GetService(typeof(ListDupes))).Returns(new ListDupes());

            mock.Setup(p => p.GetService(typeof(TrackHashValidator))).Returns(new TrackHashValidator(appContext));
            var sut = new Program(directoryProcessor.Object, fileSystem, mock.Object, appContext);

            var result = await sut.Start(commandLine.Split(' '));

            Assert.AreEqual(1, result);

            directoryProcessor.Verify(p => p.ProcessList(It.IsAny<ITrackListProcessor>()), Times.Exactly(processorExecutionCount));

        }
    }
}
