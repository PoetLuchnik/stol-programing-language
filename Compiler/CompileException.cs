using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOL.Compiler
{
    public enum ErrCode
    {
        NameNotFound,
        BadTokenArea,
        InvalidNumber,
        InvalidArgument,
        NotByteValue,
        ProcBodyNotFound,
        ProcInProc,
        ProcNameNotFound,
        KeyProc,
        LackBracket,
        ExcessBracket,
        ArgNotFound,
        FileNotFound,
        OneCharOnly,
        UndefinedProcess,
        UndefinedVariable,
        UndefinedDotCode,
        UndefinedEntryPoint,
        UndefinedCommand,
        UndefinedBody,
        Count
    }
    public class CompileException : Exception
    {
        public static string GetMessage(ErrCode code, string arg)
        {
            switch(code)
            {
                case ErrCode.UndefinedBody: return $"Undefined body for {arg}";
                case ErrCode.NotByteValue: return $"Not byte value '{arg}' need [0; 256)";
                case ErrCode.InvalidArgument: return $"Bad argument {arg}";
                case ErrCode.InvalidNumber: return $"Invalid number '{arg}'";
                case ErrCode.UndefinedDotCode: return $"Undefined dot-code '{arg}'";
                case ErrCode.UndefinedEntryPoint: return $"Undefined entrypoint '{arg}'";
                case ErrCode.ProcInProc: return "Process cannot be defined in another process";
                case ErrCode.NameNotFound: return "Name not found";
                case ErrCode.OneCharOnly: return "One char required only";
                case ErrCode.ProcBodyNotFound: return "Process body not found";
                case ErrCode.BadTokenArea: return $"\'{arg}\' cannot be here";
                case ErrCode.KeyProc: return $"Process name cannot be \'{arg}\'";
                case ErrCode.LackBracket: return "Not found lack close bracket \'}\'";
                case ErrCode.ExcessBracket: return "Found excess close bracket \'}\'";
                case ErrCode.ArgNotFound: return $"No argument";
                case ErrCode.UndefinedProcess: return $"Process \'{arg}\' undefined";
                case ErrCode.UndefinedVariable: return $"Variable \'{arg}\' undefined";
                case ErrCode.UndefinedCommand: return $"Command \'{arg}\' undefined";
                case ErrCode.FileNotFound: return $"File \'{arg}\' not found";
                default: return "Undefined exception";
            }
        }
        public CompileException(string file, int line, ErrCode code) 
            : this(file, line, code, "") { }
        public CompileException(string file, int line, ErrCode code, string arg) : base()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($"\n\nError\n\nFile: \'{file}\' (Line: {line})\n{GetMessage(code, arg)}");
            Console.ReadLine();
            Environment.Exit((int)code);
        }
        public CompileException(ErrCode code, string arg) : base()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($"\n\nError\n\n{GetMessage(code, arg)}");
            Console.ReadLine();
            Environment.Exit((int)code);
        }
    }
}
