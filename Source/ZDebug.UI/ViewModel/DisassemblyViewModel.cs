﻿using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.UI.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyViewModel : ViewModelWithViewBase<UserControl>
    {
        private struct AddressAndIndex
        {
            public readonly int Address;
            public readonly int Index;

            public AddressAndIndex(int address, int index)
            {
                this.Address = address;
                this.Index = index;
            }
        }

        private readonly BulkObservableCollection<DisassemblyLineViewModel> lines;
        private readonly Dictionary<int, DisassemblyLineViewModel> addressToLineMap;
        private readonly List<AddressAndIndex> routineAddressAndIndexList;

        private DisassemblyLineViewModel inputLine;

        public DisassemblyViewModel()
            : base("DisassemblyView")
        {
            lines = new BulkObservableCollection<DisassemblyLineViewModel>();
            addressToLineMap = new Dictionary<int, DisassemblyLineViewModel>();
            routineAddressAndIndexList = new List<AddressAndIndex>();
        }

        private DisassemblyLineViewModel GetLineByAddress(int address)
        {
            return addressToLineMap[address];
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            var reader = e.Story.Memory.CreateReader(0);

            DisassemblyLineViewModel ipLine;

            lines.BeginBulkOperation();
            try
            {
                var routineTable = e.Story.RoutineTable;

                for (int rIndex = 0; rIndex < routineTable.Count; rIndex++)
                {
                    var routine = routineTable[rIndex];

                    if (rIndex > 0)
                    {
                        var lastRoutine = routineTable[rIndex - 1];
                        if (lastRoutine.Address + lastRoutine.Length < routine.Address)
                        {
                            var addressGapLine = new DisassemblyAddressGapLineViewModel(lastRoutine, routine);
                            lines.Add(addressGapLine);
                        }
                    }

                    var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(routine);
                    routineAddressAndIndexList.Add(new AddressAndIndex(routineHeaderLine.Address, lines.Count));
                    lines.Add(routineHeaderLine);
                    addressToLineMap.Add(routine.Address, routineHeaderLine);

                    var instructions = routine.Instructions;
                    var lastIndex = instructions.Count - 1;
                    for (int i = 0; i <= lastIndex; i++)
                    {
                        var instruction = instructions[i];
                        var instructionLine = new DisassemblyInstructionLineViewModel(instruction, i == lastIndex);

                        if (DebuggerService.BreakpointExists(instruction.Address))
                        {
                            instructionLine.HasBreakpoint = true;
                        }

                        lines.Add(instructionLine);
                        addressToLineMap.Add(instruction.Address, instructionLine);
                    }
                }

                ipLine = GetLineByAddress(e.Story.Processor.PC);
                ipLine.HasIP = true;
            }
            finally
            {
                lines.EndBulkOperation();
            }

            BringLineIntoView(ipLine);

            e.Story.Processor.Stepped += Processor_Stepped;
            e.Story.Processor.EnterStackFrame += Processor_EnterFrame;
            e.Story.Processor.ExitStackFrame += Processor_ExitFrame;
            e.Story.RoutineTable.RoutineAdded += RoutineTable_RoutineAdded;
        }

        private void BringLineIntoView(DisassemblyLineViewModel line)
        {
            var lines = this.View.FindName<ItemsControl>("lines");
            lines.BringIntoView(line);
        }

        private void Processor_Stepped(object sender, ProcessorSteppedEventArgs e)
        {
            var oldLine = GetLineByAddress(e.OldPC);
            oldLine.HasIP = false;

            if (DebuggerService.State == DebuggerState.Running ||
                DebuggerService.State == DebuggerState.AwaitingInput ||
                DebuggerService.State == DebuggerState.Done)
            {
                return;
            }

            var newLine = GetLineByAddress(e.NewPC);
            newLine.HasIP = true;

            BringLineIntoView(newLine);
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            var returnLine = GetLineByAddress(e.PreviousAddress);
            returnLine.IsNextInstruction = true;

            returnLine.ToolTip = "Execution will return here when " + e.Address.ToString("x4") + " returns.";
        }

        private void Processor_ExitFrame(object sender, StackFrameEventArgs e)
        {
            var returnLine = GetLineByAddress(e.Address);
            returnLine.IsNextInstruction = false;
            returnLine.ToolTip = null;
        }

        private void RoutineTable_RoutineAdded(object sender, RoutineAddedEventArgs e)
        {
            lines.BeginBulkOperation();
            try
            {
                // FInd routine header line that would follow this routine
                int nextRoutineIndex = -1;
                int insertionPoint = -1;
                for (int i = 0; i < routineAddressAndIndexList.Count; i++)
                {
                    var addressAndIndex = routineAddressAndIndexList[i];
                    if (addressAndIndex.Address > e.Routine.Address)
                    {
                        nextRoutineIndex = i;
                        insertionPoint = addressAndIndex.Index;
                        break;
                    }
                }

                // If no routine header found, insert at the end of the list.
                if (nextRoutineIndex == -1)
                {
                    insertionPoint = lines.Count;
                }

                var count = 0;

                // Is previous line an address gap? If so, we need to either remove it or update it in place.
                if (insertionPoint > 0)
                {
                    var addressGap = lines[insertionPoint - 1] as DisassemblyAddressGapLineViewModel;
                    if (addressGap != null)
                    {
                        var priorRoutine = addressGap.Start;
                        var nextRoutine = addressGap.End;

                        if (addressGap.StartAddress == e.Routine.Address)
                        {
                            // If the address gap starts at this routine, we need to remove it.
                            lines.RemoveAt(--insertionPoint);
                            count--;
                        }
                        else if (addressGap.StartAddress < e.Routine.Address)
                        {
                            // If the address gap starts before this routine, we need to update it in place.
                            lines[insertionPoint - 1] = new DisassemblyAddressGapLineViewModel(priorRoutine, e.Routine);
                        }

                        if (nextRoutine.Address > e.Routine.Address + e.Routine.Length - 1)
                        {
                            // If there is a gap between this routine and the next one, we need to insert an address gap.
                            var newAddressGap = new DisassemblyAddressGapLineViewModel(e.Routine, nextRoutine);
                            lines.Insert(insertionPoint, newAddressGap);
                            count++;
                        }
                    }
                }

                var instructions = e.Routine.Instructions;
                var lastIndex = instructions.Count - 1;
                for (int i = lastIndex; i >= 0; i--)
                {
                    var instruction = instructions[i];
                    var instructionLine = new DisassemblyInstructionLineViewModel(instruction, i == lastIndex);

                    if (DebuggerService.BreakpointExists(instruction.Address))
                    {
                        instructionLine.HasBreakpoint = true;
                    }

                    lines.Insert(insertionPoint, instructionLine);
                    count++;
                    addressToLineMap.Add(instruction.Address, instructionLine);
                }

                var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(e.Routine);
                lines.Insert(insertionPoint, routineHeaderLine);
                count++;
                addressToLineMap.Add(e.Routine.Address, routineHeaderLine);

                if (nextRoutineIndex >= 0)
                {
                    // fix up routine indeces...
                    for (int i = nextRoutineIndex; i < routineAddressAndIndexList.Count; i++)
                    {
                        var addressAndIndex = routineAddressAndIndexList[i];
                        routineAddressAndIndexList[i] = new AddressAndIndex(addressAndIndex.Address, addressAndIndex.Index + count);
                    }

                    routineAddressAndIndexList.Insert(nextRoutineIndex, new AddressAndIndex(e.Routine.Address, insertionPoint));
                }
                else
                {
                    routineAddressAndIndexList.Add(new AddressAndIndex(e.Routine.Address, insertionPoint));
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            lines.Clear();
            addressToLineMap.Clear();
            routineAddressAndIndexList.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.Running)
            {
                this.View.DataContext = null;
            }
            else if (e.OldState == DebuggerState.Running)
            {
                this.View.DataContext = this;
            }

            if (e.NewState == DebuggerState.Unavailable)
            {
                return;
            }

            if (e.NewState == DebuggerState.StoppedAtError)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                line.State = DisassemblyLineState.Blocked;
                line.ToolTip = new ExceptionToolTip(DebuggerService.CurrentException);
                BringLineIntoView(line);
            }
            else if (e.OldState == DebuggerState.Running && e.NewState == DebuggerState.Stopped)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.PC);
                line.HasIP = true;
                BringLineIntoView(line);
            }
            else if (e.NewState == DebuggerState.Done)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                line.State = DisassemblyLineState.Stopped;
                BringLineIntoView(line);
            }
            else if (e.NewState == DebuggerState.AwaitingInput)
            {
                inputLine = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                inputLine.State = DisassemblyLineState.Paused;
                BringLineIntoView(inputLine);
            }
            else if (e.OldState == DebuggerState.AwaitingInput)
            {
                inputLine.State = DisassemblyLineState.None;
                inputLine = null;

                var ipLine = GetLineByAddress(DebuggerService.Story.Processor.PC);
                ipLine.HasIP = true;
            }
        }

        private void DebuggerService_BreakpointRemoved(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = false;
            }
        }

        private void DebuggerService_BreakpointAdded(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = true;
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            DebuggerService.StateChanged += DebuggerService_StateChanged;

            DebuggerService.BreakpointAdded += DebuggerService_BreakpointAdded;
            DebuggerService.BreakpointRemoved += DebuggerService_BreakpointRemoved;

            var typeface = new Typeface(this.View.FontFamily, this.View.FontStyle, this.View.FontWeight, this.View.FontStretch);

            var addressText = new FormattedText("  000000: ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, this.View.FontSize, this.View.Foreground);
            this.View.Resources["addressWidth"] = new GridLength(addressText.WidthIncludingTrailingWhitespace);

            var opcodeName = new FormattedText("check_arg_count  ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, this.View.FontSize, this.View.Foreground);
            this.View.Resources["opcodeWidth"] = new GridLength(opcodeName.WidthIncludingTrailingWhitespace);

        }

        public BulkObservableCollection<DisassemblyLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
