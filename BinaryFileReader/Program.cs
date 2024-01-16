namespace BinaryFileReader
{
    public class Program
    {
        public string[] Folder { get; set; }
        public string Input { get; set; }
        public List<byte[]> Output { get; set; }
        public Dictionary<byte[], string> IsoValues { get; set; }
        public string Extension { get; set; }

        public void ReadDirectory(string path)
        {
            Folder = Directory.GetFiles(path);
        }

        public void ReadFile(string filepath)
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

        public void FindExtensionFromSignature(byte[] input)
        {
            foreach (var entry in IsoValues)
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

        public byte[] ToDecimal(string input)
        {
            List<byte> output = new List<byte>();

            foreach (char c in input)
            {
                output.Add((byte)c);
            }

            return output.ToArray();
        }

        public string[] ToHexadecimal(string input)
        {
            List<string> output = new List<string>();

            foreach (char c in input)
            {
                var byteValue = (byte)c;
                output.Add(byteValue.ToString("X2"));
            }

            return output.ToArray();
        }

        public Program()
        {
            Folder = Array.Empty<string>();
            Input = string.Empty;
            Output = new List<byte[]>();

            IsoValues = new Dictionary<byte[], string>(){
                {ToDecimal("%PDF-"), ".pdf"},
                {ToDecimal("JFIF"), ".jpg"},
                {ToDecimal("PNG"), ".png"}
            };

            Extension = string.Empty;
        }

        public static void Main(string[] args)
        {
        }
    }
}