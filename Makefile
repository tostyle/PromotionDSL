# Makefile for Promotion DSL project

.PHONY: antlr generate parse

# 1. Generate C# ANTLR parser and visitor code
antlr:
	antlr4 -Dlanguage=CSharp -o ./PromotionDSL/Generated ./Grammar/Promotion.g4 -package PromotionDSL -visitor

# 2. Parse a DSL file using antlr4-parse (pass the filename as FILE=...)
parse:
	@if [ -z "$(FILE)" ]; then \
		echo "Usage: make parse FILE=basic.promo"; \
		exit 1; \
	fi; \
	antlr4-parse Grammar/Promotion.g4 program Examples/$(FILE)

# Usage:
#   make antlr         # Generate parser code
#   make parse FILE=sample.promo  # Parse Examples/sample.promo
