namespace hexa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                switch (args[0])
                {
                    case "c": Parser.CompressFile(args[1], args[2]); break;
                    case "d": Parser.DecompressFile(args[1], args[2]); break;
                    default: PrintUsage(); break;
                }
            }
            else
            {
                PrintUsage();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  hexa c <in> <out>");
            Console.WriteLine("  hexa d <in> <out>");
        }
    }
}