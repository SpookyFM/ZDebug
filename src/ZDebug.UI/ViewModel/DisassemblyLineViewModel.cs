﻿namespace ZDebug.UI.ViewModel
{
    internal abstract partial class DisassemblyLineViewModel : ViewModelBase
    {
        private bool hasBreakpoint;
        private bool hasIP;
        private bool isOnStack;
        private bool showBlankBefore;
        private bool showBlankAfter;
        private DisassemblyLineState state;
        private object toolTip;

        public bool HasBreakpoint
        {
            get { return hasBreakpoint; }
            set
            {
                if (hasBreakpoint != value)
                {
                    hasBreakpoint = value;
                    PropertyChanged("HasBreakpoint");
                }
            }
        }

        public bool HasIP
        {
            get { return hasIP; }
            set
            {
                if (hasIP != value)
                {
                    hasIP = value;
                    PropertyChanged("HasIP");
                }
            }
        }

        public bool IsOnStack
        {
            get { return isOnStack; }
            set
            {
                if (isOnStack != value)
                {
                    isOnStack = value;
                    PropertyChanged("IsOnStack");
                }
            }
        }

        public DisassemblyLineState State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    PropertyChanged("State");
                }
            }
        }

        public object ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    PropertyChanged("ToolTip");
                }
            }
        }

        public bool ShowBlankBefore
        {
            get { return showBlankBefore; }
            set
            {
                if (showBlankBefore != value)
                {
                    showBlankBefore = value;
                    PropertyChanged("ShowBlankBefore");
                }
            }
        }

        public bool ShowBlankAfter
        {
            get { return showBlankAfter; }
            set
            {
                if (showBlankAfter != value)
                {
                    showBlankAfter = value;
                    PropertyChanged("ShowBlankAfter");
                }
            }
        }
    }
}
