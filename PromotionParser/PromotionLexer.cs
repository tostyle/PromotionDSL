using System;
using System.Collections.Generic;
using System.Text;

namespace PromotionParser
{
    public enum TokenType
    {
        // Literals
        IDENTIFIER,
        NUMBER,
        STRING,

        // Keywords
        ANY,
        ALL,

        // Operators
        AND,           // &&
        OR,            // ||
        EQUALS,        // =
        GREATER,       // >
        LESS,          // <
        GREATER_EQUAL, // >=
        LESS_EQUAL,    // <=
        NOT_EQUAL,     // !=

        // Delimiters
        DOT,           // .
        LPAREN,        // (
        RPAREN,        // )

        // Special
        SEPARATOR,     // ---
        WHITESPACE,
        EOF
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"Token({Type}, '{Value}', {Line}:{Column})";
        }
    }

    public class PromotionLexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;
        private readonly List<Token> _tokens;

        public PromotionLexer(string input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _position = 0;
            _line = 1;
            _column = 1;
            _tokens = new List<Token>();
        }

        public List<Token> Tokenize()
        {
            while (_position < _input.Length)
            {
                if (char.IsWhiteSpace(Current))
                {
                    SkipWhitespace();
                }
                else if (char.IsLetter(Current) || Current == '_')
                {
                    ReadIdentifier();
                }
                else if (char.IsDigit(Current))
                {
                    ReadNumber();
                }
                else if (Current == '"')
                {
                    ReadString();
                }
                else if (Current == '&' && Peek() == '&')
                {
                    AddToken(TokenType.AND, "&&");
                    Advance(2);
                }
                else if (Current == '|' && Peek() == '|')
                {
                    AddToken(TokenType.OR, "||");
                    Advance(2);
                }
                else if (Current == '>' && Peek() == '=')
                {
                    AddToken(TokenType.GREATER_EQUAL, ">=");
                    Advance(2);
                }
                else if (Current == '<' && Peek() == '=')
                {
                    AddToken(TokenType.LESS_EQUAL, "<=");
                    Advance(2);
                }
                else if (Current == '!' && Peek() == '=')
                {
                    AddToken(TokenType.NOT_EQUAL, "!=");
                    Advance(2);
                }
                else if (Current == '-' && Peek() == '-' && PeekAt(2) == '-')
                {
                    AddToken(TokenType.SEPARATOR, "---");
                    Advance(3);
                }
                else if (Current == '=')
                {
                    AddToken(TokenType.EQUALS, "=");
                    Advance();
                }
                else if (Current == '>')
                {
                    AddToken(TokenType.GREATER, ">");
                    Advance();
                }
                else if (Current == '<')
                {
                    AddToken(TokenType.LESS, "<");
                    Advance();
                }
                else if (Current == '.')
                {
                    AddToken(TokenType.DOT, ".");
                    Advance();
                }
                else if (Current == '(')
                {
                    AddToken(TokenType.LPAREN, "(");
                    Advance();
                }
                else if (Current == ')')
                {
                    AddToken(TokenType.RPAREN, ")");
                    Advance();
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected character '{Current}' at line {_line}, column {_column}");
                }
            }

            AddToken(TokenType.EOF, "");
            return _tokens;
        }

        private char Current => _position < _input.Length ? _input[_position] : '\0';

        private char Peek(int offset = 1)
        {
            int peekPos = _position + offset;
            return peekPos < _input.Length ? _input[peekPos] : '\0';
        }

        private char PeekAt(int offset)
        {
            int peekPos = _position + offset;
            return peekPos < _input.Length ? _input[peekPos] : '\0';
        }

        private void Advance(int count = 1)
        {
            for (int i = 0; i < count && _position < _input.Length; i++)
            {
                if (_input[_position] == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
                _position++;
            }
        }

        private void SkipWhitespace()
        {
            while (_position < _input.Length && char.IsWhiteSpace(Current))
            {
                Advance();
            }
        }

        private void ReadIdentifier()
        {
            var start = _position;
            var startColumn = _column;

            while (_position < _input.Length && (char.IsLetterOrDigit(Current) || Current == '_'))
            {
                Advance();
            }

            string value = _input.Substring(start, _position - start);
            TokenType type = value.ToLower() switch
            {
                "any" => TokenType.ANY,
                "all" => TokenType.ALL,
                _ => TokenType.IDENTIFIER
            };

            _tokens.Add(new Token(type, value, _line, startColumn));
        }

        private void ReadNumber()
        {
            var start = _position;
            var startColumn = _column;

            while (_position < _input.Length && char.IsDigit(Current))
            {
                Advance();
            }

            // Handle decimal numbers
            if (_position < _input.Length && Current == '.')
            {
                Advance(); // consume '.'
                while (_position < _input.Length && char.IsDigit(Current))
                {
                    Advance();
                }
            }

            string value = _input.Substring(start, _position - start);
            _tokens.Add(new Token(TokenType.NUMBER, value, _line, startColumn));
        }

        private void ReadString()
        {
            var startColumn = _column;
            Advance(); // consume opening quote
            var start = _position;

            while (_position < _input.Length && Current != '"')
            {
                if (Current == '\n')
                {
                    throw new InvalidOperationException($"Unterminated string at line {_line}");
                }
                Advance();
            }

            if (_position >= _input.Length)
            {
                throw new InvalidOperationException($"Unterminated string at line {_line}");
            }

            string value = _input.Substring(start, _position - start);
            Advance(); // consume closing quote

            _tokens.Add(new Token(TokenType.STRING, value, _line, startColumn));
        }

        private void AddToken(TokenType type, string value)
        {
            _tokens.Add(new Token(type, value, _line, _column));
        }
    }
}
