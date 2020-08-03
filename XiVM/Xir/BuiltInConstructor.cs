using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Xir
{
    public partial class ModuleConstructor
    {
        public readonly ClassType ByteArrayType = new ClassType("Array<byte>");
        public readonly ClassType IntArrayType = new ClassType("Array<int>");
        public readonly ClassType DoubleArrayType = new ClassType("Array<double>");
        public readonly ClassType AddressArrayType = new ClassType("Array<addr>");
        public readonly ClassType StringType = new ClassType("String");

        private void ConsturctBuiltIn()
        {
            // 备份
            BasicBlock tmp = CurrentBasicBlock;

            ConstructPutChar();

            ConstructArrayType(ByteArrayType, VariableType.ByteType);
            ConstructArrayType(IntArrayType, VariableType.IntType);
            ConstructArrayType(DoubleArrayType, VariableType.DoubleType);
            ConstructArrayType(AddressArrayType, VariableType.AddressType);

            ConstructStringType();

            CurrentBasicBlock = tmp;
        }

        /// <summary>
        /// 预计会放到系统库中
        /// </summary>
        private void ConstructPutChar()
        {
            Function putchar = AddFunction("putchar", new FunctionType(null, new List<VariableType>() { VariableType.IntType }));
            CurrentBasicBlock = AddBasicBlock(putchar);
            AddLocalA(putchar.Params[0].Offset);
            AddLoadI();
            AddPutC();
            AddRet();
        }

        private void ConstructArrayType(ClassType arrayType, VariableType elementType)
        {
            arrayType.AddVariable(VariableType.IntType);
        }

        private void ConstructStringType()
        {
            StringType.AddVariable(VariableType.IntType);       // String.Length
            StringType.AddVariable(ByteArrayType);              // 数据

            // TODO 构造函数
        }
    }
}
