using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]

namespace BinaryFileReader
{
    public class Program
    {
        public string[] Folder { get; private set; }
        public string Input { get; set; }
        public List<byte[]> Output { get; }
        public Dictionary<byte[], string> Signatures { get; }
        public string Extension { get; private set; }

        public void ReadDirectory(string path)
        {
            Folder = Directory.GetFiles(path);
        }

        internal void ReadFile(string filepath)
        {
            // FileMode = how to open the file.
            // FileAccess = what can be done after opening the file.

            List<byte> output = new List<byte>();

            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            using (reader)
            {
                for (var i = reader.BaseStream.Position; i < 10; i++)
                {
                    var entry = reader.ReadByte();
                    output.Add(entry);
                }
            }

            Output.Add(output.ToArray());
        }

        public void AmendExtension(string filepath)
        {
            string actual = Path.GetExtension(filepath);

            if (actual != Extension)
            {
                string output = Path.ChangeExtension(filepath, Extension);
                File.Move(filepath, output);
                Console.WriteLine($"File amended: {output}");
            }
            else
            {
                Console.WriteLine("Coherent file extension.");
            }
        }

        internal void FindExtensionFromSignature(byte[] input)
        {
            foreach (var entry in Signatures)
            {
                Extension = string.Empty;

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

                        Extension = entry.Value;
                        break;
                    }
                }

                if (Extension.Length != 0)
                {
                    break;
                }
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
            Folder = Array.Empty<string>();
            ReadDirectory(dirPath);
            Input = string.Empty;
            Output = new List<byte[]>();

            Signatures = new Dictionary<byte[], string>(){
                {new byte[]{37, 80, 68, 70}, ".pdf"},
                {new byte[]{255, 216}, ".jpg"},
                {new byte[]{137, 80, 78, 71, 13, 10, 26, 10}, ".png"}
            };

            Extension = string.Empty;
        }

        public static void Main(string[] args)
        {
            var reader = new Program(@"");

            for (int i = 0; i < reader.Folder.Length; i++)
            {
                reader.ReadFile(reader.Folder[i]);
                reader.FindExtensionFromSignature(reader.Output[i]);
                reader.AmendExtension(reader.Folder[i]);
            }
        }
    }
}