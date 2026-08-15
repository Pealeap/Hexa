namespace hexa
{
    public static class Compressor
    {
        // Packs the tokens into a binary file.
        // Tokens < 64 are 6-bit codes. Escape (0b111111) signals that the
        // next token is a raw 8-bit byte. The first 4 bytes of the result
        // store the total bit count so decompression knows where the real
        // data ends (the padding bits are garbage).
        public static List<byte> Package(List<byte> tokens)
        {
            List<bool> bits = new List<bool>();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == CompressionDictionary.Escape && i + 1 < tokens.Count)
                {
                    bits.AddRange(Bits(CompressionDictionary.Escape, 6));
                    bits.AddRange(Bits(tokens[++i], 8));
                }
                else
                {
                    bits.AddRange(Bits(tokens[i], 6));
                }
            }

            // header: total bit count (32 bits, little-endian)
            List<byte> result = new List<byte>();
            uint bitCount = (uint)bits.Count;
            result.Add((byte)(bitCount & 0xFF));
            result.Add((byte)((bitCount >> 8) & 0xFF));
            result.Add((byte)((bitCount >> 16) & 0xFF));
            result.Add((byte)((bitCount >> 24) & 0xFF));

            // packs the bits into bytes, MSB first
            byte buffer = 0;
            int bitsInBuffer = 0;
            foreach (bool bit in bits)
            {
                buffer = (byte)((buffer << 1) | (bit ? 1 : 0));
                bitsInBuffer++;
                if (bitsInBuffer == 8)
                {
                    result.Add(buffer);
                    buffer = 0;
                    bitsInBuffer = 0;
                }
            }
            if (bitsInBuffer > 0)
                result.Add((byte)(buffer << (8 - bitsInBuffer)));

            return result;
        }

        // Reverses Package: reads the bits and rebuilds the original file
        // bytes (decoded letters + raw bytes).
        public static List<byte> Unpackage(List<byte> packaged)
        {
            List<byte> rawBytes = new List<byte>();
            if (packaged.Count < 4) return rawBytes;

            uint totalBits = (uint)(packaged[0]
                | (packaged[1] << 8)
                | (packaged[2] << 16)
                | (packaged[3] << 24));
            int byteIdx = 4, bitPos = 0;
            uint bitsRead = 0;

            while (bitsRead + 6 <= totalBits && byteIdx < packaged.Count)
            {
                byte code = ReadBits(packaged, ref byteIdx, ref bitPos, 6);
                bitsRead += 6;

                if (code == CompressionDictionary.Escape && bitsRead + 8 <= totalBits)
                {
                    byte raw = ReadBits(packaged, ref byteIdx, ref bitPos, 8);
                    bitsRead += 8;
                    rawBytes.Add(raw);
                }
                else if (CompressionDictionary.TryGetChar(code, out char ch))
                {
                    rawBytes.Add((byte)ch); // everything in the dictionary is ASCII
                }
                else
                {
                    break;
                }
            }

            return rawBytes;
        }

        // turns the lowest `bitCount` bits of `value` into a list of bools (MSB first)
        static List<bool> Bits(int value, int bitCount)
        {
            List<bool> bits = new List<bool>(bitCount);
            for (int i = bitCount - 1; i >= 0; i--)
                bits.Add(((value >> i) & 1) == 1);
            return bits;
        }

        // reads `bitCount` bits starting at byteIdx/bitPos (MSB first)
        static byte ReadBits(List<byte> data, ref int byteIdx, ref int bitPos, int bitCount)
        {
            byte value = 0;
            for (int i = 0; i < bitCount; i++)
            {
                value = (byte)((value << 1) | ((data[byteIdx] >> (7 - bitPos)) & 1));
                bitPos++;
                if (bitPos == 8)
                {
                    bitPos = 0;
                    byteIdx++;
                }
            }
            return value;
        }
    }
}