using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using System;
using System.IO;
using System.Linq;
using TagLib.Id3v2;

namespace MP3InfoTest.Hash
{
    [TestClass]
    public class TrackHashWriterTest
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TrackHashWriter_Test(bool force)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var sut = new TrackHashWriter(false, force);

            sut.ProcessTrack(track, ".");
            sut.ProcessTrack(track, ".");

            Assert.IsNotNull(track.Hash);

            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);

                var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();

                Assert.AreEqual(1, hashTextFields.Count);
                Assert.AreEqual(1, hashTextFields.Single().Text.Length);
            }

            File.Delete(testFilename);
        }

        [DataTestMethod]
        [DataRow(true, null, true)]
        [DataRow(false, null, true)]
        [DataRow(true, "", true)]
        [DataRow(false, "", true)]
        [DataRow(true, "nonsense", true)]
        [DataRow(false, "nonsense", false)]
        [DataRow(true, "du3QRdi0WaSQXtss1n6vWuZA32IvL1ufSuquFzEicqk=", false)]
        [DataRow(false, "du3QRdi0WaSQXtss1n6vWuZA32IvL1ufSuquFzEicqk=", false)]
        public void TrackHashWriter_With_Existing_Hash_Test(bool force, string existingHash, bool expectUpdate)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
                var existingHashFrame = new UserTextInformationFrame("hash")
                {
                    Text = new string[] { existingHash }
                };
                custom.AddFrame(existingHashFrame);
                file.Save();
            }

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var sut = new TrackHashWriter(false, force);

            sut.ProcessTrack(track, ".");
            Assert.IsNotNull(track.Hash);

            sut.ProcessTrack(track, ".");

            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);

                var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();

                Assert.AreEqual(1, hashTextFields.Count);

                Assert.AreEqual(expectUpdate, hashTextFields[0].Text.First() != existingHash);
            }

            File.Delete(testFilename);
        }
    }
}
