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