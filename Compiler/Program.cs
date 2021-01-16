using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace STOL.Compiler
{
    class Program
    {
        public const string SrcFileExt = ".stol";
        public const string BinFileExt = ".stolbin";

        static uint uFrom(byte z, byte y, byte x)
        {
            return x + (uint)y * 256 + (uint)z * 65536;
        }

        static void showByteCode(ref List<byte> prg)
        {
            Console.WriteLine("\nByte Code:\n");

            for (int i = 0; i < prg.Count; i += 4)
            {
                switch ((CmdType)prg[i])
                {
                    case CmdType.PRC_IND: Console.Write("[*] "); break;
                    case CmdType.PRC_END: Console.Write(" L  "); break;
                    default: Console.Write(" |  "); break;
                }
                Console.Write($"{(CmdType)prg[i]}\t");
                Console.Write($"{uFrom(prg[i + 1], prg[i + 2], prg[i + 3])}");
                Console.Write($"\t\"{prg[i + 1].ToStr()}");
                Console.Write($"{prg[i + 2].ToStr()}");
                Console.Write($"{prg[i + 3].ToStr()}\"\n");
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            if (!File.Exists(args[0]))
                return;
            string p = args[0];

            //string p = "test\\def.stol"; //debug
            Console.WriteLine("Show byte code? (press Enter - yes / another key - no)");
            bool showBC = Console.ReadKey(true).Key == ConsoleKey.Enter;
            //Console.WriteLine("Compile...");
            List<Token> tokens = new List<Token>();
            Lexer.TokensFrom(ref tokens, p);
            List<byte> prg = new List<byte>();
            Parser.Parse(ref tokens, ref prg);
            Console.Clear();
            Console.Write("\r100%");
            
            //debug:
            //foreach (Token t in tokens)
            //    Console.WriteLine(t.ToString());
            //end
            tokens.Clear();
            //Save:
            p = Path.GetDirectoryName(p) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(p) + BinFileExt;
            File.WriteAllBytes(p, prg.ToArray());

            if (showBC)
                showByteCode(ref prg);

            prg.Clear();

            Console.ReadLine(); //debug
        }
    }
}
