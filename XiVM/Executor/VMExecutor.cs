using System;
using System.Collections;
using System.Collections.Generic;
using XiVM.Xir;

namespace XiVM.Executor
{
    public class VMExecutor
    {
        private uint IP { set; get; }
        private BinaryFunction CurrentFunction { set; get; }

        private RuntimeStack RuntimeStack { set; get; } = new RuntimeStack();

        private Stack<byte> ComputationStack { set; get; } = new Stack<byte>();
        /// <summary>
        /// 用于在计算栈中的byte序列转换为value时暂存byte
        /// 因为最大支持64位的value，所以只开byte[8]
        /// </summary>
        private byte[] Buffer { set; get; } = new byte[8];

        internal VMExecutor(BinaryModule binaryModule)
        {
            IP = 0;
            
            // TODO 将BinaryModule转化成便于执行的模式，比如说建哈希表
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
