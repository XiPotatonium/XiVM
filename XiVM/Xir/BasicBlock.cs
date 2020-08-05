using System.Collections.Generic;

namespace XiVM.Xir
{
    public class BasicBlock
    {
        public Function Function { get; }

        /// <summary>
        /// BasicBlock的指令在函数所有指令中的offset
        /// 仅在函数最后生成时计算Jmp的offset的时候被计算和使用
        /// 可以看作是一个临时缓存, 不需要随时维护
        /// </summary>
        public int Offset { set; get; }
        /// <summary>
        /// BasicBlock指令的byte数组大小
        /// 仅在函数最后生成时计算Jmp的offset的时候被计算和使用
        /// 可以看作是一个临时缓存, 不需要随时维护
        /// </summary>
        public int InstLength { set; get; }

        public LinkedList<Instruction> Instructions { get; } = new LinkedList<Instruction>();
        public List<BasicBlock> JmpTargets { get; } = new List<BasicBlock>();

        internal BasicBlock(Function function)
        {
            Function = function;
        }
    }
}
