using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    class Literal : IValueSource
    {
        public Literal(ushort value)
        {
            Value = value;
        }

        public ushort Value;

        public ushort GetValue(ExecutionContext context)
        {
            return Value;
        }
    }
}
