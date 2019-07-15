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
    public abstract class ExecutionContext
    {
        public Dictionary<string, byte> ByteVariables = new Dictionary<string, byte>();
        public Dictionary<string, ushort> WordVariables = new Dictionary<string, ushort>();

        public abstract ushort readWord();

        public abstract void seek(ushort offset);

        public abstract byte readByte();

        public abstract void print(params object[] args);
        
    }
}
