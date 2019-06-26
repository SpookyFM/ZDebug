using System.Collections.Generic;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A block is a sequence of expressions
    /// </summary>
    class Block : Expression
    {
        public Block(List<Expression> sequence)
        {
            Sequence = sequence;
        }

        public override bool Execute(ExecutionContext context)
        {
            foreach (Expression i in Sequence)
            {
                bool result = i.Execute(context);
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
        public List<Expression> Sequence;


    }
}
