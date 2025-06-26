grammar Promotion;

@namespace { PromotionDsl }

program: promotionDef EOF;

promotionDef:
	PROMOTION COLON STRING NEWLINE CONDITIONS COLON NEWLINE conditionList REWARDS COLON NEWLINE
		rewardList;

conditionList: condition+;
condition: DASH IDENTIFIER functionCall (expression)? NEWLINE;

rewardList: reward+;
reward:
	DASH CONDITION IDENTIFIER functionCall (expression)? NEWLINE;

functionCall: IDENTIFIER (propertyAccess)?;
propertyAccess: IDENTIFIER ('.' IDENTIFIER)*;

expression: logicalExpr;
logicalExpr: comparisonExpr (('&&' | '||') comparisonExpr)*;
comparisonExpr:
	operand (('=' | '>' | '<' | '>=' | '<=' | '!=') operand)?;
operand: propertyAccess | NUMBER | STRING;

// Lexer rules
PROMOTION: 'promotion';
CONDITIONS: 'conditions';
REWARDS: 'rewards';
CONDITION: 'condition';

IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;
NUMBER: [0-9]+ ('.' [0-9]+)?;
STRING: '"' (~["\r\n])* '"';
COLON: ':';
DASH: '-';
NEWLINE: [\r\n]+;
// WS: [ \t]+ -> skip;
WS: [ \t\r\n]+ -> skip;
COMMENT: '#' ~[\r\n]* -> skip;