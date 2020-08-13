using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM
{
    public class Field : Variable, IClassMember
    {
        public Class Parent { get; set; }
        public AccessFlag AccessFlag { get; set; }
        public int ConstantPoolIndex { get; set; }

        internal Field(AccessFlag flag, Class parent, VariableType type, int index)
            : base(type)
        {
            Parent = parent;
            AccessFlag = flag;
            ConstantPoolIndex = index;
        }
    }

    internal class VMField : Variable
    {
        public AccessFlag AccessFlag { get; }
        public int ClassIndex { get; }

        internal VMField(uint flag, VariableType type, int classIndex, int offset)
            : base(type)
        {
            Offset = offset;
            AccessFlag = new AccessFlag() { Flag = flag };
            ClassIndex = classIndex;
        }
    }
}
