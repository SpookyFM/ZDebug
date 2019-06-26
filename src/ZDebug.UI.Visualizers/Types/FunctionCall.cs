using System;
using System.Collections.Generic;
using System.Reflection;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A FunctionCall encodes calling a function
    /// </summary>
    class FunctionCall : Expression, IValueSource
    {
        public FunctionCall(string functionName, List<IValueSource> arguments)
        {
            this.FunctionName = functionName;
            Arguments = arguments;
        }

        public override bool Execute(ExecutionContext context)
        {
            bool ok = TryGetValue(context, out ushort result);
            return ok;
        }

        public ushort GetValue(ExecutionContext context)
        {
            bool ok = TryGetValue(context, out ushort result);
            return result;
        }

        private bool TryGetValue(ExecutionContext context, out ushort result)
        {
            MethodInfo info = context.GetType().GetMethod(this.FunctionName);
            if (info == null)
            {
                result = 0;
                return false;
            }

            object resultObj = info.Invoke(context, GetArguments(context).ToArray());
            if (resultObj != null)
            {
                if (resultObj.GetType() == typeof(ushort))
                {
                    result = (ushort)resultObj;
                }
                else
                {
                    result = (byte)resultObj;
                }

            }
            else
            {
                result = 0;
            }

            return true;
        }

        private List<object> GetArguments(ExecutionContext context)
        {
            List<object> result = new List<object>();
            foreach (IValueSource current in Arguments)
            {
                result.Add(current.GetValue(context));
            }
            return result;
        }

        public String FunctionName;
        public List<IValueSource> Arguments = new List<IValueSource>();
    }
}
