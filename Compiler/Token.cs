namespace STOL.Compiler
{
    /// <summary> Token type enum </summary>
    public enum TknType
    {
        FilePtr,
        WhiteSpace,
        NewLine,
        Comment,
        Preproc,
        Word,
        Number,
        String,
        Char,
        Operator
    }
    /// <summary> Program unit </summary>
    public class Token
    {
        /// <summary> Content </summary>
        public string Text { get; private set; }
        /// <summary> Type </summary>
        public TknType Type { get; private set; }
        /// <summary> Constructor </summary>
        private Token(TknType type, string text)
        {
            this.Type = type;
            this.Text = text;
        }
        /// <summary> Get new token </summary>
        /// <param name="type">Token type</param>
        /// <param name="text">Token content</param>
        public static Token From(TknType type, string text)
        {
            return new Token(type, text);
        }
        /// <summary> Get new token without content </summary>
        /// <param name="type">Token type</param>
        public static Token From(TknType type)
        {
            return new Token(type, "");
        }
        /// <summary> Convert token to string </summary>
        public override string ToString()
        {
            if(Type == TknType.WhiteSpace || Type == TknType.NewLine)
                return $"---{this.Type.ToString()}";
            else
                return $">{this.Type.ToString()}: \"{this.Text}\"";
        }
        /// <summary> Get token type from first src character </summary>
        public static TknType TypeFrom(char startChar)
        {
            if (char.IsWhiteSpace(startChar))
                return TknType.WhiteSpace;
            if (char.IsDigit(startChar))
                return TknType.Number;
            if (startChar.IsOperator())
                return TknType.Operator;
            switch (startChar)
            {
                case Syntax.CHAR_MARK: return TknType.Char;
                case Syntax.STRING_MARK: return TknType.String;
                case Syntax.PREPROC_MARK: return TknType.Preproc;
                case Syntax.COMMENT_MARK: return TknType.Comment;
                default: return TknType.Word;
            }
        }
    }
}
