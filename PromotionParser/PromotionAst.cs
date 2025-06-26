using System;
using System.Collections.Generic;

namespace PromotionParser
{
    // AST Node base class
    public abstract class AstNode
    {
        public abstract T Accept<T>(IPromotionVisitor<T> visitor);
    }

    // Promotion Rule
    public class PromotionRule : AstNode
    {
        public string Name { get; set; }
        public string Function { get; set; } // "any" or "all"
        public List<Statement> Statements { get; set; }

        public PromotionRule(string name, string function, List<Statement> statements)
        {
            Name = name;
            Function = function;
            Statements = statements ?? new List<Statement>();
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitPromotionRule(this);
        }
    }

    // Statement (can be function call or expression)
    public abstract class Statement : AstNode
    {
    }

    // Function Call Statement
    public class FunctionCall : Expression
    {
        public string Name { get; set; }
        public List<Expression> Arguments { get; set; }

        public FunctionCall(string name, List<Expression>? arguments = null)
        {
            Name = name;
            Arguments = arguments ?? new List<Expression>();
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitFunctionCall(this);
        }
    }

    // Expression base class
    public abstract class Expression : Statement
    {
    }

    // Binary Expression (for && and || operations)
    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; } // "&&" or "||"
        public Expression Right { get; set; }

        public BinaryExpression(Expression left, string op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }

    // Comparison Expression (for =, >, <, etc.)
    public class ComparisonExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; } // "=", ">", "<", ">=", "<=", "!="
        public Expression Right { get; set; }

        public ComparisonExpression(Expression left, string op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitComparisonExpression(this);
        }
    }

    // Parenthesized Expression
    public class ParenthesizedExpression : Expression
    {
        public Expression Expression { get; set; }

        public ParenthesizedExpression(Expression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitParenthesizedExpression(this);
        }
    }

    // Property Access (object.property)
    public class PropertyAccess : Expression
    {
        public string ObjectName { get; set; }
        public string PropertyName { get; set; }

        public PropertyAccess(string objectName, string propertyName)
        {
            ObjectName = objectName;
            PropertyName = propertyName;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitPropertyAccess(this);
        }
    }

    // Identifier
    public class Identifier : Expression
    {
        public string Name { get; set; }

        public Identifier(string name)
        {
            Name = name;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }

    // Literal values
    public class NumberLiteral : Expression
    {
        public double Value { get; set; }

        public NumberLiteral(double value)
        {
            Value = value;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitNumberLiteral(this);
        }
    }

    public class StringLiteral : Expression
    {
        public string Value { get; set; }

        public StringLiteral(string value)
        {
            Value = value;
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitStringLiteral(this);
        }
    }

    // Program (collection of promotion rules)
    public class Program : AstNode
    {
        public List<PromotionRule> Rules { get; set; }

        public Program(List<PromotionRule>? rules)
        {
            Rules = rules ?? new List<PromotionRule>();
        }

        public override T Accept<T>(IPromotionVisitor<T> visitor)
        {
            return visitor.VisitProgram(this);
        }
    }
}
