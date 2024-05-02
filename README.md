# Graphite

## Grammar
```
program 		= {declaration} EOF .
declaration		= classDeclaration
			| variableDeclaration 
			| functionDeclaration 
			| statement .
classDeclaration	= accessModifier "class" IDENTIFIER ("extends" IDENTIFIER | "") "{" {accessModifier (functionDeclaration | variableDeclaration)} "}" .
accessModifier		= "private" 
			| "public" .
variableDeclaration	= type IDENTIFIER initializer ";" .
type			= "str" 
			| "char" 
			| "int" 
			| "dec" 
			| "bool" 
			| IDENTIFIER
			| ("Set" | "List") "<"type">"
			| "Func""<"(type | "void") {"," type}">" .
initializer		= ["=" nonAssignment] .
functionDeclaration	= IDENTIFIER "("parameters")" "returns" (type | "void") block .
parameters		= [type IDENTIFIER {"," type IDENTIFIER}] .
statement		= ifStatement 
			| whileStatement 
			| returnStatement 
			| continueStatement 
			| breakStatement 
			| block 
			| expressionStatement
			| graphStatement .
ifStatement		= "if" "("expression")" block ["else" block] .
whileStatement		= "while" "("expression")" block .
returnStatement		= "return" [expression] ";" .
continueStatement	= "continue"";" .
breakStatement		= "break"";" .
block			= "{" {declaration} "}" .
expressionStatement	= expression";" .
graphStatement		= IDENTIFIER "{" {graphOperation} "}"";" .
graphOperation		= predicateOperation
			| "V" vertexOperation
			| nonAssignment "<<" (nonAssignment | "null");
			| addGraph
			| expressionStatement
			| graphWhile
			| graphIf .
predicateOperation	= predicate ("=>" | "<=>") predicate (nonAssignment | "") ";"
			| predicate "=/=" predicate";"
			| predicate ("++"|"--") set";" .
vertexOperation 	= "-" predicate ";"
			| "+" set [nonAssignment] ";" .
addGraph		= "++" IDENTIFIER ";" .
graphWhile		= "while" "("expression")" graphBlock .
graphIf			= "if" "("expression")" graphBlock ["else" graphBlock] .
graphBlock		= "{" {graphOperation | expressionStatement} "}" .
predicate		= "[" predicateOr "]" .
predicateOr		= predicateAnd {"or" predicateAnd} .
predicateAnd		= predicateUnary {"and" predicateUnary} .
predicateUnary		= "!"predicateUnary
			| predicatePrimary .
predicatePrimary	= "(" predicateOr ")"
			| additive .
expression		= assignment .
assignment		= nonAssignment ["=" nonAssignment] .
nonAssignment 		= or 
			| anonymousFunction
			| "new" IDENTIFIER "("arguments")" .
anonymousFunction	= "("parameters")" "=>" block .
or			= and {"or" and} .
and			= equality {"and" equality} .
equality 		= comparison {("==" | "!=") comparison} .
comparison		= additive {("<" | "<=" | ">=" | ">") additive} .
additive		= multiplicative {("+" | "-") multiplicative} .
multiplicative		= unary {("*"| "/" | "mod") unary} .
unary			= ("-" | "!") unary
			| call .
call 			= primary {elementAccess | "(" arguments ")"} {"." call} .
elementAccess		= "[" nonAssignment "]" .
arguments		= [nonAssignment {"," nonAssignment}] .
primary 		= "(" expression ")" 
			| STRING 
			| CHAR 
			| INTEGER 
			| DECIMAL
			| "true" 
			| "false" 
			| IDENTIFIER
			| set 
			| list
			| "null"
			| "this"
			| "super" .
set			= "{" arguments "}" .
list			= "[" arguments "]" .
```


