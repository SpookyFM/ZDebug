﻿using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A VariableReference refers to a previously declared variable
    /// </summary>
    class VariableReference : IValueSource
    {
        public VariableReference(string variableName)
        {
            VariableName = variableName;
        }


        public string VariableName;

        public ushort GetValue(ExecutionContext context)
        {
            if (context.ByteVariables.ContainsKey(VariableName))
            {
                return context.ByteVariables[VariableName];
            }
            else
            {
                return context.WordVariables[VariableName];
            }
        }
    }
}