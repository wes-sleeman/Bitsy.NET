using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitsy.CS
{
    static class Parser
    {
        private static Lexer Lexer { get; set; }
        private static List<string> outputbuffer;

        public static string[] Parse(Lexer lexer)
        {
            outputbuffer = new List<string>();
            Lexer = lexer;
            Program();
            return outputbuffer.ToArray();
        }

        private static void Program()
        {
            Match(TokenType.Begin);
            Setup();

            Block();

            Match(TokenType.End, false);
            Teardown();
        }

        private static void Block()
        {
            while (Lexer.Current.Type != TokenType.End && Lexer.Current.Type != TokenType.Else)
            {
                switch (Lexer.Current.Type)
                {
                    case TokenType.IfN:
                    case TokenType.IfZ:
                    case TokenType.IfP:
                        If();
                        break;

                    case TokenType.Loop:
                        Loop();
                        break;

                    case TokenType.Break:
                        Break();
                        break;

                    case TokenType.Print:
                        Print();
                        break;

                    case TokenType.Read:
                        Read();
                        break;

                    default:
                        Assignment();
                        break;
                }
            }
        }

        #region Expressions
        private static void Expression()
        {
            Term();

            while (Lexer.Current.Value == "+" || Lexer.Current.Value == "-")
            {
                Push();
                TokenType op = Lexer.Current.Type;
                Match(op);
                Term();
                Pop(op);
            }
        }

        private static void Term()
        {
            SignedFactor();

            while (Lexer.Current.Value == "*" || Lexer.Current.Value == "/" || Lexer.Current.Value == "%")
            {
                Push();
                TokenType op = Lexer.Current.Type;
                Match(op);
                Factor();
                Pop(op);
            }
        }

        private static void SignedFactor()
        {
            TokenType op = TokenType.Plus;
            if (Lexer.Current.Value == "+" || Lexer.Current.Value == "-")
            {
                op = Lexer.Current.Type;
                Match(op);
            }

            Factor();

            if (op == TokenType.Dash)
                Emit("register = -register");
        }

        private static void Factor()
        {
            if (Lexer.Current.Type == TokenType.IntLiteral)
            {
                string integer = Match(TokenType.IntLiteral);
                Emit($"register = {integer}");
            }
            else if (Lexer.Current.Type == TokenType.Variable)
            {
                string varname = Match(TokenType.Variable);
                Emit($"register = variable[\"{varname}\"]");
            }
            else
            {
                Match(TokenType.LeftParen);
                Expression();
                Match(TokenType.RightParen);
            }
        }

        private static void Push()
        {
            Emit("stack.Push(register)");
        }

        private static void Pop(TokenType? optype = null)
        {
            if (optype == null)
            {
                Emit("register = stack.Pop()");
                return;
            }
            char op;
            switch (optype.Value)
            {
                case TokenType.Plus:
                    op = '+';
                    break;
                case TokenType.Dash:
                    op = '-';
                    break;
                case TokenType.Star:
                    op = '*';
                    break;
                case TokenType.Slash:
                    op = '/';
                    break;
                case TokenType.Percent:
                    op = '%';
                    break;
                default:
                    throw new NotSupportedException("Unreachable code hit.");
            }
            Emit($"register = stack.Pop() {op} register");
        }
        #endregion

        private static void If()
        {
            TokenType condtype = Lexer.Current.Type;
            Match(condtype);
            Expression();

            string comp = "";
            switch (condtype)
            {
                case TokenType.IfN:
                    comp = "<";
                    break;
                case TokenType.IfZ:
                    comp = "==";
                    break;
                case TokenType.IfP:
                    comp = ">";
                    break;
            }

            Emit($"if (register {comp} 0)\n\t{{", false);

            Block();

            if (Lexer.Current.Type == TokenType.Else)
            {
                Match(TokenType.Else);
                Emit("}\n\telse\n\t{", false);
                Block();
            }

            Match(TokenType.End);
            Emit("}", false);
        }

        private static void Loop()
        {
            Match(TokenType.Loop);
            Emit("while (true)\n\t{", false);

            Block();

            Match(TokenType.End);
            Emit("}", false);
        }

        private static void Break()
        {
            Match(TokenType.Break);
            Emit("break");
        }

        private static void Print()
        {
            Match(TokenType.Print);
            Expression();
            Emit("Console.WriteLine(register)");
        }

        private static void Read()
        {
            Match(TokenType.Read);
            string varname = Match(TokenType.Variable);
            Emit($"variable[\"{varname}\"] = ReadIn()");
        }

        private static void Assignment()
        {
            string varname = Match(TokenType.Variable);
            Match(TokenType.Equals);
            Expression();
            Emit($"variable[\"{varname}\"] = register");
        }

        private static string Match(TokenType type, bool advance = true)
        {
            if (Lexer.Current.Type != type)
                throw new ArgumentException($"Error: Expected {type} but received {Lexer.Current.Type}.");
            string retval = Lexer.Current.Value;
            if (advance)
                Lexer.Advance();
            return retval;
        }

        private static void Emit(string output, bool semi = true) => outputbuffer.Add('\t' + output + (semi ? ";" : string.Empty));

        private static void Setup() => Emit("" +
            @"using System; using System.Collections.Generic;
namespace BitsyExec {
class VariableDictionary : Dictionary<string, int> {public new int this[string key] { get { return ContainsKey(key) ? base[key] : 0; } set { base[key] = value;} }}
class Program
{
private static int register = 0;
static readonly VariableDictionary variable = new VariableDictionary();
static readonly Stack<int> stack = new Stack<int>();
static int ReadIn() { try { return Convert.ToInt32(Console.ReadLine()); } catch { return 0; }}
public static void Main()
{

    // BEGIN USER GENERATED CODE", false);
        private static void Teardown() => Emit("" +
            @"  // END USER GENERATED CODE

}}}", false);
    }
}