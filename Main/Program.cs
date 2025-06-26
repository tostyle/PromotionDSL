using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PromotionDSL;
using PromotionEngine.Domain;

namespace promotion_dsl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🎯 Promotion DSL Parser");
            Console.WriteLine("=======================");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: promotion-dsl <promotion-file.promo>");
                Console.WriteLine("\nExample:");
                Console.WriteLine("  promotion-dsl examples/simple.promo");
                Console.WriteLine("\nTo run unit tests, use:");
                Console.WriteLine("  dotnet test");
                return;
            }

            try
            {
                string filePath = args[0];

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ File not found: {filePath}");
                    return;
                }

                Console.WriteLine($"📄 Parsing promotion file: {filePath}");

                // Parse the DSL file
                var promotion = ParsePromotionFile(filePath);

                // Display parsed promotion information
                DisplayPromotionInfo(promotion);

                Console.WriteLine("\n✅ Promotion parsed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error parsing promotion: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static BasePromotion ParsePromotionFile(string filePath)
        {
            string content = File.ReadAllText(filePath);

            // Parse with ANTLR
            var inputStream = new AntlrInputStream(content);
            var lexer = new PromotionLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PromotionParser(tokenStream);

            // Parse starting from the 'program' rule
            var tree = parser.program();

            // Map to domain objects
            var mapper = new PromotionMapper();
            return mapper.MapPromotion(tree);
        }

        static void DisplayPromotionInfo(BasePromotion promotion)
        {
            Console.WriteLine($"\nPromotion Details:");
            Console.WriteLine($"  Name: {promotion.Name}");
            Console.WriteLine($"  Conditions: {promotion.Conditions.Count}");

            foreach (var condition in promotion.Conditions)
            {
                Console.WriteLine($"    - {condition.Name}: {condition.FunctionName}");
                if (condition.Parameters?.Count > 0)
                {
                    Console.WriteLine($"      Parameters: {string.Join(", ", condition.Parameters)}");
                }
                if (condition.Expression != null)
                {
                    Console.WriteLine($"      Expression: {condition.Expression}");
                }
            }

            Console.WriteLine($"  Rewards: {promotion.Rewards.Count}");
            foreach (var reward in promotion.Rewards)
            {
                Console.WriteLine($"    - {reward.ConditionName}: {reward.RewardType}");
                if (reward.Parameters?.Count > 0)
                {
                    Console.WriteLine($"      Parameters: {string.Join(", ", reward.Parameters)}");
                }
            }

            // Test with sample data to show functionality
            Console.WriteLine($"\nTesting with sample cart data:");
            var context = CreateSampleContext();
            var result = promotion.Apply(context);

            Console.WriteLine($"  Applicable: {result.IsApplicable}");
            if (result.IsApplicable)
            {
                Console.WriteLine($"  Triggered Conditions: {string.Join(", ", result.TriggeredConditions)}");
                Console.WriteLine($"  Applied Rewards: {result.AppliedRewards.Count}");

                foreach (var appliedReward in result.AppliedRewards)
                {
                    Console.WriteLine($"    - {appliedReward.Description} (Value: {appliedReward.Value:C})");
                }
            }

            if (result.Errors.Count > 0)
            {
                Console.WriteLine($"  Errors: {string.Join(", ", result.Errors)}");
            }
        }

        static PromotionContext CreateSampleContext()
        {
            return new PromotionContext
            {
                Cart = new Cart
                {
                    Items = new List<CartItem>
                    {
                        new CartItem { Sku = "ITEM001", Price = 29.99m, Quantity = 2, Name = "Test Product 1" },
                        new CartItem { Sku = "ITEM002", Price = 49.99m, Quantity = 1, Name = "Test Product 2" }
                    }
                },
                Config = new PromotionConfig
                {
                    Values = new Dictionary<string, object>
                    {
                        { "minAmount", 50.00m },
                        { "minQuantity", 2 },
                        { "discountPercent", 10.0m },
                        { "discountAmount", 5.00m },
                        { "sku", "ITEM001" },
                        { "targetSku", "ITEM001" },
                        { "threshold", 100.00m },
                        { "premiumThreshold", 200.00m },
                        { "pointsMultiplier", 2.0m },
                        { "freeProduct", "FREE001" },
                        { "bonusProduct", "BONUS001" },
                        { "standardDiscount", 10.0m },
                        { "premiumDiscount", 25.00m },
                        { "minTargetQuantity", 1 }
                    }
                }
            };
        }
    }
}