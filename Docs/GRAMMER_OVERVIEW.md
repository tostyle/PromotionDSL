# Promotion DSL Grammar Summary

## Top-Level Structure
- The DSL describes a promotion with conditions and rewards, using reserved keywords and a YAML-like structure.

## Grammar Overview

- **promotionDef**: The root rule, representing a promotion block.
  - Starts with `promotion: "<name>"` (quoted string for the promotion name)
  - Followed by a `conditions:` section (list of conditions)
  - Followed by a `rewards:` section (list of rewards)

### Example Structure
```yaml
promotion: "Simple Test"
conditions:
- A any minimumSpending config.minAmount
rewards:
- condition A discount percentage config.discountPercent
```

## Grammar Rules (from Promotion.g4)

- **program**: The entry point, expects a `promotionDef` and EOF.
- **promotionDef**: 
  - `promotion` COLON STRING NEWLINE
  - `conditions` COLON NEWLINE conditionList
  - `rewards` COLON NEWLINE rewardList
- **conditionList**: One or more `condition` entries.
- **condition**: 
  - DASH IDENTIFIER functionCall (expression)? NEWLINE
    - Example: `- A any minimumSpending config.minAmount`
- **rewardList**: One or more `reward` entries.
- **reward**: 
  - DASH CONDITION IDENTIFIER functionCall (expression)? NEWLINE
    - Example: `- condition A discount percentage config.discountPercent`
- **functionCall**: 
  - IDENTIFIER (propertyAccess)?
    - Example: `any minimumSpending config.minAmount`
- **propertyAccess**: 
  - IDENTIFIER ('.' IDENTIFIER)*
    - Example: `config.minAmount`
- **expression**: Logical and comparison expressions (optional, for complex conditions).

## Tokens (Lexer)
- **PROMOTION**: 'promotion'
- **CONDITIONS**: 'conditions'
- **REWARDS**: 'rewards'
- **CONDITION**: 'condition'
- **IDENTIFIER**: Alphanumeric identifier
- **NUMBER**: Numeric literal
- **STRING**: Quoted string
- **COLON**: ':'
- **DASH**: '-'
- **NEWLINE**: Line break
- **WS**: Whitespace (skipped)
- **COMMENT**: `#` to end of line (skipped)

## Summary
- The grammar enforces a block structure for promotions, with clear sections for conditions and rewards.
- Each condition and reward is a line starting with `-` and follows a specific pattern.
- Function calls and property access are supported, as well as logical/comparison expressions for advanced use cases.
- Reserved keywords are handled as distinct tokens to avoid ambiguity with identifiers.