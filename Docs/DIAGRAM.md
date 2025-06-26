# Promotion DSL System Diagrams

## 1. Promotion DSL Authoring and Testing Flow

```mermaid
flowchart LR
    A([user prompt promotion detail they want]) --> B[Promotion DSL Agent - generate promotion DSL]
    B --> C[Promotion Test Agent write test case of promotion]
    C --> D[save to code storage - can be database or file]
    B --> E[Tools: validate DSL syntax]
    C --> F[Tools: write unittest to test promotion]

```

## 2. Promotion Engine Usage Flow

```mermaid
flowchart LR
    A([user add item to cart]) --> B[check item that related to promotion or not]
    B -- if item has promotion --> C[get promotion engine from database - retrieve DSL code]
    C --> D[generate promotion code from DSL]
    D --> E[validate promotion]
    E --> F[check user reward]
```

---

## Summary

These diagrams illustrate the workflow for both authoring/testing promotions using a DSL and the runtime flow for applying promotions in a shopping cart system:

- **Promotion DSL Authoring and Testing Flow:**
    - Users describe the promotion they want.
    - The Promotion DSL Agent generates the DSL code.
    - The Promotion Test Agent writes test cases for the promotion.
    - Tools are used to validate the DSL syntax and to write unittests.
    - The final DSL and tests are saved to code storage (database or file).

- **Promotion Engine Usage Flow:**
    - When a user adds an item to the cart, the system checks if the item is related to a promotion.
    - If so, it retrieves the promotion DSL code from storage.
    - The promotion code is generated and validated.
    - The system then checks and applies the appropriate user reward.
