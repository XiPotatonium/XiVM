using System.Collections.Generic;

namespace XiVM.Xir
{
    public class BasicBlock
    {
        public Function Function { get; }

        public List<Instruction> Instructions { get; } = new List<Instruction>();
        public List<Instruction> Jmps { get; } = new List<Instruction>();
        public List<BasicBlock> JmpTargets { get; } = new List<BasicBlock>();

        internal BasicBlock(Function function)
        {
            Function = function;
        }
    }
}
