using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    public static class Syntax
    {
        private static List<string> KeyWords;
        public const char COMMENT_MARK = '|';
        public const char STRING_MARK = '\"';
        public const char CHAR_MARK = '\'';
        public const char PREPROC_MARK = '.';
        public const char TXTCTRL_MARK = '\\';
        public const string OPERATORS = "{},;";
        public const string WORD_END = OPERATORS + "|\"\'";
        public const string DC_PASTEONCE = "append";
        public const string DC_ENTRYPOINT = "entrypoint";
        public const string DC_DEFINE = "define";
        public const string DEFAULT_ENTRYPOINT = ":main";
        /// <summary> proc [proc_name] </summary>
        public const string KW_PROCESS = "proc";
        /// <summary> byte [var_name] </summary>
        public const string KW_BYTE = "byte";
        /// <summary> pop [var_name | null] </summary>
        public const string KW_POP = "pop";
        /// <summary> this [var_name] </summary>
        public const string KW_THIS = "this";
        /// <summary> is [var_name | value] </summary>
        public const string KW_IS = "is";
        /// <summary> dec [var_name] </summary>
        public const string KW_DECREMENT = "dec";
        /// <summary> inc [var_name] </summary>
        public const string KW_INCREMENT = "inc";
        /// <summary> stop [null] </summary>
        public const string KW_STOP = "stop";
        /// <summary> if [var_name | value] </summary>
        public const string KW_IF = "if";
        /// <summary> while [var_name | value] </summary>
        public const string KW_WHILE = "while";
        /// <summary> else [null] </summary>
        public const string KW_ELSE = "else";
        /// <summary> push [var_name | value] </summary>
        public const string KW_PUSH = "push";
        /// <summary> push* [null] </summary>
        public const string KW_PUSH_THIS = "push*";
        /// <summary> pop* [null] </summary>
        public const string KW_POP_THIS = "pop*";
        /// <summary> peek [var_name] </summary>
        public const string KW_PEEK = "peek";
        /// <summary> output [var_name | 3 * value] </summary>
        public const string KW_OUTPUT = "output";
        /// <summary> input [var_name | null] </summary>
        public const string KW_INPUT = "input";
        /// <summary> mul [var_name | value] </summary>
        public const string KW_MULTIPLY = "mul";
        /// <summary> add [var_name | value] </summary>
        public const string KW_ADD = "add";
        /// <summary> sub [var_name | value] </summary>
        public const string KW_SUBTRACT = "sub";
        /// <summary> div [var_name | value] </summary>
        public const string KW_DIVIDE = "div";
        /// <summary> mod [var_name | value] </summary>
        public const string KW_MODULE = "mod";
        /// <summary> flip [null] </summary>
        public const string KW_FLIP = "flip";
        /// <summary> outclear [null] </summary>
        public const string KW_OUTCLEAR = "outclear";
        /// <summary> staclear [null] </summary>
        public const string KW_STACLEAR = "staclear";
        /// <summary> not [var_name] </summary>
        public const string KW_NOT = "not";

        /// <summary> true </summary>
        public const string KW_TRUE = "true";
        /// <summary> false </summary>
        public const string KW_FALSE = "false";

        public const string OP_HAS = "has";
        public const string OP_AND = "and";
        public const string OP_BEGIN = "{";
        public const string OP_END = "}";
        public const string OP_EXPR_SEP = ";";
        public const string OP_ARG_SEP = ",";
        public static bool IsWordEnd(this char c)
        {
            return char.IsWhiteSpace(c) || WORD_END.Contains(c);
        }
        public static bool IsOperator(this char c)
        {
            return OPERATORS.Contains(c);
        }
        public static bool IsEnumOperator(Token t)
        {
            return t.Type == TknType.Operator && 
                (t.Text == OP_AND || t.Text == OP_ARG_SEP || t.Text == OP_EXPR_SEP);
        }
        public static char ToControl(this char c)
        {
            switch (c)
            {
                case '\\': return '\\';
                case 'n': return '\n';
                case 'r': return '\r';
                case 't': return '\t';
                case '\'': return '\'';
                case '\"': return '\"';
                case 'a': return '\a';
                case 'b': return '\b';
                case 'f': return '\f';
                default: return (char)0;
            }
        }
        public static bool IsKeyWord(string word)
        {
            initWords();
            return KeyWords.Contains(word);
        }
        public static bool IsTokenHasKeyWord(Token t, string kw)
        {
            return t.Type == TknType.Word && t.Text == kw;
        }
        public static bool IsTokenHasOperator(Token t, string kw)
        {
            return t.Type == TknType.Operator && t.Text == kw;
        }
        private static void initWords()
        {
            if (KeyWords != null) return;

            KeyWords = new List<string>();
            KeyWords.Add(KW_ADD);
            KeyWords.Add(KW_BYTE);
            KeyWords.Add(KW_DECREMENT);
            KeyWords.Add(KW_DIVIDE);
            KeyWords.Add(KW_ELSE);
            KeyWords.Add(KW_IF);
            KeyWords.Add(KW_INPUT);
            KeyWords.Add(KW_IS);
            KeyWords.Add(KW_MODULE);
            KeyWords.Add(KW_MULTIPLY);
            KeyWords.Add(KW_OUTPUT);
            KeyWords.Add(KW_PEEK);
            KeyWords.Add(KW_POP);
            KeyWords.Add(KW_POP_THIS);
            KeyWords.Add(KW_PROCESS);
            KeyWords.Add(KW_PUSH);
            KeyWords.Add(KW_PUSH_THIS);
            KeyWords.Add(KW_STOP);
            KeyWords.Add(KW_SUBTRACT);
            KeyWords.Add(KW_THIS);
            KeyWords.Add(KW_WHILE);
            KeyWords.Add(KW_OUTCLEAR);
            KeyWords.Add(KW_STACLEAR);
            KeyWords.Add(KW_NOT);
            KeyWords.Add(OP_AND);
            KeyWords.Add(OP_HAS);
        }
    }
}
