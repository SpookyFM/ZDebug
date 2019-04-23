using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Services
{
    public sealed class PropertyViewChangedArgs : EventArgs
    {

        public PropertyViewChangedArgs(int number, PropertyView newView)
        {
            this.Number = number;
            this.NewView = newView;
        }

        public int Number
        {
            get;
            private set;
        }

        public PropertyView NewView
        {
            get;
            private set;
        }
    }
}
