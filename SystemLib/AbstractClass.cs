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

        /// <summary>
        /// 不用设置bb
        /// </summary>
        internal abstract void StaticInitializerGen();

        internal virtual void CodeGen()
        {
            Constructor.CurrentBasicBlock = Class.StaticInitializer.BasicBlocks.First.Value;
            StaticInitializerGen();

            foreach (AbstractMethod method in Methods)
            {
                Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(method.Method);
                method.MethodGen();
            }
        }
    }
}
