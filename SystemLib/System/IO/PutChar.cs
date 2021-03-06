﻿using System.Collections.Generic;
using XiVM;
using XiVM.Runtime;

namespace SystemLib.System.IO
{
    public class PutChar : AbstractMethod
    {
        public static readonly string MethodName = "PutChar";

        internal PutChar(AbstractClass parent)
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
            Constructor.AddPushA(Preserved.GetAbsoluteAddress(PreservedAddressTag.STDCHARIO));
            Constructor.AddStoreT(Method.Params[0].Type);

            Constructor.AddRet();
        }
    }
}
