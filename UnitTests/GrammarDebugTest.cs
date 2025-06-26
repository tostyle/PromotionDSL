using Antlr4.Runtime;
using PromotionDSL;
using PromotionEngine.Domain;
using System.IO;

namespace UnitTests;

public class GrammarDebugTest
{
    [Fact]
    public void DebugGrammarFormat()
    {
        // Let's test what the grammar actually expects
        var dslContent = @"promotion: ""Grammar Test""
conditions:
- A minimumSpending config.minAmount
rewards:
- condition A discount config.discountPercent
";

        Console.WriteLine("Testing grammar-conformant format...");

        // Create ANTLR input stream
        var inputStream = new AntlrInputStream(dslContent);

        // Create lexer
        var lexer = new PromotionLexer(inputStream);

        // Create token stream
        var tokenStream = new CommonTokenStream(lexer);

        // Create parser with error listener
        var parser = new PromotionParser(tokenStream);

        // Add error listener to see what's wrong
        var errorListener = new TestErrorListener();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        // Parse the program (root rule)
        var parseTree = parser.program();

        // Check for errors
        if (errorListener.Errors.Any())
        {
            foreach (var error in errorListener.Errors)
            {
                Console.WriteLine($"Parse Error: {error}");
            }
        }
        else
        {
            Console.WriteLine("No parse errors!");

            // Try to map
            var mapper = new PromotionMapper();
            var promotion = mapper.MapPromotion(parseTree);

            Console.WriteLine($"Promotion: {promotion.Name}");
            Console.WriteLine($"Conditions: {promotion.Conditions.Count}");
            Console.WriteLine($"Rewards: {promotion.Rewards.Count}");

            if (promotion.Conditions.Any())
            {
                var condition = promotion.Conditions[0];
                Console.WriteLine($"First condition: {condition.Name} - {condition.FunctionName} - [{string.Join(", ", condition.Parameters)}]");
            }
        }

        Assert.True(true); // This is just for debugging
    }
}
