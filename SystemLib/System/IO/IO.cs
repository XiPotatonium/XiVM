using System.Collections.Generic;
using XiVM;

namespace SystemLib.System.IO
{
    internal class IO : AbstractClass
    {
        public IO() : base("IO")
        {
            Methods = new List<AbstractMethod>()
            {
                new PutChar(this),
                new Write(this)
            };
        }

        public override void ClassGen()
        {
            Constructor.CurrentBasicBlock = ClassType.StaticInitializer.BasicBlocks.First.Value;
            Constructor.AddRet();

            foreach (AbstractMethod method in Methods)
            {
                method.MethodGen();
            }
        }
    }
}
