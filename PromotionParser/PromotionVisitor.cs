namespace PromotionParser
{
    // Visitor interface for traversing the AST
    public interface IPromotionVisitor<T>
    {
        T VisitProgram(Program program);
        T VisitPromotionRule(PromotionRule rule);
        T VisitFunctionCall(FunctionCall functionCall);
        T VisitBinaryExpression(BinaryExpression binaryExpression);
        T VisitComparisonExpression(ComparisonExpression comparisonExpression);
        T VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression);
        T VisitPropertyAccess(PropertyAccess propertyAccess);
        T VisitIdentifier(Identifier identifier);
        T VisitNumberLiteral(NumberLiteral numberLiteral);
        T VisitStringLiteral(StringLiteral stringLiteral);
    }

    // Base visitor class with default implementations
    public abstract class PromotionVisitorBase<T> : IPromotionVisitor<T>
    {
        public virtual T? VisitProgram(Program program)
        {
            foreach (var rule in program.Rules)
            {
                rule.Accept(this);
            }
            return default(T);
        }

        public virtual T? VisitPromotionRule(PromotionRule rule)
        {
            foreach (var statement in rule.Statements)
            {
                statement.Accept(this);
            }
            return default(T);
        }

        public virtual T? VisitFunctionCall(FunctionCall functionCall)
        {
            foreach (var arg in functionCall.Arguments)
            {
                arg.Accept(this);
            }
            return default(T);
        }

        public virtual T? VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            binaryExpression.Left.Accept(this);
            binaryExpression.Right.Accept(this);
            return default(T);
        }

        public virtual T? VisitComparisonExpression(ComparisonExpression comparisonExpression)
        {
            comparisonExpression.Left.Accept(this);
            comparisonExpression.Right.Accept(this);
            return default(T);
        }

        public virtual T? VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
        {
            return parenthesizedExpression.Expression.Accept(this);
        }

        public abstract T VisitPropertyAccess(PropertyAccess propertyAccess);
        public abstract T VisitIdentifier(Identifier identifier);
        public abstract T VisitNumberLiteral(NumberLiteral numberLiteral);
        public abstract T VisitStringLiteral(StringLiteral stringLiteral);
    }

    // Example visitor implementation for pretty printing
    public class PromotionPrettyPrintVisitor : PromotionVisitorBase<string>
    {
        private int _indentLevel = 0;
        private const string IndentString = "  ";

        private string GetIndent() => new string(' ', _indentLevel * IndentString.Length);

        public override string VisitProgram(Program program)
        {
            var result = "Program:\n";
            _indentLevel++;

            foreach (var rule in program.Rules)
            {
                result += GetIndent() + rule.Accept(this) + "\n";
            }

            _indentLevel--;
            return result;
        }

        public override string VisitPromotionRule(PromotionRule rule)
        {
            var result = $"PromotionRule: {rule.Name} {rule.Function}\n";
            _indentLevel++;

            foreach (var statement in rule.Statements)
            {
                result += GetIndent() + statement.Accept(this) + "\n";
            }

            _indentLevel--;
            return result;
        }

        public override string VisitFunctionCall(FunctionCall functionCall)
        {
            var result = $"FunctionCall: {functionCall.Name}";
            if (functionCall.Arguments.Count > 0)
            {
                result += "(";
                for (int i = 0; i < functionCall.Arguments.Count; i++)
                {
                    if (i > 0) result += ", ";
                    result += functionCall.Arguments[i].Accept(this);
                }
                result += ")";
            }
            return result;
        }

        public override string VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            return $"({binaryExpression.Left.Accept(this)} {binaryExpression.Operator} {binaryExpression.Right.Accept(this)})";
        }

        public override string VisitComparisonExpression(ComparisonExpression comparisonExpression)
        {
            return $"({comparisonExpression.Left.Accept(this)} {comparisonExpression.Operator} {comparisonExpression.Right.Accept(this)})";
        }

        public override string VisitPropertyAccess(PropertyAccess propertyAccess)
        {
            return $"{propertyAccess.ObjectName}.{propertyAccess.PropertyName}";
        }

        public override string VisitIdentifier(Identifier identifier)
        {
            return identifier.Name;
        }

        public override string VisitNumberLiteral(NumberLiteral numberLiteral)
        {
            return numberLiteral.Value.ToString();
        }

        public override string VisitStringLiteral(StringLiteral stringLiteral)
        {
            return $"\"{stringLiteral.Value}\"";
        }
    }

    // Example visitor for evaluating expressions (placeholder implementation)
    public class PromotionEvaluationVisitor : PromotionVisitorBase<object>
    {
        private readonly Dictionary<string, object> _context;

        public PromotionEvaluationVisitor(Dictionary<string, object>? context = null)
        {
            _context = context ?? new Dictionary<string, object>();
        }

        public override object VisitBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = binaryExpression.Left.Accept(this);
            var right = binaryExpression.Right.Accept(this);

            return binaryExpression.Operator switch
            {
                "&&" => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                "||" => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                _ => throw new InvalidOperationException($"Unknown binary operator: {binaryExpression.Operator}")
            };
        }

        public override object VisitComparisonExpression(ComparisonExpression comparisonExpression)
        {
            var left = comparisonExpression.Left.Accept(this);
            var right = comparisonExpression.Right.Accept(this);

            return comparisonExpression.Operator switch
            {
                "=" => Equals(left, right),
                ">" => Comparer<object>.Default.Compare(left, right) > 0,
                "<" => Comparer<object>.Default.Compare(left, right) < 0,
                ">=" => Comparer<object>.Default.Compare(left, right) >= 0,
                "<=" => Comparer<object>.Default.Compare(left, right) <= 0,
                "!=" => !Equals(left, right),
                _ => throw new InvalidOperationException($"Unknown comparison operator: {comparisonExpression.Operator}")
            };
        }

        public override object? VisitPropertyAccess(PropertyAccess propertyAccess)
        {
            var key = $"{propertyAccess.ObjectName}.{propertyAccess.PropertyName}";
            return _context.TryGetValue(key, out var value) ? value : null;
        }

        public override object VisitIdentifier(Identifier identifier)
        {
            return _context.TryGetValue(identifier.Name, out var value) ? value : identifier.Name;
        }

        public override object VisitNumberLiteral(NumberLiteral numberLiteral)
        {
            return numberLiteral.Value;
        }

        public override object VisitStringLiteral(StringLiteral stringLiteral)
        {
            return stringLiteral.Value;
        }

        public override object VisitFunctionCall(FunctionCall functionCall)
        {
            // This is a placeholder - in a real implementation, you'd evaluate the function
            // For now, just return the function name
            return functionCall.Name;
        }
    }
}
