using System;
using System.Collections.Generic;
using System.Linq;

namespace PromotionEngine.Domain
{
    /// <summary>
    /// Shopping cart containing product items
    /// </summary>
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount => Items.Sum(x => x.Price * x.Quantity);
        public int TotalQuantity => Items.Sum(x => x.Quantity);
        public int UniqueItemsCount => Items.Count;

        public CartItem? GetItemBySku(string sku)
        {
            return Items.FirstOrDefault(x => x.Sku == sku);
        }

        public List<CartItem> GetItemsBySku(string sku)
        {
            return Items.Where(x => x.Sku == sku).ToList();
        }
    }

    /// <summary>
    /// Individual item in the cart
    /// </summary>
    public class CartItem
    {
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Promotion configuration containing dynamic values
    /// </summary>
    public class PromotionConfig
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (Values.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public bool HasValue(string key)
        {
            return Values.ContainsKey(key);
        }
    }

    /// <summary>
    /// Context for promotion evaluation
    /// </summary>
    public class PromotionContext
    {
        public Cart Cart { get; set; } = new Cart();
        public PromotionConfig Config { get; set; } = new PromotionConfig();
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Result of promotion evaluation and application
    /// </summary>
    public class PromotionResult
    {
        public string PromotionName { get; set; } = string.Empty;
        public bool IsApplicable { get; set; }
        public List<string> TriggeredConditions { get; set; } = new List<string>();
        public List<AppliedReward> AppliedRewards { get; set; } = new List<AppliedReward>();
        public List<string> Errors { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Details of an applied reward
    /// </summary>
    public class AppliedReward
    {
        public string RewardType { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
