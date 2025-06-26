using System;
using System.Collections.Generic;

namespace PromotionParser
{
    public class PromotionParserTester
    {
        public static void RunTests()
        {
            Console.WriteLine("Running Promotion Parser Tests");
            Console.WriteLine("==============================");

            TestBasicPromotion();
            TestComplexExpressions();
            TestParentheses();
            TestMultipleRules();
            TestErrorHandling();
        }

        private static void TestBasicPromotion()
        {
            Console.WriteLine("\n1. Testing Basic Promotion:");
            string input = "A any minimumSpending config.minAmount";

            try
            {
                var result = ParseAndPrint(input);
                Console.WriteLine("✓ Basic promotion parsed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Basic promotion failed: {ex.Message}");
            }
        }

        private static void TestComplexExpressions()
        {
            Console.WriteLine("\n2. Testing Complex Expressions:");
            string input = "B any item.sku = config.sku && item.quantity > config.minQuantity";

            try
            {
                var result = ParseAndPrint(input);
                Console.WriteLine("✓ Complex expressions parsed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Complex expressions failed: {ex.Message}");
            }
        }

        private static void TestParentheses()
        {
            Console.WriteLine("\n3. Testing Parentheses:");
            string input = "C any (item.price > 100 && item.category = \"premium\") || item.vip = true";

            try
            {
                var result = ParseAndPrint(input);
                Console.WriteLine("✓ Parentheses parsed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Parentheses failed: {ex.Message}");
            }
        }

        private static void TestMultipleRules()
        {
            Console.WriteLine("\n4. Testing Multiple Rules:");
            string input = @"A any minimumSpending config.minAmount
---
B all item.sku = config.sku && item.quantity >= 2
---
C any totalAmount config.threshold";

            try
            {
                var result = ParseAndPrint(input);
                Console.WriteLine("✓ Multiple rules parsed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Multiple rules failed: {ex.Message}");
            }
        }

        private static void TestErrorHandling()
        {
            Console.WriteLine("\n5. Testing Error Handling:");

            // Test invalid tokens
            string[] invalidInputs = {
                "A invalid_function test",  // invalid function (should be 'any' or 'all')
                "123 any test",             // promotion name cannot start with number
                "A any test config.",       // incomplete property access
                "A any test config.prop)"   // unmatched parenthesis
            };

            foreach (var input in invalidInputs)
            {
                try
                {
                    ParseAndPrint(input);
                    Console.WriteLine($"✗ Should have failed for: {input}");
                }
                catch (Exception)
                {
                    Console.WriteLine($"✓ Correctly rejected invalid input: {input}");
                }
            }
        }

        private static string ParseAndPrint(string input)
        {
            var lexer = new PromotionLexer(input);
            var tokens = lexer.Tokenize();
            var parser = new PromotionDslParser(tokens);
            var program = parser.Parse();
            var visitor = new PromotionPrettyPrintVisitor();
            return program.Accept(visitor);
        }
    }
}
