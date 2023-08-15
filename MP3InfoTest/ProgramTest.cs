using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
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
			const string Filename = TestTracks.TEST_MP3_NAME;

			const string testFileName = @"c:\temp\mp3s\testfile.mp3";
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
			{
				{ testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
			});

			var directoryProcessor = A.Fake<IDirectoryProcessor>();
			A.CallTo(() => directoryProcessor.ProcessList(A<string>._, A<ITrackListProcessor>._, A<bool>._)).Returns(1);

			var sut = new Program();

			var result = await sut.Main(commandLine.Split(' '), directoryProcessor, fileSystem);

			// Assert.AreEqual(1, result); System.CommandLine no longer supports int return values

			A.CallTo(() => directoryProcessor.ProcessList(A<string>._, A<ITrackListProcessor>._, A<bool>._)).MustHaveHappened(processorExecutionCount, Times.Exactly);

		}
	}
}
