# parse grammar
```
antlr4-parse Promotion.g4 program examples/simple.promo
```

# generate c#
```
antlr4 -Dlanguage=CSharp -o ./Generated Promotion.g4 -package PromotionDSL
```