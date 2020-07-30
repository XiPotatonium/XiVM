using System.Collections.Generic;

namespace XiVM.Xir
{
    public class BasicBlock
    {
        public Function Function { get; }

        public LinkedList<Instruction> Instructions { get; } = new LinkedList<Instruction>();
        public List<Instruction> Jmps { get; } = new List<Instruction>();
        public List<BasicBlock> JmpTargets { get; } = new List<BasicBlock>();

        internal BasicBlock(Function function)
        {
            Function = function;
        }
    }
}
