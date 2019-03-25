using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.Core.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class LocalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly DebuggerService debuggerService;
        private readonly VariableViewService variableViewService;
        private readonly RoutineService routineService;

        private readonly IndexedVariableViewModel[] locals;

        private VariableViewModel[] stack;
        private VariableViewModel[] reversedStack;

        [ImportingConstructor]
        public LocalsViewModel(
            StoryService storyService,
            DebuggerService debuggerService,
            VariableViewService variableViewService,
            RoutineService routineService)
            : base("LocalsView")
        {
            this.storyService = storyService;

            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += DebuggerService_MachineDestroyed;
            this.debuggerService.StateChanged += DebuggerService_StateChanged;
            this.debuggerService.Stepped += DebuggerService_ProcessorStepped;

            this.variableViewService = variableViewService;
            variableViewService.LocalViewChanged += VariableViewService_LocalViewChanged;

            this.routineService = routineService;

            this.locals = new IndexedVariableViewModel[15];

            for (int i = 0; i < 15; i++)
            {
                this.locals[i] = new IndexedVariableViewModel(i, 0);
            }

            this.stack = new VariableViewModel[0];
            this.reversedStack = new VariableViewModel[0];

            SetVariableViewCommand = RegisterCommand<KeyValuePair<VariableViewModel, VariableView>>(
                text: "Set Variable View",
                name: "SetVariableView",
                executed: SetVariableViewExecuted,
                canExecute: CanSetVariableViewExecute);
        }

        private void VariableViewService_LocalViewChanged(object sender, LocalViewChangedArgs e)
        {
            var processor = debuggerService.Machine;
            if (storyService.IsStoryOpen && processor != null)
            {
                var routine = routineService.RoutineTable.GetByAddressWithin(processor.PC);
                if (routine.Address == e.Address)
                {
                    locals[e.Index].VariableView = e.NewView;
                }
            }
        }

        private void Update()
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                var processor = debuggerService.Machine;

                // Update locals...
                var localCount = processor.LocalCount;
                for (int i = 0; i < 15; i++)
                {
                    var local = locals[i];

                    var visible = i < localCount;
                    if (visible)
                    {
                        local.IsModified = local.Value != processor.Locals[i] && local.Visible == visible;
                        local.Value = processor.Locals[i];
                        var routine = routineService.RoutineTable.GetByAddressWithin(processor.PC);
                        var variableView = variableViewService.GetViewForLocal(routine, i);
                        local.VariableView = variableView;
                    }

                    local.Visible = visible;

                    if (!visible)
                    {
                        local.IsModified = false;
                    }
                }

                // Update stack...
                var stackValues = processor.GetStackValues();
                Array.Resize(ref stack, stackValues.Length);
                var numItems = stackValues.Length - 1;
                for (int i = numItems; i >= 0; i--)
                {
                    int index = numItems - i;
                    if (stack[index] == null)
                    {
                        var stackValue = new VariableViewModel(stackValues[i]);
                        stackValue.IsModified = true;
                        stack[index] = stackValue;
                    }
                    else
                    {
                        var stackValue = stack[index];
                        stackValue.IsModified = stackValue.Value != stackValues[i];
                        stackValue.Value = stackValues[i];
                    }
                }

                reversedStack = stack.Reverse();
                PropertyChanged("LocalStack");
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

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            Update();

            PropertyChanged("HasStory");
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                locals[i].Visible = false;
            }

            PropertyChanged("HasStory");
        }

        private bool CanSetVariableViewExecute(KeyValuePair<VariableViewModel, VariableView> parameter)
        {
            return true;
        }

        private void SetVariableViewExecuted(KeyValuePair<VariableViewModel, VariableView> parameter)
        {
            var viewModel = (IndexedVariableViewModel)parameter.Key;
            var view = parameter.Value;
            var processor = debuggerService.Machine;
            var routine = routineService.RoutineTable.GetByAddressWithin(processor.PC);
            variableViewService.SetViewForLocal(routine, viewModel.Index, view);
        }

        public IndexedVariableViewModel[] Locals
        {
            get { return locals; }
        }

        public VariableViewModel[] LocalStack
        {
            get { return reversedStack; }
        }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public ICommand SetVariableViewCommand
        {
            get; private set;
        }
    }
}
