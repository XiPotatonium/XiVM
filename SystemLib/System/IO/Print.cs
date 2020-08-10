using System.Collections.Generic;
using XiVM;

namespace SystemLib.System.IO
{
    internal class PutChar : AbstractMethod
    {
        public PutChar(AbstractClass parent)
            : base(parent, "Write", null, new List<VariableType>()
            {
                VariableType.IntType
            }, new AccessFlag() { IsStatic = true })
        {

        }
        public override void MethodGen()
        {
            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(Method);

            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPutC();

            Constructor.AddRet();
        }
    }

    /// <summary>
    /// TODO 用StringType
    /// </summary>
    internal class Write : AbstractMethod
    {
        public Write(AbstractClass parent)
            : base(parent, "Write", null, new List<VariableType>()
            {
                VariableType.AddressType
            }, new AccessFlag() { IsStatic = true })
        {

        }

        public override void MethodGen()
        {
            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(Method);

            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPutS();

            Constructor.AddRet();
        }
    }
}
