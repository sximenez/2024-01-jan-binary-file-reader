using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]

namespace BinaryFileReader
{
    public class Program
    {
        public string[] Files { get; private set; }
        public Dictionary<byte[], string> Signatures { get; }

        internal byte[] ReadFile(string filePath)
        {
            // FileMode = how to open the file.
            // FileAccess = what can be done after opening the file.

            byte[] sequence = new byte[10];

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            using (reader)
            {
                for (var i = reader.BaseStream.Position; i < 10; i++)
                {
                    sequence[i] = reader.ReadByte();
                }
            }

            return sequence;
        }

        internal string FindExtensionFromSignature(byte[] input)
        {
            string extension = string.Empty;
            foreach (var entry in Signatures)
            {
                byte expected = entry.Key[0];

                for (int i = 0; i < input.Length; i++)
                {
                    byte actual = input[i];

                    if (actual == expected)
                    {
                        for (int j = 1; j < entry.Key.Length; j++)
                        {
                            byte next = input[i + j];

                            if (next != entry.Key[j])
                            {
                                break;
                            }
                        }

                        extension = entry.Value;
                        break;
                    }
                }

                if (extension.Length != 0)
                {
                    break;
                }
            }

            return extension;
        }

        internal void AmendExtension(string filePath, string extension)
        {
            string expected = Path.GetExtension(filePath);

            if (expected != extension)
            {
                string actual = Path.ChangeExtension(filePath, extension);
                File.Move(filePath, actual);
                Console.WriteLine($"File amended: {actual}");
            }
            else
            {
                Console.WriteLine("Coherent file extension.");
            }
        }

        internal byte[] ToDecimal(string input)
        {
            List<byte> output = new List<byte>();

            foreach (char c in input)
            {
                output.Add((byte)c);
            }

            return output.ToArray();
        }

        internal string[] ToHexadecimal(string input)
        {
            List<string> output = new List<string>();

            foreach (char c in input)
            {
                var byteValue = (byte)c;
                output.Add(byteValue.ToString("X2"));
            }

            return output.ToArray();
        }

        public Program(string dirPath)
        {
            Files = Directory.GetFiles(dirPath);
            Signatures = new Dictionary<byte[], string>
            {
                { new byte[] { 37, 80, 68, 70 }, ".pdf"},
                { new byte[] { 255, 216 }, ".jpg"},
                { new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, ".png"}
            };
        }

        public static void Main(string[] args)
        {
            var reader = new Program(@"C:\Users\steven.jimenez\source\exports\cle-medicalnet-mignon\test");

            for (int i = 0; i < reader.Files.Length; i++)
            {
                string filePath = reader.Files[i];

                var byteSequence = reader.ReadFile(filePath);
                string extension = reader.FindExtensionFromSignature(byteSequence);
                reader.AmendExtension(filePath, extension);
            }
        }
    }
}