using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDebug.Core.Basics;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Services
{
    class PropertyViewExecutionContext: ExecutionContext
    {

        private readonly byte[] value;

        private MemoryReader reader;

        private StringBuilder result;

        public PropertyViewExecutionContext(byte[] value)
        {
            this.value = value;
            reader = new MemoryReader(value, 0);
            result = new StringBuilder();
        }

        public override ushort readWord()
        {
            return reader.NextWord();
        }

        public override void seek(ushort offset)
        {
            reader.Address = offset;
        }

        public override byte readByte()
        {
            return reader.NextByte();
        }

        public override void print(ushort value, int radix)
        {
            result.Append(Convert.ToString(value, radix));
        }

        public string Result
        {
            get
            {
                return result.ToString();
            }
        }
    }
}
