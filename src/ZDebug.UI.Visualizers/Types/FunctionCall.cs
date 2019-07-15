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

        public ushort GetWordValue(ExecutionContext context)
        {
            bool ok = TryGetValue(context, out ushort result);
            return result;
        }

        static bool IsParams(ParameterInfo param)
        {
            return param.IsDefined(typeof(ParamArrayAttribute), false);
        }

        private bool TryGetValue(ExecutionContext context, out ushort result)
        {
            MethodInfo info = context.GetType().GetMethod(this.FunctionName);
            if (info == null)
            {
                result = 0;
                return false;
            }

            object[] arguments;
            
            var parameters = info.GetParameters();
            if (parameters.Length == 1 && IsParams(parameters[0]))
            {
                arguments = new object[] { GetArguments(context).ToArray() };
            } else
            {
                arguments = GetArguments(context).ToArray();
            }
            object resultObj = info.Invoke(context, arguments);
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
                object v;
                if (current.GetValueType() == typeof(ushort))
                {
                    v = current.GetWordValue(context);
                } else
                {
                    v = current.GetStringValue(context);
                }
                result.Add(v);
            }
            return result;
        }

        public string GetStringValue(ExecutionContext context)
        {
            return null;
        }

        public System.Type GetValueType()
        {
            return typeof(ushort);
        }

        public String FunctionName;
        public List<IValueSource> Arguments = new List<IValueSource>();
    }
}
