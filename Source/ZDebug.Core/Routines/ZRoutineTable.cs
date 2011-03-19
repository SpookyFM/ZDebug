﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Routines
{
    public sealed class ZRoutineTable : IIndexedEnumerable<ZRoutine>
    {
        private readonly Story story;
        private readonly InstructionCache cache;
        private readonly IntegerMap<ZRoutine> addressToRoutineMap;
        private readonly List<int> sortedAddresses;

        public ZRoutineTable(Story story, InstructionCache cache = null)
        {
            this.story = story;
            this.cache = cache ?? new InstructionCache();
            this.addressToRoutineMap = new IntegerMap<ZRoutine>();
            this.sortedAddresses = new List<int>();

            Add(story.MainRoutineAddress, "Main");
        }

        private static bool IsAnalyzableCall(Instruction i)
        {
            return i.Opcode.IsCall &&
                i.OperandCount > 0 &&
                i.Operands[0].Kind != OperandKind.Variable;
        }

        private int UnpackCallAddress(Instruction i)
        {
            return story.UnpackRoutineAddress(i.Operands[0].Value);
        }

        public void Add(int address, string name = null)
        {
            if (Exists(address))
            {
                return;
            }

            var routine = ZRoutine.Create(address, story.Memory, cache, name);

            addressToRoutineMap.Add(address, routine);

            var index = sortedAddresses.BinarySearch(address);
            sortedAddresses.Insert(~index, address);

            var handler = RoutineAdded;
            if (handler != null)
            {
                handler(this, new ZRoutineAddedEventArgs(routine));
            }

            foreach (var i in routine.Instructions.Where(i => IsAnalyzableCall(i)))
            {
                Add(UnpackCallAddress(i));
            }
        }

        public bool Exists(int address)
        {
            return addressToRoutineMap.Contains(address);
        }

        public ZRoutine GetByAddress(int address)
        {
            return addressToRoutineMap[address];
        }

        public ZRoutine this[int index]
        {
            get { return addressToRoutineMap[sortedAddresses[index]]; }
        }

        public int Count
        {
            get { return addressToRoutineMap.Count; }
        }

        public IEnumerator<ZRoutine> GetEnumerator()
        {
            for (int i = 0; i < sortedAddresses.Count; i++)
            {
                yield return addressToRoutineMap[sortedAddresses[i]];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler<ZRoutineAddedEventArgs> RoutineAdded;
    }
}