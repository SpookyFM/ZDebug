﻿using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A ForLoop is a block together with a loop variable and conditions for terminating
    /// </summary>
    class ForLoop : Expression
    {
        public VariableReference Reference;
        public IValueSource RangeStart;
        public IValueSource RangeEnd;
        public Block InnerBlock;

        public ForLoop(VariableReference reference, IValueSource rangeStart, IValueSource rangeEnd, Block innerBlock)
        {
            Reference = reference;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            InnerBlock = innerBlock;
        }

        public override bool Execute(ExecutionContext context)
        {
            ushort i = 0;
            for (ushort iter = RangeStart.GetValue(context); iter < RangeEnd.GetValue(context); iter++)
            {
                context.WordVariables[Reference.VariableName] = iter;
                bool result = InnerBlock.Sequence[i++].Execute(context);
                if (!result)
                {
                    return false;
                }
            }
            context.WordVariables.Remove(Reference.VariableName);
            return true;
        }
    }
}