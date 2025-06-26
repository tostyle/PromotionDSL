using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Interface for all expressions that can be evaluated
    /// </summary>
    public interface IExpression
    {
        bool Evaluate(PromotionContext context);
        string ToString();
    }

    /// <summary>
    /// Comparison expression: left operator right (e.g., item.sku = config.sku)
    /// </summary>
    public class ComparisonExpression : IExpression
    {
        public string LeftProperty { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string RightProperty { get; set; } = string.Empty;

        public bool Evaluate(PromotionContext context)
        {
            try
            {
                var leftValue = GetPropertyValue(LeftProperty, context);
                var rightValue = GetPropertyValue(RightProperty, context);

                return CompareValues(leftValue, Operator, rightValue);
            }
            catch
            {
                return false;
            }
        }

        private object? GetPropertyValue(string propertyPath, PromotionContext context)
        {
            var parts = propertyPath.Split('.');
            if (parts.Length < 2) return null;

            var root = parts[0].ToLower();
            var property = parts[1];

            return root switch
            {
                "item" => GetItemProperty(property, context.Cart.Items.FirstOrDefault()),
                "config" => context.Config.Values.GetValueOrDefault(property),
                "cart" => GetCartProperty(property, context.Cart),
                _ => null
            };
        }

        private object? GetItemProperty(string property, CartItem? item)
        {
            if (item == null) return null;

            return property.ToLower() switch
            {
                "sku" => item.Sku,
                "price" => item.Price,
                "quantity" => item.Quantity,
                "totalprice" => item.TotalPrice,
                "name" => item.Name,
                _ => item.Properties.GetValueOrDefault(property)
            };
        }

        private object? GetCartProperty(string property, Cart cart)
        {
            return property.ToLower() switch
            {
                "totalamount" => cart.TotalAmount,
                "totalquantity" => cart.TotalQuantity,
                "itemscount" => cart.UniqueItemsCount,
                _ => null
            };
        }

        private bool CompareValues(object? left, string op, object? right)
        {
            if (left == null || right == null) return false;

            // Convert to comparable types
            if (left is string leftStr && right is string rightStr)
            {
                return op switch
                {
                    "=" => string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase),
                    "!=" => !string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }

            // Numeric comparisons
            if (TryConvertToDecimal(left, out decimal leftNum) && TryConvertToDecimal(right, out decimal rightNum))
            {
                return op switch
                {
                    "=" => leftNum == rightNum,
                    "!=" => leftNum != rightNum,
                    ">" => leftNum > rightNum,
                    "<" => leftNum < rightNum,
                    ">=" => leftNum >= rightNum,
                    "<=" => leftNum <= rightNum,
                    _ => false
                };
            }

            return false;
        }

        private bool TryConvertToDecimal(object value, out decimal result)
        {
            result = 0;
            return value switch
            {
                decimal d => (result = d) == d,
                int i => (result = i) == i,
                double db => (result = (decimal)db) == (decimal)db,
                float f => (result = (decimal)f) == (decimal)f,
                string s => decimal.TryParse(s, out result),
                _ => false
            };
        }

        public override string ToString()
        {
            return $"{LeftProperty} {Operator} {RightProperty}";
        }
    }

    /// <summary>
    /// Logical expression: left && right or left || right
    /// </summary>
    public class LogicalExpression : IExpression
    {
        public IExpression Left { get; set; } = null!;
        public string Operator { get; set; } = string.Empty; // "&&" or "||"
        public IExpression Right { get; set; } = null!;

        public bool Evaluate(PromotionContext context)
        {
            return Operator switch
            {
                "&&" => Left.Evaluate(context) && Right.Evaluate(context),
                "||" => Left.Evaluate(context) || Right.Evaluate(context),
                _ => false
            };
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }
    }

    /// <summary>
    /// Function call expression (e.g., minimumSpending config.minAmount)
    /// </summary>
    public class FunctionExpression : IExpression
    {
        public string FunctionName { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();

        public bool Evaluate(PromotionContext context)
        {
            return FunctionName.ToLower() switch
            {
                "minimumspending" => EvaluateMinimumSpending(context),
                "minimumquantity" => EvaluateMinimumQuantity(context),
                "any" => EvaluateAny(context),
                "all" => EvaluateAll(context),
                _ => false
            };
        }

        private bool EvaluateMinimumSpending(PromotionContext context)
        {
            if (Parameters.Count == 0) return false;

            var configKey = Parameters[0].Replace("config.", "");
            var minAmount = context.Config.GetValue<decimal>(configKey);

            return context.Cart.TotalAmount >= minAmount;
        }

        private bool EvaluateMinimumQuantity(PromotionContext context)
        {
            if (Parameters.Count == 0) return false;

            var configKey = Parameters[0].Replace("config.", "");
            var minQuantity = context.Config.GetValue<int>(configKey);

            return context.Cart.TotalQuantity >= minQuantity;
        }

        private bool EvaluateAny(PromotionContext context)
        {
            // For now, return true if any items exist
            return context.Cart.Items.Any();
        }

        private bool EvaluateAll(PromotionContext context)
        {
            // For now, return true if all conditions are met (simplified)
            return context.Cart.Items.Any();
        }

        public override string ToString()
        {
            return $"{FunctionName}({string.Join(", ", Parameters)})";
        }
    }

    /// <summary>
    /// Composite expression that can combine functions and expressions
    /// </summary>
    public class CompositeExpression : IExpression
    {
        public FunctionExpression? Function { get; set; }
        public IExpression? AdditionalExpression { get; set; }
        public string CombineOperator { get; set; } = "&&"; // How to combine function and expression

        public bool Evaluate(PromotionContext context)
        {
            var functionResult = Function?.Evaluate(context) ?? true;
            var expressionResult = AdditionalExpression?.Evaluate(context) ?? true;

            return CombineOperator switch
            {
                "&&" => functionResult && expressionResult,
                "||" => functionResult || expressionResult,
                _ => functionResult
            };
        }

        public override string ToString()
        {
            if (Function == null && AdditionalExpression == null) return "";
            if (Function == null) return AdditionalExpression?.ToString() ?? "";
            if (AdditionalExpression == null) return Function.ToString();

            return $"{Function} {CombineOperator} {AdditionalExpression}";
        }
    }
}
