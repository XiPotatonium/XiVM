using System;
using System.Collections.Generic;
using System.Text;

namespace XiVM.Runtime
{
    /// <summary>
    /// 暂时只区分是不是地址
    /// </summary>
    enum SlotDataTag
    {
        ADDRESS,
        OTHER
    }

    struct Slot
    {
        public SlotDataTag DataTag { set; get; }
        public int Data { set; get; }
    }
}
