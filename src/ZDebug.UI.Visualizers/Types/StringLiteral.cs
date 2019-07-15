using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    class StringLiteral : IValueSource
    {
        private string Value;

        public StringLiteral(string value)
        {
            Value = value;
        }

        public string GetValue(ExecutionContext context)
        {
            return Value;
        }

        public ushort GetWordValue(ExecutionContext context)
        {
            return 0;
        }

        public string GetStringValue(ExecutionContext context)
        {
            return Value;
        }

        public System.Type GetValueType()
        {
            return typeof(string);
        }
    }
}
