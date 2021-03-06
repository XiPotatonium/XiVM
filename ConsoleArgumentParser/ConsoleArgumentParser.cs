﻿using System;
using System.Collections.Generic;

namespace ConsoleArgumentParser
{
    public class ArgumentParser
    {
        private Dictionary<string, ConsoleArgument> Arguments { get; } = new Dictionary<string, ConsoleArgument>();
        /// <summary>
        /// 目前default rule只支持一个string，如果要支持数组再说
        /// </summary>
        public ConsoleArgument DefaultRule { get; }

        /// <summary>
        /// 必须要有一个参数，这个参数是默认规则（不需要-xxx）
        /// </summary>
        /// <param name="defaultArgument"></param>
        public ArgumentParser(ConsoleArgument defaultArgument)
        {
            DefaultRule = defaultArgument;
        }

        public void AddArgument(ConsoleArgument consoleArgument)
        {

            Arguments.Add(consoleArgument.FullName, consoleArgument);
            if (!string.IsNullOrEmpty(consoleArgument.Alias))
            {
                Arguments.Add(consoleArgument.Alias, consoleArgument);
            }
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith('-'))
                {
                    if (Arguments.TryGetValue(args[i].Substring(1), out ConsoleArgument arg))
                    {
                        arg.IsSet = true;
                        switch (arg.ValueType)
                        {
                            case ArgumentValueType.STRING:
                                arg.StringValue = args[++i];
                                break;
                            case ArgumentValueType.INT:
                                arg.IntValue = Convert.ToInt32(args[++i]);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        throw new Exception($"Unexpect argument {args[i]}");
                    }
                }
                else
                {
                    if (DefaultRule.IsSet)
                    {
                        throw new Exception($"Duplicated default rule {DefaultRule.StringValue} and {args[i]}");
                    }
                    else
                    {
                        DefaultRule.StringValue = args[i];
                        DefaultRule.IsSet = true;
                    }
                }
            }
            if (!DefaultRule.IsSet)
            {
                throw new Exception($"Default rule not set");
            }
        }
    }
}
