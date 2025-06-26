# Overview

I will give u example grammar u need to help me to create lexer and parser of thiese grammar for me



token of this grammar overall like this


- start with promotion name
- then function and then u need to crate recursive descent to support expression grammar
- statement can be expression function call
- expression can be recursive eg. A && B && C || D
- expression need to support parentheses
- variable is an object pattern `<object name>.<property>`

## Examples
```
A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
---
B any item.sku = config.sku
---
A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
---
B any item.sku = config.sku && item.quantity > config.minQuantity
---
C all totalAmount config.threshold
```