using System;
using System.Collections.Generic;
using System.Linq;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Base promotion class containing conditions and rewards with business logic
    /// </summary>
    public class BasePromotion
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PromotionCondition> Conditions { get; set; } = new List<PromotionCondition>();
        public List<PromotionReward> Rewards { get; set; } = new List<PromotionReward>();
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Validates if the promotion can be applied to the given context
        /// </summary>
        public PromotionValidationResult Validate(PromotionContext context)
        {
            var result = new PromotionValidationResult { PromotionName = Name };

            try
            {
                // Check if promotion is active
                if (!IsActive)
                {
                    result.Errors.Add("Promotion is not active");
                    return result;
                }

                // Check date validity
                var now = DateTime.Now;
                if (StartDate.HasValue && now < StartDate.Value)
                {
                    result.Errors.Add($"Promotion has not started yet (starts: {StartDate.Value:yyyy-MM-dd})");
                    return result;
                }

                if (EndDate.HasValue && now > EndDate.Value)
                {
                    result.Errors.Add($"Promotion has expired (ended: {EndDate.Value:yyyy-MM-dd})");
                    return result;
                }

                // Validate context
                if (context.Cart == null)
                {
                    result.Errors.Add("Cart is required");
                    return result;
                }

                if (context.Config == null)
                {
                    result.Errors.Add("Configuration is required");
                    return result;
                }

                // Check if cart is empty
                if (!context.Cart.Items.Any())
                {
                    result.Errors.Add("Cart is empty");
                    return result;
                }

                // Validate conditions
                foreach (var condition in Conditions)
                {
                    if (!condition.IsValid())
                    {
                        result.Errors.Add($"Invalid condition: {condition.Name}");
                    }
                }

                // Validate rewards
                foreach (var reward in Rewards)
                {
                    if (!reward.IsValid())
                    {
                        result.Errors.Add($"Invalid reward: {reward.RewardType} for condition {reward.ConditionName}");
                    }
                }

                result.IsValid = !result.Errors.Any();
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Validation error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Checks if the promotion is eligible for the given context
        /// </summary>
        public bool IsEligible(PromotionContext context)
        {
            var validation = Validate(context);
            if (!validation.IsValid)
            {
                return false;
            }

            // Check if at least one condition is met
            return Conditions.Any(condition => condition.Evaluate(context));
        }

        /// <summary>
        /// Applies the promotion to the given context and returns the result
        /// </summary>
        public PromotionResult Apply(PromotionContext context)
        {
            var result = new PromotionResult { PromotionName = Name };

            try
            {
                // Validate first
                var validation = Validate(context);
                if (!validation.IsValid)
                {
                    result.Errors.AddRange(validation.Errors);
                    return result;
                }

                // Evaluate conditions and collect triggered ones
                var triggeredConditions = new List<PromotionCondition>();
                foreach (var condition in Conditions)
                {
                    if (condition.Evaluate(context))
                    {
                        triggeredConditions.Add(condition);
                        result.TriggeredConditions.Add(condition.Name);
                    }
                }

                // If no conditions are triggered, promotion is not applicable
                if (!triggeredConditions.Any())
                {
                    result.IsApplicable = false;
                    return result;
                }

                result.IsApplicable = true;

                // Apply rewards for triggered conditions
                foreach (var triggeredCondition in triggeredConditions)
                {
                    var applicableRewards = Rewards.Where(r => r.ConditionName == triggeredCondition.Name);

                    foreach (var reward in applicableRewards)
                    {
                        try
                        {
                            var appliedReward = reward.Apply(context);
                            result.AppliedRewards.Add(appliedReward);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error applying reward {reward.RewardType} for condition {reward.ConditionName}: {ex.Message}");
                        }
                    }
                }

                // Add metadata
                result.Metadata["totalSavings"] = result.AppliedRewards.Sum(r => r.Value);
                result.Metadata["triggeredConditionsCount"] = result.TriggeredConditions.Count;
                result.Metadata["appliedRewardsCount"] = result.AppliedRewards.Count;

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error applying promotion: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Gets all required data for this promotion to function
        /// </summary>
        public List<string> GetRequiredData()
        {
            var requiredData = new List<string>();

            foreach (var condition in Conditions)
            {
                requiredData.AddRange(condition.GetRequiredData());
            }

            // Remove duplicates
            return requiredData.Distinct().ToList();
        }

        /// <summary>
        /// Calculates the potential value of this promotion without applying it
        /// </summary>
        public decimal CalculatePotentialValue(PromotionContext context)
        {
            if (!IsEligible(context))
            {
                return 0m;
            }

            var totalValue = 0m;

            foreach (var condition in Conditions)
            {
                if (condition.Evaluate(context))
                {
                    var applicableRewards = Rewards.Where(r => r.ConditionName == condition.Name);
                    totalValue += applicableRewards.Sum(reward => reward.CalculateValue(context));
                }
            }

            return totalValue;
        }

        /// <summary>
        /// Adds a condition to the promotion
        /// </summary>
        public void AddCondition(PromotionCondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (!condition.IsValid()) throw new ArgumentException("Invalid condition", nameof(condition));

            Conditions.Add(condition);
        }

        /// <summary>
        /// Adds a reward to the promotion
        /// </summary>
        public void AddReward(PromotionReward reward)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (!reward.IsValid()) throw new ArgumentException("Invalid reward", nameof(reward));

            Rewards.Add(reward);
        }

        /// <summary>
        /// Gets a condition by name
        /// </summary>
        public PromotionCondition? GetCondition(string name)
        {
            return Conditions.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets rewards for a specific condition
        /// </summary>
        public List<PromotionReward> GetRewardsForCondition(string conditionName)
        {
            return Rewards.Where(r => r.ConditionName.Equals(conditionName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public override string ToString()
        {
            return $"Promotion: {Name} (Conditions: {Conditions.Count}, Rewards: {Rewards.Count}, Active: {IsActive})";
        }
    }

    /// <summary>
    /// Result of promotion validation
    /// </summary>
    public class PromotionValidationResult
    {
        public string PromotionName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
