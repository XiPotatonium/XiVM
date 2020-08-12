﻿using System.Collections.Generic;
using XiVM;

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
            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(Method);

            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPutS();

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
            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(Method);

            Constructor.AddLocal(Method.Params[0].Offset);
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddPutI();

            Constructor.AddRet();
        }
    }
}
