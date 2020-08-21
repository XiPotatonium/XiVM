using System;

namespace XiVM.Runtime
{
    /// <summary>
    /// 预留地址(相对地址)和对应地址表示的东西
    /// </summary>
    public enum PreservedAddressTag
    {
        /// <summary>
        /// 空设备
        /// </summary>
        NULL = 0,
        /// <summary>
        /// 字符输入输出
        /// </summary>
        STDCHARIO = 1,
        /// <summary>
        /// 标准类型输入输出
        /// </summary>
        STDTIO = 2,
        /// <summary>
        /// 字符串输入输出
        /// </summary>
        STDSTRINGIO = 3
    }

    public static class Preserved
    {
        public static readonly int SizeLimit = 99;

        public static uint GetAbsoluteAddress(PreservedAddressTag tag)
        {
            return MemoryMap.MapToAbsolute((uint)tag, MemoryTag.PRESERVED);
        }

        internal static void SetInt(uint offset, int value)
        {
            PreservedAddressTag tag = (PreservedAddressTag)offset;
            switch (tag)
            {
                case PreservedAddressTag.NULL:
                    break;
                case PreservedAddressTag.STDCHARIO:
                    Console.Write((char)value);
                    break;
                case PreservedAddressTag.STDTIO:
                    Console.Write(value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static int GetInt(Stack stack)
        {
            throw new NotImplementedException();
        }

        internal static void SetDouble(uint offset, double value)
        {
            throw new NotImplementedException();
        }

        internal static double GetDouble(Stack stack)
        {
            throw new NotImplementedException();
        }

        internal static void SetAddress(uint offset, uint value)
        {
            PreservedAddressTag tag = (PreservedAddressTag)offset;
            switch (tag)
            {
                case PreservedAddressTag.NULL:
                    break;
                case PreservedAddressTag.STDSTRINGIO:
                    Console.Write(VMExecutor.GetString(value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static uint GetAddress(Stack stack)
        {
            throw new NotImplementedException();
        }
    }
}
