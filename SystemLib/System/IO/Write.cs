using System.Collections.Generic;
using XiVM;
using XiVM.Runtime;

namespace SystemLib.System.IO
{
    public class WriteString : AbstractMethod
    {
        public static readonly string MethodName = "Write";

        internal WriteString(AbstractClass parent)
            : base(parent)
        {

        }

        internal override void DeclarationGen()
        {
            Class stringClass = Program.ModuleConstructor.Classes.Find(c => c.Name == String.String.ClassName);
            List<VariableType> ps = new List<VariableType>()
            {
                stringClass.ObjectType
            };
            Method = Constructor.AddMethod(Parent.Class, MethodName,
                null, ps, new AccessFlag() { IsStatic = true });
        }

        internal override void MethodGen()
        {
            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPushA(Preserved.GetAbsoluteAddress(PreservedAddressTag.STDSTRINGIO));
            Constructor.AddStoreT(Method.Params[0].Type);

            Constructor.AddRet();
        }
    }

    public class WriteInt : AbstractMethod
    {
        public static readonly string MethodName = "Write";

        internal WriteInt(AbstractClass parent)
            : base(parent)
        {

        }

        internal override void DeclarationGen()
        {
            List<VariableType> ps = new List<VariableType>()
            {
                VariableType.IntType
            };
            Method = Constructor.AddMethod(Parent.Class, MethodName,
                null, ps, new AccessFlag() { IsStatic = true });
        }

        internal override void MethodGen()
        {
            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPushA(Preserved.GetAbsoluteAddress(PreservedAddressTag.STDTIO));
            Constructor.AddStoreT(Method.Params[0].Type);

            Constructor.AddRet();
        }
    }
}
