lexer grammar CliOutputLexer;

STRING_LITERAL  : '"' (~["\\] | '\\' .)* '"';
REGEX_LITERAL   : '$' '"' (~["\\] | '\\' .)* '"';
INT             : [0-9]+;
COMMENT         : '/*' .*? '*/' -> skip;
LINE_COMMENT    : '//' ~[\r\n]* -> skip;
WS              : [ \t\n\r]+ -> skip;

ANCHOR          : 'Anchor';
FROM            : 'From';
TO              : 'To';
TO_END          : 'ToEnd';
REGEX           : 'Regex';
MATCH           : 'Match';
YIELD_GROUP     : 'YieldGroup';
YIELD_ALL       : 'YieldAll';
YIELD           : 'Yield';

VALUES          : 'Values';
FIRST           : 'First';
LAST            : 'Last';

AVERAGE         : 'Average';
MAX             : 'Max';
MIN             : 'Min';
SUM             : 'Sum';
AT              : 'At';
INDEX           : 'Index';

DOT             : '.';
LPAREN          : '(';
RPAREN          : ')';
SEMI            : ';';
COLON           : ':';