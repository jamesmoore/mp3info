using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using TagLib.Id3v2;

namespace MP3InfoTest
{
    [TestClass]
    public class FileSystemTagLibFile_Test
    {
        [TestMethod]
        public void FileSystemTagLibFile_Save_Test()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            const string testFileName = @"c:\temp\testfile.mp3";
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });


            WriteComment(mockFileSystem, testFileName);
            WriteComment(mockFileSystem, testFileName);

            var mockFSbytes = mockFileSystem.File.ReadAllBytes(testFileName);
            File.WriteAllBytes("mockedfs.mp3", mockFSbytes);

            var realFilesystem = new FileSystem();

            const string DestFileName = "realfs.mp3";
            realFilesystem.File.Copy(Filename, DestFileName, true);

            WriteComment(realFilesystem, DestFileName);
            WriteComment(realFilesystem, DestFileName);

            var realbytes = realFilesystem.File.ReadAllBytes(DestFileName);

            Assert.AreEqual(realbytes.Length, mockFSbytes.Length);
        }

        [TestMethod]
        public void FileSystemTagLibFile_Write_Hash_Save_Test()
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            const string testFileName = @"c:\temp\testfile.mp3";
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { testFileName, new MockFileData(File.ReadAllBytes(Filename)) },
            });

            var track = new Track(mockFileSystem, testFileName);
            track.WriteHash();

            var mockFSbytes = mockFileSystem.File.ReadAllBytes(testFileName);
            File.WriteAllBytes("mockedfs.mp3", mockFSbytes);

            var realFilesystem = new FileSystem();

            const string DestFileName = "realfs.mp3";
            realFilesystem.File.Copy(Filename, DestFileName, true);

            var track2 = new Track(realFilesystem, DestFileName);
            track2.WriteHash();

            var realbytes = realFilesystem.File.ReadAllBytes(DestFileName);

            Assert.AreEqual(realbytes.Length, mockFSbytes.Length);
        }

        private void WriteComment(IFileSystem fileSystem, string filename)
        {
            var originalBytes = fileSystem.File.ReadAllBytes(filename);

            using (var tagFile = TagLib.File.Create(new FileSystemTagLibFile(fileSystem, filename)))
            {
                var custom = tagFile.GetId3v2Tag();

                var hashTextFields = custom.GetUserTextInformationFrames().Where(p => p.Description == "comment").ToList();
                foreach (var frame in hashTextFields)
                {
                    custom.RemoveFrame(frame);
                }

                var newHash = new UserTextInformationFrame("comment")
                {
                    Text = new string[] { "RANDOM COMMENT" }
                };
                custom.AddFrame(newHash);

                tagFile.RemoveTags(TagLib.TagTypes.Id3v1);
                tagFile.Save();
            }
        }

    }
}
