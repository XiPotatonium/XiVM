using System;
using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;

namespace XiLang.Symbol
{
    public class ClassSymbol : Symbol
    {
        public List<VarSymbol> Vars { private set; get; }
        public List<FuncSymbol> Funcs { private set; get; }

        public ClassSymbol(string name) : base(name)
        {

        }

        /// <summary>
        /// TODO 暂时符号表仅用于判断ID是不是一个类，不需要详细的信息
        /// </summary>
        /// <param name="stmt"></param>
        public void Init(ClassStmt stmt)
        {
            throw new NotImplementedException();
        }
    }
}
