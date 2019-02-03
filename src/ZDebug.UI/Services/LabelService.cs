using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class LabelService : IService
    {
        private Dictionary<int, int> labelDictionary = new Dictionary<int, int>();
        private readonly DebuggerService debuggerService;
        private readonly RoutineService routineService;

        [ImportingConstructor]
        public LabelService(DebuggerService debuggerService, RoutineService routineService)
        {
            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.routineService = routineService;
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            // TODO: Why is this code required to be done twice, just like in the ViewModel?
            foreach (var currentRoutine in this.routineService.RoutineTable)
            {
                int[] newLabels = ReadLabels(currentRoutine.Instructions.ToArray(), currentRoutine.Address, currentRoutine.Length);
                for (int i = 0; i < newLabels.Length; i++)
                {
                    int currentLabel = newLabels[i];
                    // We set the labels to start at 1 to be consistent with TXD output
                    SetLabel(currentLabel, i + 1);
                }
            }
            this.routineService.RoutineTable.RoutineAdded += RoutineTable_RoutineAdded;
        }

        private void RoutineTable_RoutineAdded(object sender, ZRoutineAddedEventArgs e)
        {
            var addedRoutine = e.Routine;
            int[] newLabels = ReadLabels(addedRoutine.Instructions.ToArray(), addedRoutine.Address, addedRoutine.Length);
            for (int i = 0; i < newLabels.Length; i++)
            {
                int currentLabel = newLabels[i];
                // We set the labels to start at 1 to be consistent with TXD output
                SetLabel(currentLabel, i + 1);
            }
        }

        public void SetLabel(int address, int labelNumber)
        {
            labelDictionary[address] = labelNumber;
        }

        public int? GetLabel(int address)
        {
            int result = -1;
            if (!labelDictionary.TryGetValue(address, out result))
            {
                return null;
            } else
            {
                return result;
            }
        }

        private static int[] ReadLabels(Instruction[] instructions, int address, int length)
        {
            var labels = new SortedSet<int>();
            int lastAddress = address + length;

            foreach (Instruction i in instructions)
            {
                // TODO: This recreates the code below, might want to put this into the Instruction class
                // TODO: Check if jumps and branches are even allowed to be outside of the routine.
                if (i.Opcode.IsJump)
                {
                    var jumpOffset = (short)i.Operands[0].Value;
                    var jumpAddress = i.Address + i.Length + jumpOffset - 2;
                    if (jumpAddress >= address && jumpAddress <= lastAddress)
                    {
                        labels.Add(jumpAddress);
                    }
                }

                else if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var branchAddress = i.Address + +i.Length + i.Branch.Offset - 2;
                    if (branchAddress >= address && branchAddress <= lastAddress)
                    {
                        labels.Add(branchAddress);
                    }
                }
            }
            
            return labels.ToArray();
        }


    }
}
