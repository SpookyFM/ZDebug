using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Visualizers.Types
{
    public class Program : Block
    {
        public Program(List<Expression> sequence) : base(sequence)
        {

        }

        public String Name
        {
            get;
            set;
        }

    }
}
