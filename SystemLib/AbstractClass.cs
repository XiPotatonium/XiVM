using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace SystemLib
{
    public abstract class AbstractClass
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        public Class Class { get; }
        public List<AbstractMethod> Methods { get; } = new List<AbstractMethod>();

        protected AbstractClass(string name)
        {
            Class = Constructor.AddClass(name);
        }

        internal abstract void DeclarationGen();

        internal virtual void CodeGen()
        {
            Constructor.CurrentBasicBlock = Class.StaticInitializer.BasicBlocks.First.Value;
            Constructor.AddRet();

            foreach (AbstractMethod method in Methods)
            {
                method.MethodGen();
            }
        }
    }
}
