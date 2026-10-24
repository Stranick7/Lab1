grammar Calculator;
compileUnit : expr EOF ;
expr : '(' expr ')'					# Parens
	| 'inc' '(' expr ')'			# IncFunc
	| 'dec' '(' expr ')'			# DecFunc
	| 'max' '(' expr ',' expr ')'	# MaxFunc
	| 'min' '(' expr ',' expr ')'	# MinFunc
	| NUMBER						# Number
	| CELLREF						# CellRef
	| <assoc=right> expr op='^' expr	# Power
	| expr op=('*' | '/') expr			# MulDiv
	| expr op=('+' | '-') expr			# AddSub
	;

	NUMBER : [0-9]+ ;
	CELLREF : [A-Z]+ [0-9]+ ;
	WS : [ \t\r\n]+ -> skip ;
