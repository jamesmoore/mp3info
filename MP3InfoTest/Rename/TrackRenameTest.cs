using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using MP3Info.Rename;
using System;
using System.IO;
using System.IO.Abstractions;

namespace MP3InfoTest.Rename
{
    [TestClass]
    public class TrackRenameTest
    {
        private const string expectedDestination = @".\Musick's Recreation. Milena Cord-to-Krax\Una Reverencia a Bach\0001 qfBRTb9LheSXVPw2QBB7bgY7k4GlDjsHPl48C7jFfqU=.mp3";

        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public void TrackHashWriter_Test(bool whatif, bool expectDestination)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var fileSystem = new FileSystem();
            var trackLoader = new TrackLoader(fileSystem);
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var hashWriter = new TrackHashWriter(false, false);

            hashWriter.ProcessTrack(track, ".");

            var sut = new TrackRenamer(fileSystem, whatif);

            sut.ProcessTrack(track, ".");

            Assert.AreEqual(expectDestination, File.Exists(expectedDestination));
            File.Delete(testFilename);
            if (expectDestination)
            {
                File.Delete(expectedDestination);
            }
        }
    }
}
