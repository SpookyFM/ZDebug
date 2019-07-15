using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDebug.Core.Basics;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Services
{
    abstract class ExecutionContextBase : ExecutionContext
    {
        private MemoryReader reader;

        private StringBuilder result;

        public ExecutionContextBase(byte[] memory)
        {
            reader = new MemoryReader(memory, 0);
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

        public override void print(params object[] args)
        {
            if (args.Length == 2)
            {
                ushort value = (ushort)args[0];
                ushort radix = (ushort)args[1];
                result.Append(Convert.ToString(value, radix));
            } else
            {
                string value = (string)args[0];
                result.Append(value);
            }
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
