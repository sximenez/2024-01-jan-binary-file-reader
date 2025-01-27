using BinaryFileReader;
using System.Globalization;

namespace UnitTests
{
    [TestClass()]
    public class TestsProgram
    {
        private Program _mock;
        private string _directory;
        private string _corruptedDir;

        [TestInitialize()]
        public void Init()
        {
            _directory = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files";
            _corruptedDir = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files\corrupted";

            _mock = new Program(_directory);
        }

        [TestMethod()]
        public void Constructor_ValidDir_ValidFileNames()
        {
            var expected = new string[] {
                "jesus-curiel-1YpDkYsoggw-unsplash.jpg",
                "jesus-curiel-1YpDkYsoggw-unsplash.pdf",
                "jesus-curiel-1YpDkYsoggw-unsplash.png"
            };

            for (int i = 0; i < expected.Length; i++)
            {
                string fileName = Path.GetFileName(_mock.Files[i]);
                Assert.AreEqual(expected[i], fileName);
            }
        }

        [TestMethod()]
        public void ToHexadecimal_ValidInput_ValidOutput()
        {
            string[] expected = ["25", "50", "44", "46", "2D"];

            string _mockInput = "%PDF-";
            string[] actual = _mock.ToHexadecimal(_mockInput);

            CollectionAssert.AreEqual(expected, actual);
            Console.WriteLine(string.Join(" ", actual));
        }

        [TestMethod()]
        public void ToDecimal_ValidInput_ValidOutput()
        {
            byte[] expected = [80, 78, 71];

            string _mockInput = "PNG";
            byte[] actual = _mock.ToDecimal(_mockInput);

            CollectionAssert.AreEqual(expected, actual);
            Console.WriteLine(string.Join(" ", actual));
        }

        [TestMethod()]
        public void ReadFile_ValidDir_ValidOutput()
        {
            var expected = new List<byte[]>()
            {
                new byte[] { 255, 216, 255, 224, 0, 16, 74, 70, 73, 70 },
                new byte[] { 37, 80, 68, 70, 45, 49, 46, 55, 10, 37 },
                new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0 }
            };

            for (int i = 0; i < _mock.Files.Length; i++)
            {
                var actual = _mock.ReadFile(_mock.Files[i]);
                CollectionAssert.AreEqual(expected[i], actual);
                Console.WriteLine(string.Join(" ", actual));
            }
        }

        [TestMethod()]
        public void FindExtension_ValidDir_ValidOutput()
        {
            var expected = new string[] {
                ".jpg",
                ".pdf",
                ".png"
            };

            for (int i = 0; i < _mock.Files.Length; i++)
            {
                var byteSequence = _mock.ReadFile(_mock.Files[i]);
                var actual = _mock.FindExtensionFromSignature(byteSequence);

                Assert.AreEqual(expected[i], actual);
            }
        }

        [TestMethod()]
        public void AmendExtension_InvalidInput_ValidOutput()
        {
            var di = Directory.CreateDirectory(Path.Combine(_directory, "unit-test"));
            var corruptedFiles = Directory.GetFiles(_corruptedDir).ToList();

            foreach (string file in corruptedFiles)
            {
                File.Copy(file, Path.Combine(di.FullName, Path.GetFileName(file)), true);
            };

            foreach (var file in corruptedFiles)
            {
                var byteSequence = _mock.ReadFile(file);
                var extension = _mock.FindExtensionFromSignature(byteSequence);
                _mock.AmendExtension(file, extension);
            };

            string[] expected = _mock.Files;
            string[] actual = Directory.GetFiles(di.FullName);

            CollectionAssert.AreEqual(expected, actual);
            Directory.Delete(di.FullName, true);
        }
    }
}