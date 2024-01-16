using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryFileReader.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        private Program mock;
        private string filepath;
        private string directory;

        [TestInitialize()]
        public void Init()
        {
            mock = new Program();

            filepath = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files\jesus-curiel-1YpDkYsoggw-unsplash.jpg";

            directory = @"C:\Users\steven.jimenez\source\repos\2024-01-jan-binary-file-reader\test_files";

            mock.ReadDirectory(directory);
        }

        [TestMethod()]
        public void Folder_Should_Output_Dir_Files_When_Init()
        {
            Console.WriteLine(string.Join("\n", mock.Folder));
        }

        [TestMethod()]
        public void Dictionary_Should_Output_Iso_KeyPair_When_Init()
        {
            foreach (var entry in mock.IsoValues)
            {
                foreach (byte b in entry.Key)
                {
                    Console.Write($"{b} ");
                }

                Console.WriteLine($"{entry.Value}");
            }
        }

        [TestMethod()]
        public void ToHexadecimal_Should_Output_When_Populated()
        {
            mock.Input = "%PDF-";
            string[] result = mock.ToHexadecimal(mock.Input);
            Console.WriteLine(string.Join(" ", result));
        }

        [TestMethod()]
        public void ToDecimal_Should_Output_When_Populated()
        {
            mock.Input = "PNG";
            byte[] result = mock.ToDecimal(mock.Input);
            Console.WriteLine(string.Join(" ", result));
        }

        [TestMethod()]
        public void ReadFile_Should_Read_All_Files_When_Directory_Exists()
        {
            if (mock.Folder != Array.Empty<string>())
            {
                foreach (var file in mock.Folder)
                {
                    mock.ReadFile(file);
                }

            }
            else
            {
                Assert.Fail($"File doesn't exist: {filepath}");
            }

            foreach (var entry in mock.Output)
            {
                Console.WriteLine(string.Join(" ", entry));
            }

        }

        [TestMethod()]
        public void ReadFile_Should_Read_Individual_File_When_FilePath_Exists()
        {
            if (File.Exists(filepath))
            {
                mock.ReadFile(filepath);

                Console.WriteLine(string.Join(" ", mock.Output[0]));
            }
            else
            {
                Assert.Fail($"File doesn't exist: {filepath}");
            }
        }

        [TestMethod()]
        public void FindExtension_Should_Output_Extension_In_Signature()
        {
            if (File.Exists(filepath))
            {
                mock.ReadFile(filepath);
                mock.FindExtensionFromSignature(mock.Output[0]);
                Console.WriteLine(mock.Extension);
            }
            else
            {
                Assert.Fail($"File doesn't exist: {filepath}");
            }
        }

        [TestMethod()]
        public void FindExtension_Should_Output_All_File_Extensions_In_Signature()
        {
            if (mock.Folder != Array.Empty<string>())
            {
                for (int i = 0; i < mock.Folder.Length; i++)
                {
                    mock.ReadFile(mock.Folder[i]);
                    mock.FindExtensionFromSignature(mock.Output[i]);
                    Console.WriteLine(mock.Extension);
                }
            }
            else
            {
                Assert.Fail($"File doesn't exist: {filepath}");
            }
        }

        [TestMethod()]
        public void AmendExtension_Should_Output_True_When_Different()
        {
            if (mock.Folder != Array.Empty<string>())
            {
                for (int i = 0; i < mock.Folder.Length; i++)
                {
                    mock.ReadFile(mock.Folder[i]);
                    mock.FindExtensionFromSignature(mock.Output[i]);
                    mock.AmendExtension(mock.Folder[i]);
                }
            }
            else
            {
                Assert.Fail($"File doesn't exist: {filepath}");
            }
        }
    }
}