using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace STOL.Compiler
{
    public enum DotCodeType
    {
        APPEND,
        ENTRYPOINT,
        DEFINE,
        UNDEFINED
    }
    public static class Lexer
    {
        public static string EntryPoint { get; private set; }
        private static List<string> onceFiles;
        private static List<LexDef> defs;

        /// <summary> Convert src file to tokens and append to list </summary>
        /// <param name="tokens"> Result </param>
        /// <param name="file"> Name of src file </param>
        public static void TokensFrom(ref List<Token> tokens, string file)
        {
            if (EntryPoint == null)
                EntryPoint = Syntax.DEFAULT_ENTRYPOINT;

            string src = File.ReadAllText(file);
            string dir = Path.GetDirectoryName(file);
            int i = 0;
            int line = 1;

            if (onceFiles == null) onceFiles = new List<string>();
            if (defs == null) defs = new List<LexDef>();

            tokens.Add(Token.From(TknType.FilePtr, file));

            while (i < src.Length)
            {
                string tmp = "";
                switch (Token.TypeFrom(src[i]))
                {
                    case TknType.WhiteSpace:
                        while(i < src.Length && char.IsWhiteSpace(src[i]))
                        {
                            tmp += src[i];
                            if (src[i] == '\n')
                            {
                                tokens.Add(Token.From(TknType.NewLine));
                                line++;
                            }
                            i++;
                        }
                        break;
                    case TknType.Comment:
                        i++;
                        while (i < src.Length && src[i] != Syntax.COMMENT_MARK)
                            tmp += src[i++];
                        i++;
                        //not add comments!
                        //tokens.Add(Token.From(TokenType.Comment, tmp));
                        break;
                    case TknType.String:
                        i++;
                        while (i < src.Length && src[i] != Syntax.STRING_MARK)
                        {
                            if(src[i] == Syntax.TXTCTRL_MARK)
                                tmp += src[++i].ToControl();
                            else
                                tmp += src[i];
                            i++;
                        }
                        i++;
                        tokens.Add(Token.From(TknType.String, tmp));
                        break;
                    case TknType.Char:
                        i++;
                        while (i < src.Length && src[i] != Syntax.CHAR_MARK)
                        {
                            if (src[i] == Syntax.TXTCTRL_MARK)
                                tmp += src[++i].ToControl();
                            else
                                tmp += src[i];
                            i++;
                        }
                        if (tmp.Length != 1)
                            new CompileException(file, line, ErrCode.OneCharOnly);
                        i++;
                        tokens.Add(Token.From(TknType.Char, tmp));
                        break;
                    case TknType.Operator:
                        tokens.Add(Token.From(TknType.Operator, "" + src[i++]));
                        break;
                    case TknType.Number:
                        while (i < src.Length && char.IsDigit(src[i]))
                            tmp += src[i++];
                        tokens.Add(Token.From(TknType.Number, tmp));
                        break;
                    case TknType.Preproc:
                        while (i < src.Length && src[i] != '\n' && src[i] != '\r')
                            tmp += src[i++];

                        runDotCode(tmp, ref tokens, file, line);

                        //tokens.Add(Token.From(TknType.NewLine));
                        //line++;

                        break;
                    case TknType.Word:
                        int startI = i;
                        while (i < src.Length && !src[i].IsWordEnd())
                            tmp += src[i++];

                        switch(tmp)
                        {
                            case Syntax.OP_AND: tokens.Add(Token.From(TknType.Operator, tmp)); break;
                            case Syntax.KW_FALSE: tokens.Add(Token.From(TknType.Number, "0")); break;
                            case Syntax.KW_TRUE: tokens.Add(Token.From(TknType.Number, "1")); break;
                            default:
                                int d = seekDef(tmp);
                                if(d == -1)
                                {
                                    tokens.Add(Token.From(TknType.Word, tmp));
                                }
                                else
                                {
                                    src = src.Remove(startI, tmp.Length);
                                    src = src.Insert(startI, defs[d].Value);
                                    i = startI;
                                }
                                break;
                        }
                        break;
                }
            }
        }
        private static void runDotCode(string dc, ref List<Token> ts, string F, int L)
        {
            DotCodeType type = GetDotCodeType(dc);

            switch(GetDotCodeType(dc))
            {
                case DotCodeType.ENTRYPOINT: dc_entrypoint(dc); break;
                case DotCodeType.APPEND: dc_append(dc, ref ts, F, L); break;
                case DotCodeType.DEFINE: dc_define(dc, F, L); break;
                default: new CompileException(F, L, ErrCode.UndefinedDotCode, dc); break;
            }
        }
        private static void dc_define(string dc, string F, int L)
        {
            defs.Add(new LexDef(dc, F, L));
        }
        private static void dc_entrypoint(string dc)
        {
            if (EntryPoint == Syntax.DEFAULT_ENTRYPOINT)
                EntryPoint = GetDotCodeNameArg(dc);
        }
        private static void dc_append(string dc, ref List<Token> tokens, string F, int L)
        {
            //file exist?
            string strArg = GetDotCodeStrArg(dc), pasteFile = strArg;
            if (!File.Exists(pasteFile))
            {
                pasteFile = AppDomain.CurrentDomain.BaseDirectory + '\\' + strArg;
                if (!File.Exists(pasteFile))
                {
                    pasteFile = Path.GetDirectoryName(F) + '\\' + strArg;
                    if (!File.Exists(pasteFile))
                        new CompileException(F, L, ErrCode.FileNotFound, strArg);
                }
            }
            //is not .pasteonce file?
            if (onceFiles.Contains(pasteFile)) return;
            //paste other file tokens:
            Lexer.TokensFrom(ref tokens, pasteFile);
            tokens.Add(Token.From(TknType.FilePtr, F));
            //save, if once
            onceFiles.Add(pasteFile);
        }
        private static int seekDef(string word)
        {
            for (int i = 0; i < defs.Count; i++)
                if (defs[i].Name == word)
                    return i;
            return -1;
        }
        public static DotCodeType GetDotCodeType(string dc)
        {
            switch (GetDotCodeName(dc))
            {
                case Syntax.DC_PASTEONCE: return DotCodeType.APPEND;
                case Syntax.DC_ENTRYPOINT: return DotCodeType.ENTRYPOINT;
                case Syntax.DC_DEFINE: return DotCodeType.DEFINE;
                default: return DotCodeType.UNDEFINED;
            }
        }
        public static string GetDotCodeName(string dc)
        {
            string name = "";
            int i = 1;
            while (i < dc.Length && !dc[i].IsWordEnd())
                name += dc[i++];
            return name;
        }
        public static string GetDotCodeStrArg(string dc)
        {
            string arg = "";
            int i = 0;
            while (i < dc.Length && dc[i] != Syntax.STRING_MARK)
                i++;
            i++;
            while (i < dc.Length && dc[i] != Syntax.STRING_MARK)
            {
                if (dc[i] == Syntax.TXTCTRL_MARK)
                    arg += dc[++i].ToControl();
                else
                    arg += dc[i];
                i++;
            }
            return arg;
        }
        public static string GetDotCodeNameArg(string dc)
        {
            string arg = "";
            int i = 0;
            //seek space
            while (i < dc.Length && !char.IsWhiteSpace(dc[i]))
                i++;
            i++;
            //seek proc name
            while (i < dc.Length && char.IsWhiteSpace(dc[i]))
                i++;
            //read proc name
            while (i < dc.Length && !dc[i].IsWordEnd())
                arg += dc[i++];
            return arg;
        }
    }
}
