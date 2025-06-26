I declare dsl language like this want u to help me design grammar in antlr
I will explain overall of this

it like yaml files but with some reserve keyword

1. promotion as top level as prmotion name
conditions as promo condition array
rewards as reward array of promotion

2. condition will format like this
<condition-name> at start of line and then  <function> <function-argument>  possible to has <expression> and can recursively descending expression start from  <function>

3. reward section
can start with condition with matching name from <condition-name> from condition section and then <function> and like normal functiob with some expression

--- 
I never write grammar before if u need any more information plz tell me what u want just generate start up grammar first 


---
Example
```yaml
promotion: "promo 7/7 all day"
conditions:
    - A any minimumSpending config.minAmount && minimumQuantity config.minQuantity
    - B any item.sku = config.sku && item.quantity > config.minQuantity
rewards:
    - condition A select discountAmount config.discountAmount
    - condition B select discountPercent config.discountPercentage
```