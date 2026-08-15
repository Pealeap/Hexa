using System.Text;

namespace hexa
{
    public class Parser
    {
        public static void CompressFile(string input, string output)
        {
            // turn the whole file into a string
            string originalText = File.ReadAllText(input);

            // preparing the token list (6-bit codes + raw bytes after Escape)
            List<byte> tokens = new List<byte>();

            // translation
            foreach (char letter in originalText)
            {
                if (CompressionDictionary.TryGetCode(letter, out byte code))
                {
                    tokens.Add(code);
                }
                else
                {
                    // not in the dictionary: keep the original form, byte by byte (UTF-8).
                    // each raw byte needs its own escape marker
                    foreach (byte rawByte in Encoding.UTF8.GetBytes(letter.ToString()))
                    {
                        tokens.Add(CompressionDictionary.Escape);
                        tokens.Add(rawByte);
                    }
                }
            }

            // packaging
            List<byte> packedBytes = Compressor.Package(tokens);

            // save directly
            File.WriteAllBytes(output, packedBytes.ToArray());

            Console.WriteLine("Done!");
        }

        public static void DecompressFile(string input, string output)
        {
            // reads the packed binary file
            List<byte> packaged = File.ReadAllBytes(input).ToList();

            // back to the original bytes (byte-by-byte writing preserves UTF-8)
            List<byte> originalBytes = Compressor.Unpackage(packaged);

            File.WriteAllBytes(output, originalBytes.ToArray());

            Console.WriteLine("Done!");
        }
    }
}