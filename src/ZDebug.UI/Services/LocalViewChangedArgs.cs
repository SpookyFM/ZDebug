using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Services
{
    public sealed class LocalViewChangedArgs : EventArgs
    {

        public LocalViewChangedArgs(int address, int index, VariableView newView)
        {
            this.Address = address;
            this.Index = index;
            this.NewView = newView;
        }

        public int Address
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public VariableView NewView
        {
            get;
            private set;
        }
    }
}
