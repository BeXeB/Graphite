# Graphite

## Grammar
```
program 		→ declaration* EOF  
declaration		→ classDecl
			| variableDecl 
			| funcDecl 
			| stmt  
classDecl		→ accessMod "class" IDENTIFIER ("extends" IDENTIFIER | "") "{" (accessMod (funcDecl | variableDecl))* "}"  
accessMod		→ "private" 
			| "public"  
variableDecl		→ type IDENTIFIER initializer ";"  
type			→ "str" 
			| "char" 
			| "int" 
			| "dec" 
			| "bool" 
			| IDENTIFIER
			| "Func""<"type("," type)*">"  
initializer		→ "=" expression 
			| ""  
funcDecl		→ IDENTIFIER "("parameters")" "returns" (type | "void") block  
parameters		→ type IDENTIFIER ("," type IDENTIFIER)*
			| ""  
stmt			→ ifStmt 
			| whileStmt 
			| returnStmt 
			| continueStmt 
			| breakStmt 
			| block 
			| exprStmt
			| graphStmt  
ifStmt			→ "if" "("expression")" block ("else" block | "")  
whileStmt		→ "while" "("expression")" block  
returnStmt		→ "return" (expression | "") ";"  
continueStmt		→ "continue"";"  
breakStmt		→ "break"";"  
block			→ "{"declaration*"}"  
exprStmt		→ expression";"  
graphStmt		→ IDENTIFIER "{" graphOperation* "}"";"  
graphOperation		→ predicate predOperation
			| "V" vertexOperation
			| STRING "<<" (STRING | "null");
			| exprStmt
			| graphWhile
			| graphIf  
predOperation		→ ("=>" | "<=>") predicate (INTEGER | DECIMAL | "") ";"
			| "=/=" predicate";"
			| ("++"|"--") set";"  
vertexOperation 	→ "-" predicate ";"
			| "+" set (INTEGER | "") ";"  
graphWhile		→ "while" "("expression")" graphBlock  
graphIf			→ "if" "("expression")" graphBlock "else" graphBlock   
graphBlock		→ "{" (graphOperation | exprStmt)*  "}"  
predicate		→ "[" predOr "]"  
predOr			→ predAnd ("or" predAnd)*  
predAnd			→ predPrimary ("and" predPrimary)*
predUnary		→ ("!" | "") predPrimary
predPrimary		→ "(" predOr ")"
			| STRING
expression		→ assignment 
			| nonAssignment  	
assignment		→ IDENTIFIER "=" nonAssignment 
			| call "." IDENTIFIER "=" expression 
			| "new" IDENTIFIER "("arguments")"  
nonAssignment 		→ or 
			| anonFunc  
anonFunc		→ "("parameters")" "=>" block  
or			→ and ("or" and)*  
and			→ equality ("and" equality)*  
equality 		→ comparison (("==" | "!=") comparison)*  
comparison		→ additive (("<" | "<=" | ">=" | ">") additive)*  
additive		→ mult (("+" | "-") mult)*  
mult			→ unary (("*"| "/" | "mod") unary)*  
unary			→ ("-" | "!") unary
			| call 
			| primary  
call 			→ IDENTIFIER ("(" arguments ")" | "") ("." call)*  
arguments		→ expression ("," expression)* 
			| ""  
primary 		→ "(" expression ")" 
			| STRING 
			| CHAR 
			| INTEGER 
			| DECIMAL
			| "true" 
			| "false" 
			| IDENTIFIER 
			| set 
			| list
			| elementAccess
			| "null"  
set			→ "{" elements "}"  
list			→ "[" elements "]"  
elementAccess		→ primary "[" arguments "]"  
elements		→ expression ("," expression)* 
			| ""  
```


