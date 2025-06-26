using Antlr4.Runtime;
using PromotionDSL;
using PromotionEngine.Domain;
using System.IO;

namespace UnitTests;

/// <summary>
/// Debug tests to understand parser behavior
/// </summary>
public class ParserDebugTest
{
    [Fact]
    public void DebugSimplePromotion()
    {
        // Let's test with a very simple, correctly formatted DSL
        var dslContent = @"promotion: ""Simple Test""
conditions:
- A any minimumSpending config.minAmount
rewards:
- condition A discount percentage config.discountPercent
";

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

        Assert.Empty(errorListener.Errors);

        // Print the parse tree structure
        Console.WriteLine($"Parse tree: {parseTree.GetType().Name}");
        Console.WriteLine($"Parse tree text: {parseTree.GetText()}");
        Console.WriteLine($"Children count: {parseTree.ChildCount}");

        for (int i = 0; i < parseTree.ChildCount; i++)
        {
            var child = parseTree.GetChild(i);
            Console.WriteLine($"Child {i}: {child.GetType().Name} - {child.GetText()}");
        }

        // Try to map
        var mapper = new PromotionMapper();

        // Let's visit the program first
        var result = mapper.Visit(parseTree);
        Console.WriteLine($"Mapper result: {result?.GetType().Name ?? "null"}");

        if (result == null)
        {
            // Try visiting the promotion definition directly
            if (parseTree.ChildCount > 0 && parseTree.GetChild(0) is PromotionParser.PromotionDefContext promotionDef)
            {
                var promotionResult = mapper.Visit(promotionDef);
                Console.WriteLine($"Direct promotion result: {promotionResult?.GetType().Name ?? "null"}");
            }
        }

        var promotion = mapper.MapPromotion(parseTree);

        Assert.NotNull(promotion);
        Assert.Equal("Simple Test", promotion.Name);
    }

    [Fact]
    public void DebugTokenization()
    {
        var dslContent = @"promotion: ""Simple Test""
conditions:
- A any minimumSpending config.minAmount
rewards:
- condition A discount percentage config.discountPercent
";

        var inputStream = new AntlrInputStream(dslContent);
        var lexer = new PromotionLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        var tokens = tokenStream.GetTokens();
        foreach (var token in tokens)
        {
            Console.WriteLine($"Token: {token.Type} = '{token.Text}' (Line: {token.Line}, Col: {token.Column})");
        }

        // This should help us understand what tokens are being generated
        Assert.True(tokens.Count > 0);
    }
}

public class TestErrorListener : BaseErrorListener
{
    public List<string> Errors { get; } = new List<string>();

    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Errors.Add($"Line {line}:{charPositionInLine} {msg}");
    }
}
