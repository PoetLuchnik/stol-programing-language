using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    //.define NAME 
    public class LexDef
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public LexDef(string dc, string F, int L)
        {
            int i = 0;
            while(!char.IsWhiteSpace(dc[i]))
                i++;
            while (char.IsWhiteSpace(dc[i]))
                i++;

            if (Token.TypeFrom(dc[i]) != TknType.Word)
                new CompileException(F, L, ErrCode.NameNotFound);

            //read name
            Name = "";
            while (i < dc.Length && !dc[i].IsWordEnd())
                Name += dc[i++];
            //read value
            Value = "";
            while (i < dc.Length)
                Value += dc[i++];
        }
    }
}
