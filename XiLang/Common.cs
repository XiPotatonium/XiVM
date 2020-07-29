namespace XiLang
{
    public enum SyntacticValueType
    {
        BOOL, INT, DOUBLE, STRING, CLASS, VOID
    }

    public enum ValueType
    {
        INT, DOUBLE, STRING, BOOL, NULL
    }

    public enum OpType
    {
        NEG, INC, DEC,
        ADD, SUB, MUL, DIV, MOD,
        LOG_NOT, LOG_AND, LOG_OR,
        BIT_NOT, BIT_AND, BIT_XOR, BIT_OR, BIT_SL, BIT_SR,
        EQ, NE, GE, GT, LE, LT,
        ASSIGN, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, SL_ASSIGN, SR_ASSIGN,
        CONDITIONAL,
        CAST, CALL, CLASS_ACCESS, ARRAY_ACCESS
    }
}
