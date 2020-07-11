namespace XiLang
{
    public enum BasicVarType
    {
        BOOL, I32, F32, VOID, STRING,
        USER_DEF        // 用户定义的类型，比如class
    }

    public enum OpType
    {
        NEG, INC, DEC,
        ADD, SUB, MUL, DIV, MOD,
        LOG_NOT, LOG_AND, LOG_OR,
        BIT_AND, BIT_XOR, BIT_OR, BIT_SL, BIT_SR,
        EQ, NE, GE, GT, LE, LT,
        ASSIGN, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, SL_ASSIGN, SR_ASSIGN,
        CONDITIONAL,
        CAST, CALL, CLASS_ACCESS, ARRAY_ACCESS
    }

    public enum ValueType
    {
        INT, FLOAT, STR, BOOL, NULL
    }
}
