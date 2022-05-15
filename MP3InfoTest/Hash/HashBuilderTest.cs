using Microsoft.VisualStudio.TestTools.UnitTesting;
using MP3Info;
using MP3Info.Hash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Id3v2;

namespace MP3InfoTest.Hash
{
    [TestClass]
    public class HashBuilderTest
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HashBuilder_Test(bool force)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var sut = new HashBuilder(false, force);

            sut.ProcessTrack(track, ".");
            sut.ProcessTrack(track, ".");

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
        [DataRow(true)]
        [DataRow(false)]
        public void HashBuilder_With_Existing_Hash_Test(bool force)
        {
            const string Filename = "Musicks_Recreation_Milena_Cord-to-Krax_-_01_-_Prelude__Tres_viste_BWV_995.mp3";

            var testFilename = Guid.NewGuid().ToString() + ".mp3";

            File.Copy(Filename, testFilename);

            var fileInfo = new FileInfo(testFilename);

            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
                var existingHash = new UserTextInformationFrame("hash")
                {
                    Text = new string[] { "nonsense" }
                };
                custom.AddFrame(existingHash);
                file.Save();
            }

            var trackLoader = new TrackLoader();
            var track = trackLoader.GetTrack(fileInfo.FullName);

            var sut = new HashBuilder(false, force);

            sut.ProcessTrack(track, ".");
            sut.ProcessTrack(track, ".");

            using (var file = TagLib.File.Create(fileInfo.FullName))
            {
                var custom = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);

                var hashTextFields = custom.GetFrames().OfType<UserTextInformationFrame>().Where(p => p.Description == "hash").ToList();

                Assert.AreEqual(1, hashTextFields.Count);

                Assert.AreEqual(force, hashTextFields[0].Text.First() != "nonsense");
            }

            File.Delete(testFilename);
        }
    }
}
