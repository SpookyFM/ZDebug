using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDebug.Core.Basics;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Services
{
    class VariableViewExecutionContext : ExecutionContextBase
    {
        private readonly ushort value;

        public VariableViewExecutionContext(ushort value, byte[] memory):
            base(memory)
        {
            this.value = value;
            var bytes = BitConverter.GetBytes(value);
            ByteVariables["valueByte1"] = bytes[0];
            ByteVariables["valueByte2"] = bytes[1];
            WordVariables["valueWord"] = value;
        }

        
    }
}
