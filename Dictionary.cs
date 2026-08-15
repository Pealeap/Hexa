namespace hexa
{
    public static class CompressionDictionary
    {
        // 6-bit values reserved as special markers
        public const byte Space = 0b111110;
        public const byte Escape = 0b111111; // the next byte comes raw, uncompressed

        // <char, 6-bit code>
        static readonly Dictionary<char, byte> lowerCase = new Dictionary<char, byte>()
    {
        { 'a', 0b000000 },
        { 'b', 0b000001 },
        { 'c', 0b000010 },
        { 'd', 0b000011 },
        { 'e', 0b000100 },
        { 'f', 0b000101 },
        { 'g', 0b000110 },
        { 'h', 0b000111 },
        { 'i', 0b001000 },
        { 'j', 0b001001 },
        { 'k', 0b001010 },
        { 'l', 0b001011 },
        { 'm', 0b001100 },
        { 'n', 0b001101 },
        { 'o', 0b001110 },
        { 'p', 0b001111 },
        { 'q', 0b010000 },
        { 'r', 0b010001 },
        { 's', 0b010010 },
        { 't', 0b010011 },
        { 'u', 0b010100 },
        { 'v', 0b010101 },
        { 'w', 0b010110 },
        { 'x', 0b010111 },
        { 'y', 0b011000 },
        { 'z', 0b011001 },
    };
        static readonly Dictionary<char, byte> upperCase = new Dictionary<char, byte>()
    {
        { 'A', 0b011010 },
        { 'B', 0b011011 },
        { 'C', 0b011100 },
        { 'D', 0b011101 },
        { 'E', 0b011110 },
        { 'F', 0b011111 },
        { 'G', 0b100000 },
        { 'H', 0b100001 },
        { 'I', 0b100010 },
        { 'J', 0b100011 },
        { 'K', 0b100100 },
        { 'L', 0b100101 },
        { 'M', 0b100110 },
        { 'N', 0b100111 },
        { 'O', 0b101000 },
        { 'P', 0b101001 },
        { 'Q', 0b101010 },
        { 'R', 0b101011 },
        { 'S', 0b101100 },
        { 'T', 0b101101 },
        { 'U', 0b101110 },
        { 'V', 0b101111 },
        { 'W', 0b110000 },
        { 'X', 0b110001 },
        { 'Y', 0b110010 },
        { 'Z', 0b110011 },
    };

        static readonly Dictionary<char, byte> numbers = new Dictionary<char, byte>()
    {
        { '0', 0b110100 },
        { '1', 0b110101 },
        { '2', 0b110110 },
        { '3', 0b110111 },
        { '4', 0b111000 },
        { '5', 0b111001 },
        { '6', 0b111010 },
        { '7', 0b111011 },
        { '8', 0b111100 },
        { '9', 0b111101 },
    };

        // inverse: 6-bit code -> char
        static readonly Dictionary<byte, char> codeToChar = new Dictionary<byte, char>();

        static CompressionDictionary()
        {
            foreach (var dictionary in new[] { lowerCase, upperCase, numbers })
                foreach (var pair in dictionary)
                    codeToChar[pair.Value] = pair.Key;
            codeToChar[Space] = ' ';
        }

        // tries to find the 6-bit code for a letter (including space)
        public static bool TryGetCode(char ch, out byte code)
        {
            if (lowerCase.TryGetValue(ch, out code)) return true;
            if (upperCase.TryGetValue(ch, out code)) return true;
            if (numbers.TryGetValue(ch, out code)) return true;
            if (ch == ' ') { code = Space; return true; }
            code = 0;
            return false;
        }

        // tries to find the letter for a 6-bit code
        public static bool TryGetChar(byte code, out char ch)
        {
            return codeToChar.TryGetValue(code, out ch);
        }
    }
}