using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace SystemLib
{
    internal abstract class AbstractClass
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        public ClassType ClassType { private set; get; }
        public List<AbstractMethod> Methods { protected set; get; }

        public AbstractClass(string name)
        {
            ClassType = Constructor.AddClassType(name);
        }

        public abstract void ClassGen();
    }
}
