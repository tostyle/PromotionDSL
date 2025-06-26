# Promotion DSL Parser

A complete lexer, parser, and AST visitor implementation for the promotion domain-specific language (DSL) described in the requirements.

## Overview

This parser handles promotion rules with the following grammar structure:

```
<promotion_name> <function> <statements>
```

Where:
- **promotion_name**: Identifier (A, B, C, etc.)
- **function**: `any` or `all`
- **statements**: Function calls and expressions with logical operators

## Features

### 1. Lexical Analysis (PromotionLexer.cs)
- Tokenizes promotion DSL input
- Supports identifiers, numbers, strings
- Handles operators: `&&`, `||`, `=`, `>`, `<`, `>=`, `<=`, `!=`
- Recognizes keywords: `any`, `all`
- Supports property access: `object.property`
- Handles rule separators: `---`

### 2. Syntax Analysis (PromotionParser.cs)
- Recursive descent parser
- Builds Abstract Syntax Tree (AST)
- Supports operator precedence
- Handles parentheses for expression grouping
- Supports complex nested expressions

### 3. AST Structure (PromotionAst.cs)
- **Program**: Root node containing promotion rules
- **PromotionRule**: Individual promotion with name, function, and statements
- **Expressions**: Binary, comparison, property access, literals
- **FunctionCall**: Function invocation with arguments

### 4. Visitor Pattern (PromotionVisitor.cs)
- **IPromotionVisitor<T>**: Interface for AST traversal
- **PromotionVisitorBase<T>**: Base implementation
- **PromotionPrettyPrintVisitor**: Pretty prints AST structure
- **PromotionEvaluationVisitor**: Evaluates expressions (example)

## Usage Example

```csharp
// 1. Create lexer and tokenize input
var lexer = new PromotionLexer(dslInput);
var tokens = lexer.Tokenize();

// 2. Parse tokens into AST
var parser = new PromotionDslParser(tokens);
var program = parser.Parse();

// 3. Traverse AST using visitor
var visitor = new PromotionPrettyPrintVisitor();
var result = program.Accept(visitor);
```

## Supported Grammar Examples

### Basic Function Calls
```
A any minimumSpending config.minAmount
C all totalAmount config.threshold
```

### Property Comparisons
```
B any item.sku = config.sku
B any item.quantity > config.minQuantity
```

### Complex Expressions
```
A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
B any item.sku = config.sku && item.quantity > config.minQuantity
```

### Parentheses Support
```
C any (item.price > 100 && item.category = "premium") || item.vip = true
```

### Multiple Rules
Rules are separated by `---`:
```
A any minimumSpending config.minAmount
---
B any item.sku = config.sku
---
C all totalAmount config.threshold
```

## Operator Precedence

1. **Parentheses**: `( )`
2. **Comparison**: `=`, `>`, `<`, `>=`, `<=`, `!=`
3. **Logical AND**: `&&`
4. **Logical OR**: `||`

## Error Handling

The parser provides meaningful error messages for:
- Invalid tokens
- Syntax errors
- Malformed expressions
- Missing operators or operands

## Testing

The implementation includes comprehensive tests in `PromotionParserTester.cs`:
- Basic promotion parsing
- Complex expression handling
- Parentheses support
- Multiple rules
- Error conditions

## Running the Parser

```bash
cd PromotionParser
dotnet build
dotnet run
```

This will run the demo with test cases and show:
1. Tokenization results
2. AST parsing
3. Pretty-printed AST structure
4. Individual rule analysis

## Extending the Parser

To add new functionality:

1. **New Tokens**: Add to `TokenType` enum and update lexer
2. **New AST Nodes**: Create classes inheriting from `AstNode`
3. **New Visitors**: Implement `IPromotionVisitor<T>` interface
4. **New Grammar Rules**: Update parser methods

The parser is designed to be easily extensible while maintaining clean separation of concerns between lexical analysis, parsing, and AST traversal.
