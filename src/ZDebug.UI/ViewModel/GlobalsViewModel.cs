using System.Collections.Generic;
using System.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class GlobalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly DebuggerService debuggerService;
        private readonly VariableViewService variableViewService;

        private readonly IndexedVariableViewModel[] globals;

        [ImportingConstructor]
        public GlobalsViewModel(
            StoryService storyService,
            DebuggerService debuggerService,
            VariableViewService variableViewService)
            : base("GlobalsView")
        {
            this.storyService = storyService;

            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += DebuggerService_MachineDestroyed;
            this.debuggerService.StateChanged += DebuggerService_StateChanged;
            this.debuggerService.Stepped += DebuggerService_ProcessorStepped;

            this.variableViewService = variableViewService;
            variableViewService.GlobalViewChanged += VariableViewService_GlobalViewChanged;

            this.globals = new IndexedVariableViewModel[240];

            for (int i = 0; i < 240; i++)
            {
                var newGlobal = new IndexedVariableViewModel(i, 0);
                newGlobal.Visible = false;
                globals[i] = newGlobal;
            }

            SetVariableViewCommand = RegisterCommand<KeyValuePair<VariableViewModel, VariableView>>(
                text: "Set Variable View",
                name: "SetVariableView",
                executed: SetVariableViewExecuted,
                canExecute: CanSetVariableViewExecute);
        }

        private void VariableViewService_GlobalViewChanged(object sender, GlobalViewChangedArgs e)
        {
            globals[e.Index].VariableView = e.NewView;
        }

        private void Update(bool storyOpened = false)
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                var story = storyService.Story;

                // Update globals...
                for (int i = 0; i < 240; i++)
                {
                    var global = globals[i];

                    var newGlobalValue = story.GlobalVariablesTable[i];
                    global.IsModified = !storyOpened && global.Value != newGlobalValue;
                    global.Value = newGlobalValue;
                    var variableView = variableViewService.GetViewForGlobal(i);
                    global.VariableView = variableView;
                }
            }
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = true;
            }

            Update(storyOpened: true);
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
        {
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = false;
            }
        }

        private void DebuggerService_ProcessorStepped(object sender, SteppedEventArgs e)
        {
            Update();
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

        private bool CanSetVariableViewExecute(KeyValuePair<VariableViewModel, VariableView> parameter)
        {
            return true;
        }

        private void SetVariableViewExecuted(KeyValuePair<VariableViewModel, VariableView> parameter)
        {
            var viewModel = (IndexedVariableViewModel)parameter.Key;
            var view = parameter.Value;
            variableViewService.SetViewForGlobal(viewModel.Index, view);
        }

        public IndexedVariableViewModel[] Globals
        {
            get { return globals; }
        }

        public ICommand SetVariableViewCommand
        {
            get; private set;
        }
    }
}
