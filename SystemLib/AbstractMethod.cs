using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace SystemLib
{
    internal abstract class AbstractMethod
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        public AbstractClass Parent { private set; get; }
        public Method Method { private set; get; }

        public AbstractMethod(AbstractClass parent, string name, VariableType retType, List<VariableType> paramsType, AccessFlag flag)
        {
            Method = Constructor.AddMethod(parent.ClassType, name,
                Constructor.AddMethodType(retType, paramsType), flag);
        }

        public abstract void MethodGen();
    }
}
