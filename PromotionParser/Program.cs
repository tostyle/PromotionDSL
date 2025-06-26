using PromotionParser;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Promotion DSL Parser Demo");
        Console.WriteLine("========================");

        // Run tests first
        PromotionParserTester.RunTests();

        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("MAIN DEMO");
        Console.WriteLine(new string('=', 50));

        // Test input based on the examples from README
        string input = @"A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
---
B any item.sku = config.sku
---
A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
---
B any item.sku = config.sku && item.quantity > config.minQuantity
---
C all totalAmount config.threshold";

        try
        {
            Console.WriteLine("Input:");
            Console.WriteLine(input);
            Console.WriteLine();

            // Step 1: Lexical Analysis
            Console.WriteLine("1. Lexical Analysis (Tokenization):");
            var lexer = new PromotionLexer(input);
            var tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                if (token.Type != TokenType.EOF)
                    Console.WriteLine($"  {token}");
            }
            Console.WriteLine();

            // Step 2: Parsing
            Console.WriteLine("2. Parsing (Building AST):");
            var parser = new PromotionDslParser(tokens);
            var program = parser.Parse();
            Console.WriteLine($"  Parsed {program.Rules.Count} promotion rules");
            Console.WriteLine();

            // Step 3: AST Traversal using Visitor
            Console.WriteLine("3. AST Traversal (Using Visitor Pattern):");
            var prettyPrintVisitor = new PromotionPrettyPrintVisitor();
            var output = program.Accept(prettyPrintVisitor);
            Console.WriteLine(output);

            // Step 4: Show individual rule analysis
            Console.WriteLine("4. Individual Rule Analysis:");
            for (int i = 0; i < program.Rules.Count; i++)
            {
                var rule = program.Rules[i];
                Console.WriteLine($"  Rule {i + 1}:");
                Console.WriteLine($"    Name: {rule.Name}");
                Console.WriteLine($"    Function: {rule.Function}");
                Console.WriteLine($"    Statements: {rule.Statements.Count}");

                foreach (var statement in rule.Statements)
                {
                    Console.WriteLine($"      - {statement.Accept(prettyPrintVisitor).Trim()}");
                }
                Console.WriteLine();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
