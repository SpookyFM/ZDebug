using System.Linq;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Routines;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame stackFrame;
        private readonly ZRoutineTable routineTable;
        private readonly uint jumpToAddress;

        public StackFrameViewModel(StackFrame stackFrame, ZRoutineTable routineTable, uint jumpToAddress)
        {
            this.stackFrame = stackFrame;
            this.routineTable = routineTable;
            this.jumpToAddress = jumpToAddress;
        }

        public string Name
        {
            get { return routineTable.GetByAddress((int)stackFrame.CallAddress).Name; }
        }

        public bool HasName
        {
            get { return Name.Length > 0; }
        }

        public uint CallAddress
        {
            get { return stackFrame.CallAddress; }
        }

        /** The address to which we want to navigate when clicking the call stack */
        public uint JumpToAddress
        {
            get { return jumpToAddress; }
        }

        public string ArgText
        {
            get
            {
                return "(" + string.Join(", ", stackFrame.Arguments.ToArray().ConvertAll(arg => arg.ToString("x4"))) + ")";
            }
        }
    }
}
