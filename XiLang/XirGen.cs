using XiLang.AbstractSyntaxTree;
using XiLang.PassMgr;
using XiLang.Symbol;
using XiVM.Xir;

namespace XiLang
{
    internal class XirGenPass : IASTPass
    {
        public static XirGenPass Singleton { get; } = new XirGenPass();
        public static void InitSingleton(string moduleName)
        {
            ModuleConstructor = new ModuleConstructor(moduleName);
            VariableSymbolTable = new SymbolTable<VariableSymbol>();
        }

        public static ModuleConstructor ModuleConstructor { private set; get; }
        public static SymbolTable<VariableSymbol> VariableSymbolTable { private set; get; }
        // 函数和类的符号表待定，如果ModuleConstructor可以代劳那么就不需要了

        private XirGenPass() { }

        public object Run(AST root)
        {
            AST.CodeGen(root);
            return null;
        }

        public void Dump(string dirName)
        {
            ModuleConstructor.Dump(dirName);
        }
    }
}
