using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Visualizers.Execution
{
    /// <summary>
    /// An ExecutionContext saves the state of the program and provides the functions to be called
    /// </summary>
    class ExecutionContext
    {
        public Dictionary<string, byte> ByteVariables = new Dictionary<string, byte>();
        public Dictionary<string, ushort> WordVariables = new Dictionary<string, ushort>();

        public ushort readWord()
        {
            return 1024;
        }

        public void seek(ushort offset)
        {
            Console.WriteLine("Seeking to " + offset);
        }

        public byte readByte()
        {
            return 128;
        }

        public void print(ushort value, int radix)
        {
            string result = Convert.ToString(value, radix);
        }
    }
}
