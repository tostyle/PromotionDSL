using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree;
using PromotionDSL;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Maps ANTLR parse tree to promotion domain objects
    /// </summary>
    public class PromotionMapper : PromotionBaseVisitor<object>
    {
        /// <summary>
        /// Maps the entire promotion from the parse tree
        /// </summary>
        public BasePromotion MapPromotion(IParseTree parseTree)
        {
            var promotion = Visit(parseTree) as BasePromotion;

            if (promotion == null)
            {
                throw new InvalidOperationException("Failed to map promotion from parse tree");
            }

            return promotion;
        }


        public override object VisitPromotionDef(PromotionParser.PromotionDefContext context)
        {
            var promotion = new BasePromotion
            {
                Name = CleanString(context.STRING()?.GetText()),
                IsActive = true
            };

            // Map conditions
            if (context.conditionList() != null)
            {
                var conditions = Visit(context.conditionList()) as List<PromotionCondition>;
                if (conditions != null)
                {
                    foreach (var condition in conditions)
                    {
                        promotion.AddCondition(condition);
                    }
                }
            }

            // Map rewards
            if (context.rewardList() != null)
            {
                var rewards = Visit(context.rewardList()) as List<PromotionReward>;
                if (rewards != null)
                {
                    foreach (var reward in rewards)
                    {
                        promotion.AddReward(reward);
                    }
                }
            }

            return promotion;
        }

        public override object VisitConditionList(PromotionParser.ConditionListContext context)
        {
            var conditions = new List<PromotionCondition>();

            foreach (var conditionContext in context.condition())
            {
                var condition = Visit(conditionContext) as PromotionCondition;
                if (condition != null)
                {
                    conditions.Add(condition);
                }
            }

            return conditions;
        }

        // Helper to extract function info from context
        private FunctionCallInfo ExtractFunctionInfo(PromotionParser.FunctionCallContext functionCall, string contextName, string errorPrefix)
        {
            if (functionCall == null)
            {
                throw new InvalidOperationException($"Function call is required for {errorPrefix} {contextName}");
            }
            var functionInfo = Visit(functionCall) as FunctionCallInfo;
            if (functionInfo == null)
            {
                throw new InvalidOperationException($"Failed to parse function call for {errorPrefix} {contextName}");
            }
            return functionInfo;
        }

        public override object VisitCondition(PromotionParser.ConditionContext context)
        {
            var conditionName = context.IDENTIFIER()?.GetText() ?? "";
            var functionInfo = ExtractFunctionInfo(context.functionCall(), conditionName, "condition");

            var condition = new PromotionCondition
            {
                Name = conditionName,
                FunctionName = functionInfo.FunctionName,
                Parameters = functionInfo.Parameters
            };

            // Map expression if present
            if (context.expression() != null)
            {
                var expression = Visit(context.expression()) as IExpression;
                condition.Expression = expression;
            }

            return condition;
        }

        public override object VisitReward(PromotionParser.RewardContext context)
        {
            var conditionName = context.IDENTIFIER()?.GetText() ?? "";
            var functionInfo = ExtractFunctionInfo(context.functionCall(), conditionName, "reward targeting condition");

            var reward = new PromotionReward
            {
                ConditionName = conditionName,
                RewardType = functionInfo.FunctionName,
                Parameters = functionInfo.Parameters
            };

            // Map expression if present
            if (context.expression() != null)
            {
                var expression = Visit(context.expression()) as IExpression;
                reward.Expression = expression;
            }

            return reward;
        }

        public override object VisitFunctionCall(PromotionParser.FunctionCallContext context)
        {
            var functionName = context.IDENTIFIER()?.GetText() ?? "";
            var parameters = new List<string>();

            // Get property access parameters
            if (context.propertyAccess() != null)
            {
                var propertyInfo = Visit(context.propertyAccess()) as PropertyAccessInfo;
                if (propertyInfo != null)
                {
                    parameters.Add(propertyInfo.FullPath);
                }
            }

            return new FunctionCallInfo
            {
                FunctionName = functionName,
                Parameters = parameters
            };
        }

        public override object VisitPropertyAccess(PromotionParser.PropertyAccessContext context)
        {
            var properties = new List<string>();

            foreach (var identifier in context.IDENTIFIER())
            {
                properties.Add(identifier.GetText());
            }

            return new PropertyAccessInfo
            {
                Properties = properties,
                FullPath = string.Join(".", properties)
            };
        }

        public override object VisitExpression(PromotionParser.ExpressionContext context)
        {
            return Visit(context.logicalExpr());
        }

        public override object VisitLogicalExpr(PromotionParser.LogicalExprContext context)
        {
            var comparisons = new List<ComparisonExpression>();

            foreach (var comparisonContext in context.comparisonExpr())
            {
                var comparison = Visit(comparisonContext) as ComparisonExpression;
                if (comparison != null)
                {
                    comparisons.Add(comparison);
                }
            }

            // If only one comparison, return it directly
            if (comparisons.Count == 1)
            {
                return comparisons[0];
            }

            // Build logical expression tree
            if (comparisons.Count > 1)
            {
                var result = comparisons[0] as IExpression;

                for (int i = 1; i < comparisons.Count; i++)
                {
                    // Extract operator from context text (simplified approach)
                    var contextText = context.GetText();
                    var op = contextText.Contains("&&") ? "&&" : "||";

                    result = new LogicalExpression
                    {
                        Left = result,
                        Operator = op,
                        Right = comparisons[i]
                    };
                }

                return result;
            }

            return null;
        }

        public override object VisitComparisonExpr(PromotionParser.ComparisonExprContext context)
        {
            var operands = context.operand();
            if (operands?.Length == 0)
            {
                return null;
            }

            var leftOperand = Visit(operands[0]) as OperandInfo;
            if (leftOperand == null)
            {
                return null;
            }

            // Single operand case
            if (operands.Length == 1)
            {
                // This might be a simple property reference
                return new ComparisonExpression
                {
                    LeftProperty = leftOperand.Value,
                    Operator = "",
                    RightProperty = ""
                };
            }

            // Comparison case
            var rightOperand = Visit(operands[1]) as OperandInfo;
            if (rightOperand == null)
            {
                return null;
            }

            // Extract operator from context text
            var contextText = context.GetText();
            var leftText = operands[0].GetText();
            var rightText = operands[1].GetText();
            var operatorText = contextText.Replace(leftText, "").Replace(rightText, "").Trim();

            return new ComparisonExpression
            {
                LeftProperty = leftOperand.Value,
                Operator = operatorText,
                RightProperty = rightOperand.Value
            };
        }

        public override object VisitOperand(PromotionParser.OperandContext context)
        {
            if (context.propertyAccess() != null)
            {
                var propertyInfo = Visit(context.propertyAccess()) as PropertyAccessInfo;
                return new OperandInfo
                {
                    Type = "property",
                    Value = propertyInfo?.FullPath ?? ""
                };
            }
            else if (context.NUMBER() != null)
            {
                return new OperandInfo
                {
                    Type = "number",
                    Value = context.NUMBER().GetText()
                };
            }
            else if (context.STRING() != null)
            {
                return new OperandInfo
                {
                    Type = "string",
                    Value = CleanString(context.STRING().GetText())
                };
            }

            return null;
        }

        private string CleanString(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove surrounding quotes
            if (input.StartsWith("\"") && input.EndsWith("\""))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }

    /// <summary>
    /// Helper class for function call information
    /// </summary>
    internal class FunctionCallInfo
    {
        public string FunctionName { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
    }

    /// <summary>
    /// Helper class for property access information
    /// </summary>
    internal class PropertyAccessInfo
    {
        public List<string> Properties { get; set; } = new List<string>();
        public string FullPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Helper class for operand information
    /// </summary>
    internal class OperandInfo
    {
        public string Type { get; set; } = string.Empty; // "property", "number", "string"
        public string Value { get; set; } = string.Empty;
    }
}
