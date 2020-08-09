namespace XiVM.Runtime
{
    /// <summary>
    /// 暂时只区分是不是地址
    /// </summary>
    internal enum SlotDataTag
    {
        ADDRESS,
        OTHER
    }

    internal struct Slot
    {
        public SlotDataTag DataTag { set; get; }
        public int Data { set; get; }
    }
}
