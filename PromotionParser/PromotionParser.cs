using System;
using System.Collections.Generic;
using System.Globalization;

namespace PromotionParser
{
    public class PromotionDslParser
    {
        private readonly List<Token> _tokens;
        private int _current;

        public PromotionDslParser(List<Token> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _current = 0;
        }

        public Program Parse()
        {
            var rules = new List<PromotionRule>();

            while (!IsAtEnd())
            {
                // Skip separators between rules
                if (Check(TokenType.SEPARATOR))
                {
                    Advance();
                    continue;
                }

                if (Check(TokenType.EOF))
                    break;

                var rule = ParsePromotionRule();
                if (rule != null)
                    rules.Add(rule);
            }

            return new Program(rules);
        }

        private PromotionRule ParsePromotionRule()
        {
            // Parse: IDENTIFIER (ANY|ALL) statements
            if (!Check(TokenType.IDENTIFIER))
            {
                throw new InvalidOperationException($"Expected promotion name at line {CurrentToken.Line}");
            }

            var name = Advance().Value;

            if (!Check(TokenType.ANY) && !Check(TokenType.ALL))
            {
                throw new InvalidOperationException($"Expected 'any' or 'all' after promotion name at line {CurrentToken.Line}");
            }

            var function = Advance().Value;
            var statements = new List<Statement>();

            // Parse statements until we hit a separator or EOF
            while (!Check(TokenType.SEPARATOR) && !IsAtEnd())
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }

            return new PromotionRule(name, function, statements);
        }

        private Statement ParseStatement()
        {
            return ParseExpression();
        }

        private Expression ParseExpression()
        {
            return ParseLogicalOr();
        }

        private Expression ParseLogicalOr()
        {
            var expr = ParseLogicalAnd();

            while (Match(TokenType.OR))
            {
                var op = Previous().Value;
                var right = ParseLogicalAnd();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression ParseLogicalAnd()
        {
            var expr = ParseComparison();

            while (Match(TokenType.AND))
            {
                var op = Previous().Value;
                var right = ParseComparison();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression ParseComparison()
        {
            var expr = ParsePrimary();

            if (Match(TokenType.EQUALS, TokenType.GREATER, TokenType.LESS,
                     TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL))
            {
                var op = Previous().Value;
                var right = ParsePrimary();
                return new ComparisonExpression(expr, op, right);
            }

            return expr;
        }

        private Expression ParsePrimary()
        {
            if (Match(TokenType.LPAREN))
            {
                var expr = ParseExpression();
                if (!Match(TokenType.RPAREN))
                {
                    throw new InvalidOperationException($"Expected ')' at line {CurrentToken.Line}");
                }
                return new ParenthesizedExpression(expr);
            }

            if (Match(TokenType.NUMBER))
            {
                var value = Previous().Value;
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double numValue))
                {
                    return new NumberLiteral(numValue);
                }
                throw new InvalidOperationException($"Invalid number format: {value}");
            }

            if (Match(TokenType.STRING))
            {
                return new StringLiteral(Previous().Value);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                var identifier = Previous().Value;

                // Check if this is a property access (identifier.property)
                if (Match(TokenType.DOT))
                {
                    if (!Match(TokenType.IDENTIFIER))
                    {
                        throw new InvalidOperationException($"Expected property name after '.' at line {CurrentToken.Line}");
                    }
                    var property = Previous().Value;
                    return new PropertyAccess(identifier, property);
                }

                // Check if this is a function call followed by arguments
                if (Check(TokenType.IDENTIFIER) || Check(TokenType.NUMBER) || Check(TokenType.STRING))
                {
                    var arguments = new List<Expression>();

                    // Parse arguments until we hit an operator or end
                    while (!Check(TokenType.AND) && !Check(TokenType.OR) &&
                           !Check(TokenType.EQUALS) && !Check(TokenType.GREATER) &&
                           !Check(TokenType.LESS) && !Check(TokenType.GREATER_EQUAL) &&
                           !Check(TokenType.LESS_EQUAL) && !Check(TokenType.NOT_EQUAL) &&
                           !Check(TokenType.SEPARATOR) && !IsAtEnd() && !Check(TokenType.RPAREN))
                    {
                        var arg = ParsePrimary();
                        arguments.Add(arg);
                    }

                    return new FunctionCall(identifier, arguments);
                }

                return new Identifier(identifier);
            }

            throw new InvalidOperationException($"Unexpected token: {CurrentToken.Type} at line {CurrentToken.Line}");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return CurrentToken.Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return CurrentToken.Type == TokenType.EOF;
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Token CurrentToken => _tokens[_current];
    }
}
