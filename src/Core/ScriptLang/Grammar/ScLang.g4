grammar ScLang;

script
    : (topLevelStatement? EOL)*
    ;

topLevelStatement
    : K_SCRIPT_NAME identifier                                  #scriptNameStatement
    
    | K_PROC identifier parameterList EOL
      statementBlock
      K_ENDPROC                                                 #procedureStatement

    | K_FUNC returnType=type identifier parameterList EOL
      statementBlock
      K_ENDFUNC                                                 #functionStatement

    | K_PROTO K_PROC identifier parameterList                   #procedurePrototypeStatement
    | K_PROTO K_FUNC returnType=type identifier parameterList   #functionPrototypeStatement

    | K_NATIVE K_PROC identifier parameterList                   #procedureNativeStatement
    | K_NATIVE K_FUNC returnType=type identifier parameterList   #functionNativeStatement

    | K_STRUCT identifier EOL
      structFieldList
      K_ENDSTRUCT                                               #structStatement
    
    | variableDeclarationWithInitializer                        #staticVariableStatement
    ;

statement
    : variableDeclarationWithInitializer                        #variableDeclarationStatement
    | left=expression '=' right=expression                      #assignmentStatement // TODO: more assignment operators (+=, -=, *=, /=, ...)
    
    | K_IF condition=expression EOL
      thenBlock=statementBlock
      (K_ELSE EOL
      elseBlock=statementBlock)?
      K_ENDIF                                                   #ifStatement
    
    | K_WHILE condition=expression EOL
      statementBlock
      K_ENDWHILE                                                #whileStatement
    
    | K_RETURN expression?                                      #returnStatement
    | expression argumentList                                   #invocationStatement
    ;

statementBlock
    : (statement? EOL)*
    ;

expression
    : '(' expression ')'                                        #parenthesizedExpression
    | expression argumentList                                   #invocationExpression
    | expression arrayIndexer                                   #arrayAccessExpression
    | op=(K_NOT | '-') expression                               #unaryExpression
    | left=expression op=('*' | '/' | '%') right=expression     #binaryExpression
    | left=expression op=('+' | '-') right=expression           #binaryExpression
    | left=expression op='&' right=expression                   #binaryExpression
    | left=expression op='^' right=expression                   #binaryExpression
    | left=expression op='|' right=expression                   #binaryExpression
    | '<<' expression (',' expression)* '>>'                    #aggregateExpression
    | identifier                                                #identifierExpression
    | expression '.' identifier                                 #memberAccessExpression
    | (integer | float | string | bool)                         #literalExpression
    ;

variableDeclarationWithInitializer
    : decl=variableDeclaration ('=' initializer=expression)?
    ;

variableDeclaration
    : type identifier arrayIndexer?
    ;

structFieldList
    : (variableDeclarationWithInitializer? EOL)*
    ;

argumentList
    : '(' (expression (',' expression)*)? ')'
    ;

parameterList
    : '(' (variableDeclaration (',' variableDeclaration)*)? ')'
    ;

arrayIndexer
    : '[' expression ']'
    ;

type
    : typeName isRef='&'?
    ;

typeName
    : K_BOOL
    | K_INT
    | K_FLOAT
    | K_VEC3
    | K_TEXT_LABEL
    | K_STRING
    | identifier
    ;

identifier
    : IDENTIFIER
    ;

integer
    : DECIMAL_INTEGER
    | HEX_INTEGER
    ;

float
    : FLOAT
    ;

string
    : STRING
    ;

bool
    : K_TRUE
    | K_FALSE
    ; 

// keywords
K_PROC : P R O C;
K_ENDPROC : E N D P R O C;
K_FUNC : F U N C;
K_ENDFUNC : E N D F U N C;
K_STRUCT : S T R U C T;
K_ENDSTRUCT : E N D S T R U C T;
K_PROTO : P R O T O;
K_NATIVE : N A T I V E;
K_TRUE : T R U E;
K_FALSE : F A L S E;
K_NOT : N O T;
K_IF : I F;
K_ELSE : E L S E;
K_ENDIF : E N D I F;
K_WHILE : W H I L E;
K_ENDWHILE : E N D W H I L E;
K_RETURN : R E T U R N;
K_SCRIPT_NAME : S C R I P T '_' N A M E;
K_BOOL : B O O L;
K_INT : I N T;
K_FLOAT : F L O A T;
K_VEC3 : V E C '3';
K_TEXT_LABEL : T E X T '_' L A B E L DIGIT+;
K_STRING : S T R I N G;

OP_ADD: '+';
OP_SUBTRACT: '-';
OP_MULTIPLY: '*';
OP_DIVIDE: '/';
OP_MODULO: '%';
OP_OR: '|';
OP_AND: '&';
OP_XOR: '^';

IDENTIFIER
    :   [a-zA-Z_] [a-zA-Z_0-9]*
    ;

FLOAT
    : DECIMAL_INTEGER '.' DIGIT+
    ;

DECIMAL_INTEGER
    :   [-+]? DIGIT+
    ;

HEX_INTEGER
    :   '0x' HEX_DIGIT+
    ;

STRING
    : UNCLOSED_STRING '"'
    ;

UNCLOSED_STRING
    : '"' (~["\\\r\n] | '\\' (. | EOF))*
    ;

COMMENT
    : '//' ~ [\r\n]* -> skip
    ;

EOL
    : [\r\n]+
    ;

WS
    : [ \t] -> skip
    ;

fragment DIGIT : [0-9];
fragment HEX_DIGIT : [0-9a-fA-F];

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];