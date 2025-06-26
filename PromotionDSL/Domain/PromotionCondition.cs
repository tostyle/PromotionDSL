using System;
using System.Collections.Generic;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Represents a promotion condition that can be evaluated
    /// </summary>
    public class PromotionCondition
    {
        public string Name { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
        public IExpression? Expression { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Evaluates if this condition is met given the context
        /// </summary>
        public bool Evaluate(PromotionContext context)
        {
            if (!IsActive) return false;

            try
            {
                // Create composite expression combining function and additional expression
                var compositeExpression = new CompositeExpression
                {
                    Function = new FunctionExpression
                    {
                        FunctionName = FunctionName,
                        Parameters = Parameters
                    },
                    AdditionalExpression = Expression,
                    CombineOperator = "&&" // Default to AND combination
                };

                return compositeExpression.Evaluate(context);
            }
            catch (Exception ex)
            {
                // Log error in real implementation
                Console.WriteLine($"Error evaluating condition {Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns what data is required for this condition to be evaluated
        /// </summary>
        public List<string> GetRequiredData()
        {
            var requiredData = new List<string>();

            // Add function-specific requirements
            switch (FunctionName.ToLower())
            {
                case "minimumspending":
                    requiredData.Add("cart.totalAmount");
                    break;
                case "minimumquantity":
                    requiredData.Add("cart.totalQuantity");
                    break;
                case "any":
                case "all":
                    requiredData.Add("cart.items");
                    break;
            }

            // Add parameter requirements
            foreach (var param in Parameters)
            {
                if (param.StartsWith("config."))
                {
                    requiredData.Add(param);
                }
            }

            // Add expression requirements (simplified - would need recursive analysis)
            if (Expression != null)
            {
                requiredData.Add("expression.data");
            }

            return requiredData;
        }

        /// <summary>
        /// Validates if the condition is properly configured
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Name)) return false;
            if (string.IsNullOrEmpty(FunctionName)) return false;

            // Function-specific validation
            switch (FunctionName.ToLower())
            {
                case "minimumspending":
                case "minimumquantity":
                    return Parameters.Count > 0;
                case "any":
                case "all":
                    return true; // These can work without parameters
                default:
                    return false; // Unknown function
            }
        }

        public override string ToString()
        {
            var paramStr = Parameters.Count > 0 ? $" {string.Join(" ", Parameters)}" : "";
            var exprStr = Expression != null ? $" {Expression}" : "";
            return $"{Name} {FunctionName}{paramStr}{exprStr}";
        }
    }

    /// <summary>
    /// Factory for creating promotion conditions from DSL elements
    /// </summary>
    public static class PromotionConditionFactory
    {
        public static PromotionCondition CreateCondition(string name, string functionName, List<string> parameters, IExpression? expression = null)
        {
            var condition = new PromotionCondition
            {
                Name = name,
                FunctionName = functionName,
                Parameters = parameters,
                Expression = expression
            };

            if (!condition.IsValid())
            {
                throw new ArgumentException($"Invalid condition configuration: {condition}");
            }

            return condition;
        }

        public static PromotionCondition CreateMinimumSpendingCondition(string name, string configKey)
        {
            return CreateCondition(name, "minimumSpending", new List<string> { $"config.{configKey}" });
        }

        public static PromotionCondition CreateMinimumQuantityCondition(string name, string configKey)
        {
            return CreateCondition(name, "minimumQuantity", new List<string> { $"config.{configKey}" });
        }

        public static PromotionCondition CreateAnyItemCondition(string name, IExpression expression)
        {
            return CreateCondition(name, "any", new List<string>(), expression);
        }

        public static PromotionCondition CreateAllItemsCondition(string name, IExpression expression)
        {
            return CreateCondition(name, "all", new List<string>(), expression);
        }
    }
}
