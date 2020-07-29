using System;
using XiVM.Xir;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private uint IP { set; get; }
        private uint SP { set; get; }
        private uint BP { set; get; }

        private BinaryFunction CurrentFunction { set; get; }

        public VMExecutor()
        {
            CurrentFunction = Program.BinaryModule.Entry;
            IP = 0;
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
