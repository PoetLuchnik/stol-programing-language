using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    public enum CmdType
    {
        PRC_END = 1,
        PRC_IND = 2,
        RUN_ID = 3,
        PRC_STP = 4,
        BYTE_I = 5,
        THIS_I = 6,
        OUT_3C = 7,
        AT_3C = 8,
        CLR_OUT = 9,
        PUSH_I = 10,
        POP_I = 11,
        PUSH_V = 12,
        IS_I = 13,
        IS_V = 14,
        NOT_B = 15,
        TO_B = 16,
        POP = 17,
        CLR_STK = 18,
        OUT_B = 19,
        INPUT_B = 20,
        INPUT = 21,
        ADD_I = 22,
        SUB_I = 23,
        ADD_V = 24,
        SUB_V = 25,
        INC_B = 26,
        DEC_B = 27,
        PUSH_T = 28,
        POP_T = 29,
        MUL_I = 30,
        MUL_V = 31,
        DIV_I = 32,
        DIV_V = 33,
        MOD_B = 34,
        MOD_C = 35,
        PEEK_B = 36,
        IF_C = 37,
        IF_B = 38,
        IF_HAS = 39,
        IF_ELSE = 40,
        IF_END = 41,
        WHILE_C = 42,
        WHILE_B = 43,
        WHL_HAS = 44,
        WHL_END = 45,
        /* stream name from stack */
        SETOUT = 46, //TODO
        SETIN = 47, //TODO
        //
        FLIP = 48,
        //
        IF_POP = 49,
        WHL_POP = 50,
        //First cmd: 83 84 79 76 ("STOL")
        STOLHDR = 83,
        UNDEFINED,
    }
    public class Cmd
    {
        public static Cmd FirstCommand
        {
            get
            {
                return From(CmdType.STOLHDR, 84, 79, 76);
            }
        }
        public CmdType Type { get; private set; }
        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte Z { get; private set; }
        public Cmd(CmdType type, byte z, byte y, byte x)
        {
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        static public Cmd From(CmdType type, byte z, byte y, byte x)
        {
            return new Cmd(type, z, y, x);
        }
        static public Cmd From(CmdType type, byte y, byte x)
        {
            return new Cmd(type, 0, y, x);
        }
        static public Cmd From(CmdType type, byte x)
        {
            return new Cmd(type, 0, 0, x);
        }
        static public Cmd From(CmdType type)
        {
            return new Cmd(type, 0, 0, 0);
        }
        static public Cmd From(CmdType type, uint u)
        {
            return new Cmd(type, u.At(2), u.At(1), u.At(0));
        }
        public byte[] Bytes
        {
            get
            {
                byte[] bs = new byte[4];
                bs[0] = (byte)this.Type;
                bs[1] = this.Z;
                bs[2] = this.Y;
                bs[3] = this.X;
                return bs;
            }
        }
    }
}
