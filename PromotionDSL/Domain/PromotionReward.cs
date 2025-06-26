using System;
using System.Collections.Generic;
using System.Linq;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Represents a promotion reward that can be applied
    /// </summary>
    public class PromotionReward
    {
        public string RewardType { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;
        public List<string> Parameters { get; set; } = new List<string>();
        public IExpression? Expression { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Applies this reward to the given context
        /// </summary>
        public AppliedReward Apply(PromotionContext context)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException($"Reward {RewardType} for condition {ConditionName} is not active");
            }

            try
            {
                return RewardType.ToLower() switch
                {
                    "discount" => ApplyDiscount(context),
                    "discountpercentage" => ApplyDiscountPercentage(context),
                    "discountamount" => ApplyDiscountAmount(context),
                    "freeitem" => ApplyFreeItem(context),
                    "freeshipping" => ApplyFreeShipping(context),
                    "points" => ApplyPoints(context),
                    _ => throw new NotSupportedException($"Reward type {RewardType} is not supported")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying reward {RewardType}: {ex.Message}");
                return new AppliedReward
                {
                    RewardType = RewardType,
                    ConditionName = ConditionName,
                    Value = 0,
                    Description = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Calculates the value of this reward without applying it
        /// </summary>
        public decimal CalculateValue(PromotionContext context)
        {
            return RewardType.ToLower() switch
            {
                "discount" => CalculateDiscountValue(context),
                "discountpercentage" => CalculateDiscountPercentageValue(context),
                "discountamount" => CalculateDiscountAmountValue(context),
                "freeitem" => CalculateFreeItemValue(context),
                "freeshipping" => CalculateFreeShippingValue(context),
                "points" => CalculatePointsValue(context),
                _ => 0m
            };
        }

        private AppliedReward ApplyDiscount(PromotionContext context)
        {
            var discountValue = CalculateDiscountValue(context);

            return new AppliedReward
            {
                RewardType = "discount",
                ConditionName = ConditionName,
                Value = discountValue,
                Description = $"Discount applied: {discountValue:C}",
                Parameters = Parameters.ToDictionary(p => p, p => (object)GetParameterValue(p, context))
            };
        }

        private AppliedReward ApplyDiscountPercentage(PromotionContext context)
        {
            var percentage = GetConfigValue<decimal>(context, "discountPercent");
            var discountValue = context.Cart.TotalAmount * (percentage / 100m);

            return new AppliedReward
            {
                RewardType = "discountPercentage",
                ConditionName = ConditionName,
                Value = discountValue,
                Description = $"Percentage discount applied: {percentage}% ({discountValue:C})",
                Parameters = new Dictionary<string, object> { { "percentage", percentage } }
            };
        }

        private AppliedReward ApplyDiscountAmount(PromotionContext context)
        {
            var discountAmount = GetConfigValue<decimal>(context, "discountAmount");

            return new AppliedReward
            {
                RewardType = "discountAmount",
                ConditionName = ConditionName,
                Value = discountAmount,
                Description = $"Fixed amount discount applied: {discountAmount:C}",
                Parameters = new Dictionary<string, object> { { "amount", discountAmount } }
            };
        }

        private AppliedReward ApplyFreeItem(PromotionContext context)
        {
            var freeItemSku = GetConfigValue<string>(context, "freeItemSku");
            var freeItemValue = GetConfigValue<decimal>(context, "freeItemValue", 0m);

            return new AppliedReward
            {
                RewardType = "freeItem",
                ConditionName = ConditionName,
                Value = freeItemValue,
                Description = $"Free item added: {freeItemSku}",
                Parameters = new Dictionary<string, object>
                {
                    { "sku", freeItemSku },
                    { "value", freeItemValue }
                }
            };
        }

        private AppliedReward ApplyFreeShipping(PromotionContext context)
        {
            var shippingValue = GetConfigValue<decimal>(context, "shippingCost", 0m);

            return new AppliedReward
            {
                RewardType = "freeShipping",
                ConditionName = ConditionName,
                Value = shippingValue,
                Description = "Free shipping applied",
                Parameters = new Dictionary<string, object> { { "shippingCost", shippingValue } }
            };
        }

        private AppliedReward ApplyPoints(PromotionContext context)
        {
            var pointsMultiplier = GetConfigValue<decimal>(context, "pointsMultiplier", 1m);
            var pointsValue = context.Cart.TotalAmount * pointsMultiplier;

            return new AppliedReward
            {
                RewardType = "points",
                ConditionName = ConditionName,
                Value = pointsValue,
                Description = $"Points awarded: {pointsValue} points",
                Parameters = new Dictionary<string, object> { { "multiplier", pointsMultiplier } }
            };
        }

        private decimal CalculateDiscountValue(PromotionContext context)
        {
            // Default discount calculation - can be overridden
            var discountPercent = GetConfigValue<decimal>(context, "discountPercent", 0m);
            return context.Cart.TotalAmount * (discountPercent / 100m);
        }

        private decimal CalculateDiscountPercentageValue(PromotionContext context)
        {
            var percentage = GetConfigValue<decimal>(context, "discountPercent");
            return context.Cart.TotalAmount * (percentage / 100m);
        }

        private decimal CalculateDiscountAmountValue(PromotionContext context)
        {
            return GetConfigValue<decimal>(context, "discountAmount");
        }

        private decimal CalculateFreeItemValue(PromotionContext context)
        {
            return GetConfigValue<decimal>(context, "freeItemValue", 0m);
        }

        private decimal CalculateFreeShippingValue(PromotionContext context)
        {
            return GetConfigValue<decimal>(context, "shippingCost", 0m);
        }

        private decimal CalculatePointsValue(PromotionContext context)
        {
            var pointsMultiplier = GetConfigValue<decimal>(context, "pointsMultiplier", 1m);
            return context.Cart.TotalAmount * pointsMultiplier;
        }

        private T GetConfigValue<T>(PromotionContext context, string key, T defaultValue = default(T))
        {
            return context.Config.GetValue(key, defaultValue);
        }

        private object GetParameterValue(string parameter, PromotionContext context)
        {
            if (parameter.StartsWith("config."))
            {
                var configKey = parameter.Replace("config.", "");
                return context.Config.Values.GetValueOrDefault(configKey, "");
            }
            return parameter;
        }

        /// <summary>
        /// Validates if the reward is properly configured
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(RewardType)) return false;
            if (string.IsNullOrEmpty(ConditionName)) return false;

            // Reward-specific validation
            switch (RewardType.ToLower())
            {
                case "discount":
                case "discountpercentage":
                case "discountamount":
                case "freeitem":
                case "freeshipping":
                case "points":
                    return true;
                default:
                    return false; // Unknown reward type
            }
        }

        public override string ToString()
        {
            var paramStr = Parameters.Count > 0 ? $" {string.Join(" ", Parameters)}" : "";
            var exprStr = Expression != null ? $" {Expression}" : "";
            return $"condition {ConditionName} {RewardType}{paramStr}{exprStr}";
        }
    }

    /// <summary>
    /// Factory for creating promotion rewards
    /// </summary>
    public static class PromotionRewardFactory
    {
        public static PromotionReward CreateDiscountPercentageReward(string conditionName, string configKey)
        {
            return new PromotionReward
            {
                RewardType = "discountPercentage",
                ConditionName = conditionName,
                Parameters = new List<string> { $"config.{configKey}" }
            };
        }

        public static PromotionReward CreateDiscountAmountReward(string conditionName, string configKey)
        {
            return new PromotionReward
            {
                RewardType = "discountAmount",
                ConditionName = conditionName,
                Parameters = new List<string> { $"config.{configKey}" }
            };
        }

        public static PromotionReward CreateFreeItemReward(string conditionName, string skuConfigKey, string valueConfigKey)
        {
            return new PromotionReward
            {
                RewardType = "freeItem",
                ConditionName = conditionName,
                Parameters = new List<string> { $"config.{skuConfigKey}", $"config.{valueConfigKey}" }
            };
        }

        public static PromotionReward CreatePointsReward(string conditionName, string multiplierConfigKey)
        {
            return new PromotionReward
            {
                RewardType = "points",
                ConditionName = conditionName,
                Parameters = new List<string> { $"config.{multiplierConfigKey}" }
            };
        }
    }
}
