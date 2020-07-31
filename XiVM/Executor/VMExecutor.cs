using System;
using XiVM.Xir;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private uint IP { set; get; }
        private BinaryFunction CurrentFunction { set; get; }

        private RuntimeStack RuntimeStack { get; } = new RuntimeStack();
        private RuntimeHeap RuntimeHeap { get; } = new RuntimeHeap();
        private ComputationStack ComputationStack { get; } = new ComputationStack();

        private BinaryFunction[] Functions { set; get; }
        private BinaryConstant[] Constants { set; get; }
        private BinaryClass[] Classes { set; get; }

        internal VMExecutor(BinaryModule binaryModule)
        {
            IP = 0;

            // TODO 将BinaryModule转化成便于执行的模式，比如说建哈希表
            Functions = binaryModule.Functions;
            Constants = binaryModule.Constants;
            Classes = binaryModule.Classes;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        private void ExecuteSingle()
        {
            throw new NotImplementedException();
        }
    }
}
