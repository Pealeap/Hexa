# Hexa

Hexa is a small text compression tool written in C# (.NET). It compresses text files by encoding each letter with a fixed **6-bit code** instead of the usual 8-bit byte, then packs those codes tightly into the output file. Characters that are not in the dictionary are stored unchanged ("raw") using an escape marker.

## How it works

### The dictionary

`Dictionary.cs` holds three lookup tables mapping characters to 6-bit codes (values `0b000000` to `0b111101`):

| Group | Bit range | Example |
|---|---|---|
| Lowercase `a`–`z` | `0b000000`–`0b011001` | `'a' → 0b000000` |
| Uppercase `A`–`Z` | `0b011010`–`0b110011` | `'Z' → 0b110011` |
| Digits `0`–`9` | `0b110100`–`0b111101` | `'5' → 0b111001` |

Two 6-bit codes are reserved as special markers:

- `0b111110` – **Space** (the space character gets its own code).
- `0b111111` – **Escape**: the next 8 bits are a raw, uncompressed byte.

The class also keeps a reverse lookup (`codeToChar`) so decompression can map a code back to its character, and covers `char`/`byte` (6-bit code) both ways through `TryGetCode` and `TryGetChar`.

### Compression (`Parser.CompressFile` → `Compressor.Package`)

1. The whole input file is read as text.
2. Each letter is translated to a token:
   - If it is in the dictionary → its 6-bit code.
   - Otherwise (accents, punctuation, newline, emoji…) → an **escape pair**: one `Escape` token followed by the raw byte. Multi-byte UTF-8 characters get one escape pair per byte.
3. `Compressor.Package` turns that list of tokens into a bit stream (6 bits per code, 6+8 bits per raw byte) and packs the bits into 8-bit bytes, **MSB (most-significant bit) first**:

   ```
   buffer = (buffer << 1) | bit;
   ```

   Every time the buffer holds 8 bits, it is flushed as an output byte.

Because most letters are stored in 6 bits, every 4 dictionary characters need only 3 bytes (24 bits) instead of 4 — about a **25% size reduction** on plain lowercase text.

### Output format (`.hexa` file)

| Field | Size | Description |
|---|---|---|
| Bit count | 4 bytes (little-endian `uint`) | Total number of payload bits, so the reader knows where useful data ends |
| Payload | n bytes | The packed bit stream |

The 4-byte header is followed by the packed bytes; the last byte is padded with zero bits if needed.

### Decompression (`Parser.DecompressFile` → `Compressor.Unpackage`)

`Unpackage` reads the header to learn exactly how many bits are valid, then walks the bit stream in the same MSB-first order:

- A code different from `Escape` → look it up and emit the matching character (as an ASCII byte).
- `Escape` → read the following 8 bits and emit them as a raw byte.

The result is the exact byte-for-byte original file.

## Usage

```
dotnet build -c Release
./bin/Release/net10.0/hexa c <input.txt> <output.hexa>   # compress
./bin/Release/net10.0/hexa d <input.hexa> <output.txt>   # decompress
```

Or via `dotnet run`:

```
dotnet run -- c in.txt out.hexa
dotnet run -- d out.hexa back.txt
```

## Project layout

| File | Purpose |
|---|---|
| `Dictionary.cs` | Character → 6-bit code tables, reverse lookup, `Space`/`Escape` markers |
| `Parser.cs` | File I/O: reads text, builds tokens, writes/reassembles bytes |
| `Compressor.cs` | Bit packing (`Package`) and unpacking (`Unpackage`) |
| `Program.cs` | CLI entry point |

## License

Hexa is licensed under the [Apache License 2.0](LICENSE).
