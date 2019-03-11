using System.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class CallStackViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly DebuggerService debuggerService;
        private readonly RoutineService routineService;
        private readonly NavigationService navigationService;

        private readonly BulkObservableCollection<StackFrameViewModel> stackFrames;

        [ImportingConstructor]
        public CallStackViewModel(
            DebuggerService debuggerService,
            RoutineService routineService,
            NavigationService navigationService)
            : base("CallStackView")
        {
            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += new System.EventHandler<MachineDestroyedEventArgs>(DebuggerService_MachineDestroyed);
            this.debuggerService.StateChanged += DebuggerService_StateChanged;
            this.debuggerService.Stepped += DebuggerService_ProcessorStepped;

            this.routineService = routineService;
            this.navigationService = navigationService;

            this.stackFrames = new BulkObservableCollection<StackFrameViewModel>();

            JumpToDisassemblyCommand = RegisterCommand<uint>(
                text: "Jump To Address",
                name: "JumpToAddress",
                executed: JumpToExecuted,
                canExecute: CanJumpToExecute);
        }

        private void Update()
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                var frames = debuggerService.Machine.GetStackFrames();

                stackFrames.BeginBulkOperation();
                try
                {
                    stackFrames.Clear();

                    for (int i = 0; i < frames.Length; i++)
                    {
                        var frame = frames[i];
                        uint jumpToAddress = 0;
                        if (i == 0)
                        {
                            jumpToAddress = (uint) debuggerService.Machine.PC;
                        } else
                        {
                            jumpToAddress = frames[i - 1].ReturnAddress;
                        }

                        stackFrames.Add(new StackFrameViewModel(frame, routineService.RoutineTable, jumpToAddress));
                    }
                }
                finally
                {
                    stackFrames.EndBulkOperation();
                }
            }
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            Update();
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
        {
            stackFrames.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            // When input is wrapped up, we need to update as if the processor had stepped.
            if ((e.OldState == DebuggerState.AwaitingInput ||
                e.OldState == DebuggerState.Running) &&
                e.NewState != DebuggerState.Unavailable)
            {
                Update();
            }
        }

        private void DebuggerService_ProcessorStepped(object sender, SteppedEventArgs e)
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                Update();
            }
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
        
        public ICommand JumpToDisassemblyCommand
        {
            get; private set;
        }

        private void JumpToExecuted(uint address)
        {
            navigationService.RequestNavigation((int) address);
        }

        private bool CanJumpToExecute(uint address)
        {
            return true;
        }

    }
}
