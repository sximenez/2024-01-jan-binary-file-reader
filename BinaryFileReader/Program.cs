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
            foreach (var entry in Signatures)
            {
                byte[] signature = entry.Key;
                if (input.Length >= signature.Length)
                {
                    if (input.Take(signature.Length).SequenceEqual(signature))
                    {
                        return entry.Value;
                    }
                }
            }

            return string.Empty;
        }

        internal void AmendExtension(string filePath, string extension)
        {
            string actual = Path.GetExtension(filePath);
            string expected = Path.ChangeExtension(filePath, extension);

            if (actual == extension)
            {
                return;
            }

            File.Move(filePath, expected);
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
            try
            {
                var reader = new Program(@"");

                for (int i = 0; i < reader.Files.Length; i++)
                {
                    string filePath = reader.Files[i];

                    var byteSequence = reader.ReadFile(filePath);
                    string extension = reader.FindExtensionFromSignature(byteSequence);
                    reader.AmendExtension(filePath, extension);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}