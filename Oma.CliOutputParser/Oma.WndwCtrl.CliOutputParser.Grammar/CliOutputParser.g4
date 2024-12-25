parser grammar CliOutputParser;

options {
    tokenVocab=CliOutputLexer;
}

// Parser rules
transformation  : statement+ EOF;

statement       : map | multiply | reduce;

map             : anchorFrom 
                | anchorTo 
                ;

multiply        : regexMatch ;

reduce          : regexYield 
                | valuesAvg | valuesSum | valuesMin | valuesMax 
                | valuesFirst | valuesLast 
                | valuesAt;

anchorFrom      : ANCHOR DOT FROM LPAREN STRING_LITERAL RPAREN SEMI;
anchorTo        : ANCHOR DOT TO LPAREN STRING_LITERAL RPAREN SEMI;

regexMatch      : REGEX DOT MATCH LPAREN REGEX_LITERAL RPAREN SEMI;
regexYield      : REGEX DOT YIELD_GROUP LPAREN INT RPAREN SEMI;

// Would be cleaner to combine those.. 
valuesAvg       : VALUES DOT AVERAGE LPAREN RPAREN SEMI;
valuesSum       : VALUES DOT SUM LPAREN RPAREN SEMI;
valuesMin       : VALUES DOT MIN LPAREN RPAREN SEMI;
valuesMax       : VALUES DOT MAX LPAREN RPAREN SEMI;

valuesFirst     : VALUES DOT FIRST LPAREN RPAREN SEMI;
valuesLast      : VALUES DOT LAST LPAREN RPAREN SEMI;
valuesAt        : VALUES DOT (AT | INDEX) LPAREN INT RPAREN SEMI;