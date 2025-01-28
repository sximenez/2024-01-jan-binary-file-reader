using BinaryFileReader;

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
            string projectRootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));
            _directory = Path.Combine(projectRootDir, "test-files");
            _corruptedDir = Path.Combine(projectRootDir, "test-files", "corrupted");

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
            var testDir = Directory.CreateDirectory(Path.Combine(_directory, "unit-test")).FullName;
            var corruptedFiles = Directory.GetFiles(_corruptedDir);

            foreach (string file in corruptedFiles)
            {
                File.Copy(file, Path.Combine(testDir, Path.GetFileName(file)), true);
            };

            var testFiles = Directory.GetFiles(testDir);
            foreach (var file in testFiles)
            {
                var byteSequence = _mock.ReadFile(file);
                var extension = _mock.FindExtensionFromSignature(byteSequence);
                _mock.AmendExtension(file, extension);
            };

            string[] expected = _mock.Files.Select(e => Path.GetFileName(e)).ToArray();
            string[] actual = Directory.GetFiles(testDir).Select(e => Path.GetFileName(e)).ToArray();

            CollectionAssert.AreEqual(expected, actual);
            Directory.Delete(testDir, true);
        }
    }
}