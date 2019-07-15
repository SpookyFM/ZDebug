using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDebug.Core.Basics;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Services
{
    class PropertyViewExecutionContext: ExecutionContextBase
    {
        private MemoryReader propertyReader;

        public PropertyViewExecutionContext(byte[] value, byte[] memory) :
            base(memory)
        {
            propertyReader = new MemoryReader(value, 0);
        }

        public ushort readPropertyWord()
        {
            return propertyReader.NextWord();
        }

        public byte readPropertyByte()
        {
            return propertyReader.NextByte();
        }
    }
}
