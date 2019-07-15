﻿using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    class Literal : IValueSource
    {
        public Literal(ushort value)
        {
            Value = value;
        }

        public ushort Value;

        public ushort GetWordValue(ExecutionContext context)
        {
            return Value;
        }

        public string GetStringValue(ExecutionContext context)
        {
            return null;
        }

        public System.Type GetValueType()
        {
            return typeof(ushort);
        }
    }
}
