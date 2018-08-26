using System;
using System.Linq;

namespace Bitsy.CS
{
    internal class Lexer
    {
        public IToken Current { get; private set; }

        private readonly string Code;
        private int Index = 0;

        public Lexer(string code)
        {
            Code = code;
            Advance();
        }

        public void Advance() => Current = TakeNext();

        private IToken TakeNext()
        {
            char c = Code[Index];

            if (int.TryParse(c.ToString(), out int n))
            {
                string passstr = n.ToString();
                while (int.TryParse(Code[++Index].ToString(), out n))
                    passstr += n.ToString();
                return new IntLiteral(passstr);
            }

            else if (char.IsWhiteSpace(c))
            {
                while (char.IsWhiteSpace(Code[++Index])) { }
                return TakeNext();
            }

            else if (c == '_' || char.IsLetter(c))
            {
                string ident = c.ToString();
                while (char.IsLetter(Code[++Index]))
                    ident += Code[Index];
                try { return new Keyword(ident); }
                catch { return new Variable(ident); }
            }

            else if (c == '(' || c == ')')
                return new Paren(Code[Index++]);

            else if (c == '{')
            {
                while (Code[++Index] != '}') { }
                Index++;
                return TakeNext();
            }

            else if (new char[] { '+', '-', '*', '/', '%', '=' }.Contains(c))
                return new Operator(Code[Index++]);

            else
                throw new ArgumentException($"Illegal character '{c}'.");
        }
    }

    #region Tokens
    interface IToken
    {
        TokenType Type { get; }
        string Value { get; }
    }

    struct IntLiteral : IToken
    {
        public TokenType Type => TokenType.IntLiteral;
        public string Value { get; private set; }
        public IntLiteral(string value) => Value = value;
    }

    struct Variable : IToken
    {
        public TokenType Type => TokenType.Variable;
        public string Value { get; private set; }
        public Variable(string value) => Value = value;
    }

    struct Whitespace : IToken
    {
        public TokenType Type => TokenType.Whitespace;
        public string Value { get; private set; }
        public Whitespace(string value) => Value = value;
    }

    struct Comment : IToken
    {
        public TokenType Type => TokenType.Comment;
        public string Value { get; private set; }
        public Comment(string value) => Value = value;
    }

    struct Keyword : IToken
    {
        public TokenType Type { get; private set; }
        public string Value => Enum.GetName(typeof(TokenType), Type);
        public Keyword(string kw)
        {
            if (Enum.TryParse(kw, true, out TokenType tmp))
                Type = tmp;
            else
                throw new ArgumentException($"Invalid keyword {kw}.");
        }
    }

    struct Paren : IToken
    {
        public TokenType Type { get; private set; }
        public string Value => Enum.GetName(typeof(TokenType), Type);
        public Paren(char kw)
        {
            switch (kw)
            {
                case '(':
                    Type = TokenType.LeftParen;
                    break;
                case ')':
                    Type = TokenType.RightParen;
                    break;
                default:
                    if (Enum.TryParse(kw.ToString(), true, out TokenType tmp))
                        Type = tmp;
                    else
                        throw new ArgumentException($"Invalid Paren {kw}.");
                    break;
            }
        }
    }

    struct Operator : IToken
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public Operator(char kw)
        {
            Value = kw.ToString();
            
            switch (kw)
            {
                case '+':
                    Type = TokenType.Plus;
                    break;
                case '-':
                    Type = TokenType.Dash;
                    break;
                case '*':
                    Type = TokenType.Star;
                    break;
                case '/':
                    Type = TokenType.Slash;
                    break;
                case '%':
                    Type = TokenType.Percent;
                    break;
                case '=':
                    Type = TokenType.Equals;
                    break;
                default:
                    throw new ArgumentException("Sanity check failed.");
            }
        }
    }

    enum TokenType
    {
        Whitespace,
        Variable,
        IntLiteral,
        Comment,
        LeftParen,
        RightParen,
        Plus,
        Dash,
        Star,
        Slash,
        Percent,
        Equals,
        Begin,
        End,
        IfP,
        IfZ,
        IfN,
        Else,
        Loop,
        Break,
        Print,
        Read
    }
    #endregion
}