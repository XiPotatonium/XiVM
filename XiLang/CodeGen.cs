using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;
using XiLang.PassMgr;
using XiVM.Xir;

namespace XiLang
{
    internal class CodeGenPass : IASTPass
    {
        public static CodeGenPass Singleton { get; } = new CodeGenPass();
        public static void InitSingleton(string moduleName)
        {
            Constructor = new ModuleConstructor(moduleName);
        }

        public static ModuleConstructor Constructor { private set; get; }

        public static Stack<BasicBlock> Breakable { set; get; } = new Stack<BasicBlock>();
        public static Stack<BasicBlock> Continuable { set; get; } = new Stack<BasicBlock>();

        private CodeGenPass() { }

        public object Run(AST root)
        {
            // TODO 对于顶层需要进行特殊处理
            // 所有的Class、Function均需要建立但是不生成内部细节
            // 等于是先进行默认的声明
            AST.CodeGen(root);
            return null;
        }

        public void Dump(string dirName)
        {
            Constructor.Dump(dirName);
        }
    }
}
