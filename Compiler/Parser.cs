using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    public enum BodyType
    {
        PROCESS,
        IF,
        WHILE,
        ELSE
    }
    public static class Parser
    {
        private static List<string> defProcs;
        private static Stack<BodyType> bodies;
        public static void Parse(ref List<Token> ts, ref List<byte> code)
        {
            bodies = new Stack<BodyType>();
            code.AddRange(Cmd.FirstCommand.Bytes);
            //Check:
            checkBodies(ref ts);
            //Processes:
            seekProcDefs(ref ts);

            if (!defProcs.Contains(Lexer.EntryPoint))
                new CompileException(ErrCode.UndefinedEntryPoint, Lexer.EntryPoint);

            defProcs.Remove(Lexer.EntryPoint);
            defProcs.Insert(0, Lexer.EntryPoint);
            //
            int i = 0;
            while (i < ts.Count)
                code.AddRange(readNextProc(ref ts, ref i));
        }
        public static bool IsProcessName(string name)
        {
            return defProcs.Contains(name);
        }
        private static void addUniqueProcName(string procName)
        {
            if (!defProcs.Contains(procName))
                defProcs.Add(procName);
        }
        private static void checkBodies(ref List<Token> tokens)
        {
            int line = 1;
            string file = "[not found]";
            int lvl = 0;

            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].Type == TknType.Operator)
                {
                    if (tokens[i].Text == Syntax.OP_BEGIN) lvl++;
                    if (tokens[i].Text == Syntax.OP_END) lvl--;
                }

                if (lvl < 0)
                    new CompileException(file, line, ErrCode.ExcessBracket);

                if (tokens[i].Type == TknType.FilePtr)
                {
                    if(lvl > 0)
                        new CompileException(file, line, ErrCode.LackBracket);

                    file = tokens[i].Text;
                    line = 1;
                }

                if (tokens[i].Type == TknType.NewLine)
                    line++;
            }
        }
        private static void seekProcDefs(ref List<Token> ts)
        {
            defProcs = new List<string>();

            int L = 1;
            string F = "[not found]";
            int lvl = 0;

            for (int i = 0; i < ts.Count; i++)
            {
                if (Syntax.IsTokenHasKeyWord(ts[i], Syntax.KW_PROCESS))
                {
                    if (lvl != 0)
                        new CompileException(F, L, ErrCode.ProcInProc);

                    i++;
                    if (ts[i].Type != TknType.Word)
                        new CompileException(F, L, ErrCode.ProcNameNotFound);
                    
                    if (!Syntax.IsKeyWord(ts[i].Text))
                        addUniqueProcName(ts[i].Text);
                    else
                        new CompileException(F, L, ErrCode.KeyProc, ts[i].Text);
                    //skip args...
                    i = seekProcBodies(i, ref ts, F, ref L);
                }
                else
                {
                    if (ts[i].Type != TknType.FilePtr && ts[i].Type != TknType.NewLine && lvl == 0)
                        new CompileException(F, L, ErrCode.BadTokenArea, ts[i].Text);
                }

                if (ts[i].Type == TknType.Operator)
                {
                    if (ts[i].Text == Syntax.OP_BEGIN) lvl++;
                    if (ts[i].Text == Syntax.OP_END) lvl--;
                }

                if (ts[i].Type == TknType.FilePtr)
                {
                    F = ts[i].Text;
                    L = 1;
                }

                L += ts[i].Type == TknType.NewLine ? 1 : 0;
            }
        }
        private static int seekProcBodies(int i, ref List<Token> ts, string F, ref int L)
        {
            int oldL = L;
            int oldI = i;

            while (i < ts.Count)
            {
                if (ts[i].Type == TknType.Operator && ts[i].Text == Syntax.OP_BEGIN)
                    return i;

                if (ts[i].Type == TknType.Word && Syntax.IsKeyWord(ts[i].Text))
                {
                    if (ts[i].Text != Syntax.KW_PROCESS)
                        new CompileException(F, L, ErrCode.BadTokenArea, ts[i].Text);
                    else
                        new CompileException(F, oldL, ErrCode.ProcBodyNotFound);
                }

                if  (ts[i].Type == TknType.FilePtr)
                    new CompileException(F, inLine(ref ts, oldI), ErrCode.ProcBodyNotFound);

                if (ts[i].Type != TknType.Word && ts[i].Type != TknType.NewLine)
                    new CompileException(F, L, ErrCode.BadTokenArea, ts[i].Text);

                L += ts[i].Type == TknType.NewLine ? 1 : 0;

                i++;
            }

            new CompileException(F, oldL, ErrCode.ProcBodyNotFound);

            return -1;
        }
        private static List<byte> readNextProc(ref List<Token> ts, ref int i)
        {
            List<byte> bytes = new List<byte>();
            List<string> vars = new List<string>();
            //[ns|fs] proc name [args] [ns] { ... }
            //PROC:
            i = seekKeyWord(ref ts, i, Syntax.KW_PROCESS) + 1; //[ns|fs] proc
            if (i >= ts.Count)
                return bytes;
            uint id = (uint)getProcID(ts[i].Text); //name
            bytes.AddCmd(CmdType.PRC_IND, id);
            //ARGS:
            i++; //to args
            i = readProcArgs(ref ts, ref vars, i); //[args]
            for(int q = vars.Count -1; q >= 0; q--)
            {
                uint vid = (uint)getVarID(ref vars, vars[q]);
                bytes.AddCmd(CmdType.BYTE_I, vid);
                bytes.AddCmd(CmdType.POP_I, vid);
            }
            //BODY
            i = seekOperator(ref ts, i, Syntax.OP_BEGIN); //{
            i = readNextBody(ref ts, ref bytes, ref vars, i); //{ ... }
            //END
            bytes.AddCmd(CmdType.PRC_END);
            return bytes;
        }
        /// <summary> Proc, if, while... bodies! </summary>
        private static int readNextBody(ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            i++;
            while(!Syntax.IsTokenHasOperator(ts[i], "}"))
                i = readNextExpr(ref ts, ref bytes, ref vars, i);
            return i;
        }
        /// <summary> Proc calls or keywords </summary>
        private static int readNextExpr(ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            int argCount = 0;
            //[cmd] [ [ops | null] args | null]
            //Need command or newline:
            if (ts[i].Type == TknType.NewLine || Syntax.IsEnumOperator(ts[i]))
                return i + 1;
            if (ts[i].Type != TknType.Word)
                new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedCommand, ts[i].Text);
            //
            string cmd = ts[i].Text;
            i++; //to args
            if (IsProcessName(cmd))
            {
                //push args:
                while (!isKeyWordOrProcName(ts[i]) && !Syntax.IsTokenHasOperator(ts[i], Syntax.OP_END))
                {
                    bytes.AddRange(pushArg(ts[i], ref vars, inFile(ref ts, i), inLine(ref ts, i)));
                    i++;
                }
                    
                //call proc:
                bytes.AddCmd(CmdType.RUN_ID, (uint)getProcID(cmd));
            }
            else
            {
                switch (cmd)
                {
                    //new var
                    case Syntax.KW_BYTE: 
                        while (!isKeyWordOrProcName(ts[i]))
                        {
                            if (Syntax.IsEnumOperator(ts[i]) || ts[i].Type == TknType.NewLine)
                            {
                                i++;
                                continue;
                            }

                            if (ts[i].Type != TknType.Word)
                                new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.InvalidArgument, $"'{ts[i].Text}' for {cmd}");

                            if (!vars.Contains(ts[i].Text))
                                vars.Add(ts[i].Text);

                            bytes.AddCmd(CmdType.BYTE_I, (uint)getVarID(ref vars, ts[i].Text));
                            i++;
                        }
                        break;
                    //var or value
                    case Syntax.KW_ADD: i = readCmd(CmdType.ADD_V, CmdType.ADD_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_DIVIDE: i = readCmd(CmdType.DIV_V, CmdType.DIV_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_IS: i = readCmd(CmdType.IS_V, CmdType.IS_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_MODULE: i = readCmd(CmdType.MOD_C, CmdType.MOD_B, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_MULTIPLY: i = readCmd(CmdType.MUL_V, CmdType.MUL_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_PUSH: i = readCmd(CmdType.PUSH_V, CmdType.PUSH_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_SUBTRACT: i = readCmd(CmdType.SUB_V, CmdType.SUB_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    //roots
                    case Syntax.KW_IF: i = readIf(ref ts, ref bytes, ref vars, i); break;
                    case Syntax.KW_WHILE: i = readWhile(ref ts, ref bytes, ref vars, i); break;
                    //var only
                    case Syntax.KW_THIS: i = readVarArgOnlyCmd(CmdType.THIS_I, cmd, ref ts, ref bytes, ref vars, i, ref argCount); break;
                    case Syntax.KW_POP:
                        i = readVarArgOnlyCmd(CmdType.POP_I, cmd, ref ts, ref bytes, ref vars, i, ref argCount);
                        if (argCount == 0)
                            bytes.AddCmd(CmdType.POP);
                        break;
                    case Syntax.KW_PEEK: i = readVarArgOnlyCmd(CmdType.PEEK_B, cmd, ref ts, ref bytes, ref vars, i, ref argCount); break;
                    case Syntax.KW_DECREMENT: i = readVarArgOnlyCmd(CmdType.DEC_B, cmd, ref ts, ref bytes, ref vars, i, ref argCount); break;
                    case Syntax.KW_INCREMENT: i = readVarArgOnlyCmd(CmdType.INC_B, cmd, ref ts, ref bytes, ref vars, i, ref argCount); break;
                    case Syntax.KW_NOT: i = readVarArgOnlyCmd(CmdType.NOT_B, cmd, ref ts, ref bytes, ref vars, i, ref argCount); break;
                    case Syntax.KW_INPUT:
                        i = readVarArgOnlyCmd(CmdType.INPUT_B, cmd, ref ts, ref bytes, ref vars, i, ref argCount);
                        if (argCount == 0)
                            bytes.AddCmd(CmdType.INPUT);
                        break;
                    //var or 3 value
                    case Syntax.KW_OUTPUT: i = readOutput(CmdType.SUB_V, CmdType.SUB_I, cmd, ref ts, ref bytes, ref vars, i); break;
                    //null
                    case Syntax.KW_FLIP: bytes.AddCmd(CmdType.FLIP); break;
                    case Syntax.KW_STOP: bytes.AddCmd(CmdType.PRC_STP); break;
                    case Syntax.KW_OUTCLEAR: bytes.AddCmd(CmdType.CLR_OUT); break;
                    case Syntax.KW_STACLEAR: bytes.AddCmd(CmdType.CLR_STK); break;
                    case Syntax.KW_PUSH_THIS: bytes.AddCmd(CmdType.PUSH_T); break;
                    case Syntax.KW_POP_THIS: bytes.AddCmd(CmdType.POP_T); break;
                    //
                    default: new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedCommand, cmd); break;
                }
            }

            Console.Write($"\r{(int)((float)i * 100.0 / ts.Count)}%");
            return i;
        }
        private static int readIf(ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            //if [var | has | pop | const] { ... }
            //ARGUMENT:
            Token arg = ts[i];
            switch (arg.Type)
            {
                case TknType.Number: bytes.AddCmd(CmdType.IF_C, toByte(arg.Text, inFile(ref ts, i), inLine(ref ts, i))); break;
                case TknType.Char: bytes.AddCmd(CmdType.IF_C, (byte)arg.Text[0]); break;
                case TknType.Word:
                    switch(arg.Text)
                    {
                        case Syntax.OP_HAS: bytes.AddCmd(CmdType.IF_HAS); break;
                        case Syntax.KW_POP: bytes.AddCmd(CmdType.IF_POP); break;
                        default:
                            if (!vars.Contains(arg.Text))
                                new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedVariable, arg.Text);

                            bytes.AddCmd(CmdType.IF_B, (uint)getVarID(ref vars, arg.Text));
                            break;
                    }
                    break;
                default: new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.InvalidArgument, $"'{arg.Text}' for if"); break;
            }
            //SEEK BODY ('{'):
            i++;
            while (!Syntax.IsTokenHasOperator(ts[i], Syntax.OP_BEGIN))
            {
                if (ts[i].Type != TknType.NewLine)
                    new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedBody, "if");
                i++;
            }
            //READ BODY:
            i = readNextBody(ref ts, ref bytes, ref vars, i) + 1;
            //SEEK ELSE:
            while (!Syntax.IsTokenHasKeyWord(ts[i], Syntax.KW_ELSE))
            {
                if (ts[i].Type != TknType.NewLine)
                    break;
                i++;
            }
            //IS ELSE?
            if (Syntax.IsTokenHasKeyWord(ts[i], Syntax.KW_ELSE))
            {
                bytes.AddCmd(CmdType.IF_ELSE);
                //SEEK ELSE BODY ('{'):
                i++;
                while (!Syntax.IsTokenHasOperator(ts[i], Syntax.OP_BEGIN))
                {
                    if (ts[i].Type != TknType.NewLine)
                        new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedBody, "else");
                    i++;
                }
                //READ ELSE BODY:
                i = readNextBody(ref ts, ref bytes, ref vars, i) + 1;
            }
            //END:
            bytes.AddCmd(CmdType.IF_END);

            return i;
        }
        private static int readWhile(ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            //while [var | has | pop | const] { ... }
            //ARGUMENT:
            Token arg = ts[i];
            switch(arg.Type)
            {
                case TknType.Number: bytes.AddCmd(CmdType.WHILE_C, toByte(arg.Text, inFile(ref ts, i), inLine(ref ts, i))); break;
                case TknType.Char: bytes.AddCmd(CmdType.WHILE_C, (byte)arg.Text[0]); break;
                case TknType.Word:
                    switch (arg.Text)
                    {
                        case Syntax.OP_HAS: bytes.AddCmd(CmdType.WHL_HAS); break;
                        case Syntax.KW_POP: bytes.AddCmd(CmdType.WHL_POP); break;
                        default:
                            if (!vars.Contains(arg.Text))
                                new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedVariable, arg.Text);

                            bytes.AddCmd(CmdType.WHILE_B, (uint)getVarID(ref vars, arg.Text));
                            break;
                    }
                    break;
                default: new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.InvalidArgument, $"'{arg.Text}' for while"); break;
            }
            //SEEK BODY ('{'):
            i++;
            while(!Syntax.IsTokenHasOperator(ts[i], Syntax.OP_BEGIN))
            {
                if (ts[i].Type != TknType.NewLine)
                    new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedBody, "while");
                i++;
            }
            //READ BODY:
            i = readNextBody(ref ts, ref bytes, ref vars, i) + 1;
            //END:
            bytes.AddCmd(CmdType.WHL_END);

            return i;
        }
        private static int readOutput(CmdType cmd_v, CmdType cmd_i, string scmd, ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            List<byte> content = new List<byte>();
            while (!isKeyWordOrProcName(ts[i]) && !Syntax.IsTokenHasOperator(ts[i], Syntax.OP_END))
            {
                if (Syntax.IsEnumOperator(ts[i]))
                {
                    i++;
                    continue;
                }

                switch (ts[i].Type)
                {
                    case TknType.Char: content.Add((byte)ts[i].Text[0]); break;
                    case TknType.String:
                        for (int j = 0; j < ts[i].Text.Length; j++)
                            content.Add((byte)ts[i].Text[j]);
                        break;
                    case TknType.Number: content.Add(toByte(ts[i].Text, inFile(ref ts, i), inLine(ref ts, i))); break;
                    case TknType.Word:
                        for(int q = 0; q < content.Count; q += 3)
                            bytes.AddCmd(CmdType.OUT_3C, content.AtOrNull(q),
                                                         content.AtOrNull(q + 1),
                                                         content.AtOrNull(q + 2));
                        content.Clear();
                        //
                        if (!vars.Contains(ts[i].Text))
                            new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedVariable, ts[i].Text);
                        bytes.AddCmd(CmdType.OUT_B, (uint)getVarID(ref vars, ts[i].Text));
                        break;
                    default: break;
                }
                i++;
            }

            for (int q = 0; q < content.Count; q += 3)
                bytes.AddCmd(CmdType.OUT_3C, content.AtOrNull(q),
                                             content.AtOrNull(q + 1),
                                             content.AtOrNull(q + 2));
            content.Clear();

            return i;
        }
        private static int readCmd(CmdType cmd_v, CmdType cmd_i, string scmd, ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i)
        {
            while (!isKeyWordOrProcName(ts[i]) && !Syntax.IsTokenHasOperator(ts[i], Syntax.OP_END))
            {
                if (Syntax.IsEnumOperator(ts[i]))
                {
                    i++;
                    continue;
                }

                switch (ts[i].Type)
                {
                    case TknType.Char: bytes.AddCmd(cmd_v, (byte)ts[i].Text[0]); break;
                    case TknType.String:
                        for (int j = 0; j < ts[i].Text.Length; j++)
                            bytes.AddCmd(cmd_v, (byte)ts[i].Text[j]);
                        break;
                    case TknType.Number: bytes.AddCmd(cmd_v, toByte(ts[i].Text, inFile(ref ts, i), inLine(ref ts, i))); break;
                    case TknType.Word:
                        if (!vars.Contains(ts[i].Text))
                            new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedVariable, ts[i].Text);
                        bytes.AddCmd(cmd_i, (uint)getVarID(ref vars, ts[i].Text));
                        break;
                    default: break;
                }
                i++;
            }
            return i;
        }
        private static int readVarArgOnlyCmd(CmdType cmd, string scmd, ref List<Token> ts, ref List<byte> bytes, ref List<string> vars, int i, ref int argCount)
        {
            argCount = 0;
            while (!isKeyWordOrProcName(ts[i]) && !Syntax.IsTokenHasOperator(ts[i], Syntax.OP_END))
            {
                if (Syntax.IsEnumOperator(ts[i]) || ts[i].Type == TknType.NewLine)
                {
                    i++;
                    continue;
                }

                if (ts[i].Type != TknType.Word)
                    new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.InvalidArgument, $"'{ts[i].Text}' for {scmd}");

                if (!vars.Contains(ts[i].Text))
                    new CompileException(inFile(ref ts, i), inLine(ref ts, i), ErrCode.UndefinedVariable, ts[i].Text);

                bytes.AddCmd(cmd, (uint)getVarID(ref vars, ts[i].Text));
                argCount++;
                i++;
            }
            return i;
        }
        private static List<byte> pushArg(Token t, ref List<string> vars, string file, int line)
        {
            List<byte> code = new List<byte>();
            if (Syntax.IsEnumOperator(t))
                return code;
            //
            switch(t.Type)
            {
                case TknType.Char: code.AddCmd(CmdType.PUSH_V, (byte)t.Text[0]); break;
                case TknType.String:
                    for(int i = 0; i < t.Text.Length; i++)
                        code.AddCmd(CmdType.PUSH_V, (byte)t.Text[i]);
                    break;
                case TknType.Number: code.AddCmd(CmdType.PUSH_V, toByte(t.Text, file, line)); break;
                case TknType.Word:
                    if (!vars.Contains(t.Text))
                        new CompileException(file, line, ErrCode.UndefinedVariable, t.Text);
                    code.AddCmd(CmdType.PUSH_I, (uint)getVarID(ref vars, t.Text));
                    break;
                default: break;
            }

            return code;
        }
        private static int seekKeyWord(ref List<Token> ts, int i, string word)
        {
            while (i < ts.Count && !Syntax.IsTokenHasKeyWord(ts[i], word))
                i++;
            return i;
        }
        private static int seekOperator(ref List<Token> ts, int i, string op)
        {
            while (!Syntax.IsTokenHasOperator(ts[i], op))
                i++;
            return i;
        }
        private static int getVarID(ref List<string> vars, string varName)
        {
            for (int i = 0; i < vars.Count; i++)
                if (vars[i] == varName)
                    return i;

            new CompileException(ErrCode.UndefinedVariable, varName);
            return 0;
        }
        private static int getProcID(string procName)
        {
            for (int i = 0; i < defProcs.Count; i++)
                if (defProcs[i] == procName)
                    return i;

            new CompileException(ErrCode.UndefinedProcess, procName);
            return 0;
        }
        private static int readProcArgs(ref List<Token> ts, ref List<string> vars, int i)
        {
            while(ts[i].Type == TknType.Word)
            {
                vars.Add(ts[i].Text);
                i++;
            }
            return i;
        }

        private static bool isKeyWordOrProcName(Token t)
        {
            return t.Type == TknType.Word && (Syntax.IsKeyWord(t.Text) || defProcs.Contains(t.Text));
        }
        private static string inFile(ref List<Token> tokens, int i)
        {
            string file = "[not found]";
            for (int j = 0; j < i; j++)
                if (tokens[j].Type == TknType.FilePtr)
                    file = tokens[j].Text;
            return file;
        }
        private static int inLine(ref List<Token> ts, int i)
        {
            string file = inFile(ref ts, i);
            string fileNow = "";
            int line = 1;

            for (int j = 0; j < i; j++)
            {
                if (ts[j].Type == TknType.FilePtr)
                    fileNow = ts[j].Text;
                if (ts[j].Type == TknType.NewLine && fileNow == file)
                    line++;
            }
                
            return line;
        }
        private static byte toByte(string s, string file, int line)
        {
            int n = 0;
            try
            {
                n = Convert.ToInt32(s);
            }
            catch
            {
                new CompileException(file, line, ErrCode.InvalidNumber, s);
            }
            if (n < 0 || n > 255)
                new CompileException(file, line, ErrCode.NotByteValue, s);

            return (byte)n;
        }
    }
}
