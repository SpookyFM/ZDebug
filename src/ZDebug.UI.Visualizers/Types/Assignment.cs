using System;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// An Assignment assigns a value to a previously declared variable
    /// </summary>
    class Assignment : Expression
    {
        public Assignment(String variable, IValueSource valueSource)
        {
            Variable = variable;
            ValueSource = valueSource;
        }

        public override bool Execute(ExecutionContext context)
        {
            ushort value = ValueSource.GetWordValue(context);
            if (context.WordVariables.ContainsKey(Variable))
            {
                context.WordVariables[Variable] = value;
            }
            else if (context.ByteVariables.ContainsKey(Variable))
            {
                context.ByteVariables[Variable] = (byte)value;
            }
            return true;
        }

        public string Variable;
        public IValueSource ValueSource;
    }
}
