# Promotion DSL Project

## Overview

This project implements a Domain-Specific Language (DSL) for defining complex promotion rules and rewards for e-commerce or retail systems. The DSL allows you to describe promotions in a human-readable format, parse them into an abstract syntax tree (AST) using ANTLR, and map them to C# domain models for further processing and validation.

### Key Features
- **Custom Promotion DSL**: Write promotion rules in a YAML-like syntax.
- **ANTLR Grammar**: The grammar is defined in `Grammar/Promotion.g4` and compiled to C# parser/visitor classes.
- **C# Domain Mapping**: DSL rules are mapped to C# classes under `PromotionDSL/Domain/`.
- **Unit Testing**: Comprehensive tests ensure parsing and mapping correctness.
- **Extensible**: Easily add new conditions, rewards, or grammar rules.

---

## Project Structure
- `Grammar/Promotion.g4` — ANTLR grammar for the DSL.
- `PromotionDSL/` — Main C# library for parsing and mapping.
- `UnitTests/` — xUnit test project for validating the DSL and mapping.
- `Examples/` — Example promotion DSL files.
- `Docs/` — Documentation and grammar overview.
- `Makefile` — Utilities for ANTLR code generation and grammar testing.

---

## DSL Workflow

1. **Write Promotion Rules**: Author promotion rules in the DSL (see `Examples/`).
2. **Parse with ANTLR**: Use the generated parser to convert DSL text into a parse tree.
3. **Map to Domain Models**: The `PromotionMapper` class visits the parse tree and creates C# objects (`BasePromotion`, `PromotionCondition`, `PromotionReward`, etc.).
4. **Validate & Test**: Use the domain models in your application logic or run unit tests to ensure correctness.

---

## Example DSL

```yaml
promotion: "Simple Test"
conditions:
- A minimumSpending config.minAmount
rewards:
- condition A discount config.discountPercent
```

---

## Makefile Workflow

The `Makefile` provides shortcuts for grammar development and testing:

- **Generate ANTLR Parser Code**
  ```sh
  make antlr
  ```
  - Runs ANTLR to generate C# parser and visitor code from `Grammar/Promotion.g4`.
  - Output is placed in `PromotionDSL/Generated/`.

- **Parse a DSL File (for debugging grammar)**
  ```sh
  make parse FILE=basic.promo
  ```
  - Uses `antlr4-parse` to parse a file in `Examples/` and print the parse tree.
  - Useful for quickly checking grammar changes.

---

## .NET Solution Workflow

- **Solution File**: `promotion-dsl.sln` manages the main library and test projects.
- **Key Commands**:
  - `dotnet build` — Build all projects.
  - `dotnet test` — Run all unit tests in `UnitTests/`.
  - `dotnet sln add <project>` — Add a new project to the solution.
  - `dotnet sln list` — List all projects in the solution.

**Typical Development Flow:**
1. Edit grammar or C# code.
2. Run `make antlr` if grammar changes.
3. Build and test:
   ```sh
   dotnet build
   dotnet test
   ```
4. Add new projects or files as needed using `dotnet sln` commands.

---

## Contribution & Extending

- To add new grammar rules, edit `Grammar/Promotion.g4` and run `make antlr`.
- To add new domain logic, extend classes in `PromotionDSL/Domain/`.
- To add or update tests, edit files in `UnitTests/`.

---

## References

- See `Docs/GRAMMER_OVERVIEW.md` for a summary of the DSL grammar.
- See `Examples/` for sample promotion files.

# Promotion condition example
1. 
conditions if item match with sku
rewards decreate price as percentage or amount
2. 
conditions if item match with sku
rewards decrease lowest item price 50 percent of price

3. 
conditions if item match with sku
rewards decrease item price by items 20%, 10%, 5% consecutively order item by lowest price

# Draft 1

- separate conditions and rewards section
- yaml style
```yaml
config:
    minQuantity: 1
    minAmount: 500
    sku: "product1"
promotion:
    name: "promo 7/7 all day"
    conditions:
        - A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
        - B any item.sku = config.sku && item.quantity > config.minQuantity
    # condition_expression: A || B
    rewards:
        - condition A select discountAmount config.discountAmount
        - condition B select discountPercent config.discountPercentage
```

# Draft 2
- yaml style with array of object pattern
```yaml
promotion
    name: "promo 7/7 all day"
    conditions:
        - name: A
          expression: any item.sku = config.sku && item.quantity > config.minQuantity
    rewards:
        - condition: A 
          expression: select discountAmount config.discountAmount
```

# Draft 3
- lua +  kotlin style

```lua
promotion {
    name "promo 7/7 all day"
    conditions {
        A minimumSpending config.minAmount && minimumQuantity config.minQuantity,
        B any item.sku = config.sku && item.quantity > config.minQuantity
    }
    rewards {
       condition A select discountAmount config.discountAmount,
       condition B select discountPercent config.discountPercentage
    }
}
```

# Draft 4
- sql style
```sql
PROMOTION "7/7 All Day"
WHERE
    any(minimumSpending, config.minAmount) as A 
    AND minimumQuantity >= config.minQuantity as B
    OR (item.sku = config.sku AND item.quantity > config.minQuantity) as C
REWARD
    A discount_percentage = config.discountPercent
    B discount_amount = config.discountAmount
END
```