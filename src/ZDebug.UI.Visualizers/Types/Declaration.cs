using System;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A declaration declares a new variable
    /// </summary>
    class Declaration : Expression
    {

        public Declaration(String variableType, String variableName)
        {
            VariableType = variableType == "byte" ? VariableType.ByteType : VariableType.WordType;
            VariableName = variableName;
        }

        public override bool Execute(ExecutionContext context)
        {
            switch (VariableType)
            {
                case VariableType.ByteType:
                    {
                        var dict = context.ByteVariables;
                        if (dict.ContainsKey(VariableName))
                        {
                            return false;
                        }
                        dict[VariableName] = 0;
                    }
                    break;
                case VariableType.WordType:
                    {
                        var dict = context.WordVariables;
                        if (dict.ContainsKey(VariableName))
                        {
                            return false;
                        }
                        dict[VariableName] = 0;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public VariableType VariableType;
        public String VariableName;
    }
}
