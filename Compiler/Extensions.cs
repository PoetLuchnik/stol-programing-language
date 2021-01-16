using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    public static class Extensions
    {
        public static string ToStr(this byte b)
        {
            switch(b)
            {
                case 10: return "\\n";
                case 13: return "\\r";
                default: return $"{(char)b}";
            }
        }
        public static byte AtOrNull(this List<byte> bs, int i)
        {
            if (i < bs.Count)
                return bs[i];
            return 0;
        }
        public static byte At(this uint u, byte i)
        {
            return (byte)((u >> (8 * i)) & 0xFF);
        }
        public static void AddCmd(this List<byte> bytes, CmdType type)
        {
            bytes.AddRange(Cmd.From(type).Bytes);
        }
        public static void AddCmd(this List<byte> bytes, CmdType type, uint u)
        {
            bytes.AddRange(Cmd.From(type, u).Bytes);
        }
        public static void AddCmd(this List<byte> bytes, CmdType type, byte x)
        {
            bytes.AddRange(Cmd.From(type, x).Bytes);
        }
        public static void AddCmd(this List<byte> bytes, CmdType type, byte y, byte x)
        {
            bytes.AddRange(Cmd.From(type, y, x).Bytes);
        }
        public static void AddCmd(this List<byte> bytes, CmdType type, byte z, byte y, byte x)
        {
            bytes.AddRange(Cmd.From(type, z, y, x).Bytes);
        }
    }
}
