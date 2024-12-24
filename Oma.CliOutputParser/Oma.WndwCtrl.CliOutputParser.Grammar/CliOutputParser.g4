parser grammar CliOutputParser;

options {
    tokenVocab=CliOutputLexer;
}

// Parser rules
transformation  : statement+ EOF;
statement       : anchorStatement | regexStatement | valuesStatement;

anchorStatement : ANCHOR DOT FROM LPAREN STRING_LITERAL RPAREN (DOT TO LPAREN STRING_LITERAL RPAREN | DOT TO_END LPAREN RPAREN)? SEMI;
regexStatement  : REGEX DOT MATCH LPAREN REGEX_LITERAL RPAREN DOT ( YIELD_GROUP LPAREN INT RPAREN | YIELD_ALL LPAREN RPAREN ) SEMI;
valuesStatement : VALUES DOT aggregateFunction LPAREN RPAREN SEMI;

aggregateFunction
    : AVERAGE
    | MEDIAN
    | MAX
    | MIN
    | SUM
    ;