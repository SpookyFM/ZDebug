using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Services
{
    public sealed class GlobalViewChangedArgs : EventArgs
    {

        public GlobalViewChangedArgs(int index, VariableView newView)
        {
            this.Index = index;
            this.NewView = newView;
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
