using System;
using System.Collections.Generic;
using System.Text;

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
        STDCHARIO = 2,
        /// <summary>
        /// 整数输入输出
        /// </summary>
        STDINTIO = 3,
        /// <summary>
        /// 字符串输入输出
        /// </summary>
        STDSTRINGIO = 5
    }

    public static class Preserved
    {
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
                case PreservedAddressTag.STDINTIO:
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
