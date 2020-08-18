using System.Collections.Generic;
using XiVM;

namespace SystemLib.System.String
{
    public class String : AbstractClass
    {
        public static readonly string ClassName = "String";
        internal String() : base(ClassName)
        {

        }

        internal override void DeclarationGen()
        {
            Constructor.AddClassField(Class, "Empty", Class.ObjectType, new AccessFlag() { IsStatic = true });
            Constructor.AddClassField(Class, "Length", VariableType.IntType, AccessFlag.DefaultFlag);
            // 实际上是char数组，但是因为不区分byte和char所以使用byte
            Constructor.AddClassField(Class, "Data", ArrayType.ByteArrayType, AccessFlag.DefaultFlag);

            Methods.Add(new StringInit(this));
        }

        internal override void StaticInitializerGen()
        {
            Class.Fields.TryGetValue("Empty", out Field field);
            Constructor.AddNew(Class.ConstantPoolIndex);
            Constructor.AddDup(Class.ObjectType);
            Constructor.AddConst(Constructor.StringPool.TryAdd(string.Empty));
            AbstractMethod abstractMethod = Methods.Find(m => m is StringInit);
            Constructor.AddCall(abstractMethod.Method.ConstantPoolIndex);
            Constructor.AddStoreStatic(field.ConstantPoolIndex);
            Constructor.AddPop(Class.ObjectType);

            Constructor.AddRet();
        }
    }

    public class StringInit : AbstractMethod
    {
        internal StringInit(AbstractClass parent)
            : base(parent)
        {

        }

        internal override void DeclarationGen()
        {
            Method = Constructor.AddMethod(Parent.Class, "(init)", null, new List<VariableType>()
            {
                Parent.Class.ObjectType
            }, AccessFlag.DefaultFlag);
        }

        internal override void MethodGen()
        {
            Parent.Class.Fields.TryGetValue("Length", out Field lengthField);
            Parent.Class.Fields.TryGetValue("Data", out Field dataField);

            // 复制Length
            Constructor.AddLocal(Method.Params[1].Offset);      // 传入的String
            Constructor.AddLoadT(Method.Params[1].Type);
            Constructor.AddLoadNonStatic(lengthField.ConstantPoolIndex);

            Constructor.AddLocal(Method.Params[0].Offset);      // 构造对象
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddStoreNonStatic(lengthField.ConstantPoolIndex);
            Constructor.AddPop(lengthField.Type);

            // 复制Data
            Constructor.AddLocal(Method.Params[1].Offset);      // 传入的String
            Constructor.AddLoadT(Method.Params[1].Type);
            Constructor.AddLoadNonStatic(dataField.ConstantPoolIndex);

            Constructor.AddLocal(Method.Params[0].Offset);      // 构造对象
            Constructor.AddLoadT(Method.Params[0].Type);
            Constructor.AddStoreNonStatic(dataField.ConstantPoolIndex);
            Constructor.AddPop(dataField.Type);

            Constructor.AddRet();
        }
    }
}
