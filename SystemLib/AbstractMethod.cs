using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace SystemLib
{
    public abstract class AbstractMethod
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        public AbstractClass Parent { get; }
        public Method Method { protected set; get; }

        protected AbstractMethod(AbstractClass parent)
        {
            Parent = parent;
        }

        internal abstract void DeclarationGen();

        internal abstract void MethodGen();
    }
}
