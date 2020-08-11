namespace XiLang
{
    /// <summary>
    /// 语法上允许出现的type
    /// 这些Type会出现在AST中
    /// </summary>
    public enum SyntacticValueType
    {
        BOOL, INT, DOUBLE, STRING, CLASS, VOID
    }

    /// <summary>
    /// 这些Type是XiLang中的Value可能的Type
    /// </summary>
    public enum ValueType
    {
        BOOL, INT, DOUBLE, STRING, NULL
    }

    /// <summary>
    /// 运算符类型
    /// </summary>
    public enum OpType
    {
        NEG, INC, DEC,
        ADD, SUB, MUL, DIV, MOD,
        LOG_NOT, LOG_AND, LOG_OR,
        BIT_NOT, BIT_AND, BIT_XOR, BIT_OR, BIT_SL, BIT_SR,
        EQ, NE, GE, GT, LE, LT,
        ASSIGN, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, SL_ASSIGN, SR_ASSIGN,
        CONDITIONAL,
        CAST, CALL, CLASS_ACCESS, ARRAY_ACCESS, NEW
    }
}
