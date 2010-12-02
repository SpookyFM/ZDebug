﻿using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class Processor : IExecutionContext
    {
        private readonly Story story;
        private readonly IMemoryReader reader;
        private readonly InstructionReader instructions;
        private readonly Stack<StackFrame> callStack;

        private Instruction executingInstruction;

        internal Processor(Story story)
        {
            this.story = story;

            this.callStack = new Stack<StackFrame>();

            // create "call" to main routine
            var mainRoutineAddress = story.Memory.ReadMainRoutineAddress();
            this.reader = story.Memory.CreateReader(mainRoutineAddress);
            this.instructions = reader.AsInstructionReader(story.Version);

            var localCount = reader.NextByte();
            var locals = ArrayEx.Create(localCount, i => Value.Zero);

            callStack.Push(
                new StackFrame(
                    mainRoutineAddress,
                    arguments: ArrayEx.Empty<Value>(),
                    locals: locals,
                    returnAddress: null,
                    storeVariable: null));
        }

        private StackFrame CurrentFrame
        {
            get { return callStack.Peek(); }
        }

        private Value ReadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        return CurrentFrame.PeekValue();
                    }
                    else
                    {
                        return CurrentFrame.PopValue();
                    }

                case VariableKind.Local:
                    return CurrentFrame.Locals[variable.Index];

                case VariableKind.Global:
                    return story.GlobalVariablesTable[variable.Index];

                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteVariable(Variable variable, Value value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        CurrentFrame.PopValue();
                    }

                    CurrentFrame.PushValue(value);
                    break;

                case VariableKind.Local:
                    CurrentFrame.SetLocal(variable.Index, value);
                    break;

                case VariableKind.Global:
                    story.GlobalVariablesTable[variable.Index] = value;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private Value GetOperandValue(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                    return operand.AsLargeConstant();
                case OperandKind.SmallConstant:
                    return operand.AsSmallConstant();
                case OperandKind.Variable:
                    return ReadVariable(operand.AsVariable());
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteStoreVariable(Variable storeVariable, Value value)
        {
            if (storeVariable != null)
            {
                WriteVariable(storeVariable, value);
            }
        }

        private void Call(int address, Operand[] operands, Variable storeVariable)
        {
            if (address < 0)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            // NOTE: argument values must be retrieved in case they manipulate the stack
            var argValues = operands.Select(GetOperandValue);

            if (address == 0)
            {
                // SPECIAL CASE: A routine call to packed address 0 is legal: it does nothing and returns false (0). Otherwise it is
                // illegal to call a packed address where no routine is present.

                // If there is a store variable, write 0 to it.
                WriteStoreVariable(storeVariable, Value.Zero);
            }
            else
            {
                story.RoutineTable.Add(address);

                var returnAddress = reader.Address;
                reader.Address = address;

                // read locals
                var localCount = reader.NextByte();
                var locals = story.Version <= 4
                    ? ArrayEx.Create(localCount, _ => Value.Number(reader.NextWord()))
                    : ArrayEx.Create(localCount, _ => Value.Zero);

                var numberToCopy = Math.Min(argValues.Length, locals.Length);
                Array.Copy(argValues, 0, locals, 0, numberToCopy);

                var newFrame = new StackFrame(address, argValues, locals, returnAddress, storeVariable);

                callStack.Push(newFrame);
            }
        }

        public void Step()
        {
            var oldPC = reader.Address;
            executingInstruction = instructions.NextInstruction();

            var steppingHandler = Stepping;
            if (steppingHandler != null)
            {
                steppingHandler(this, new ProcessorSteppingEventArgs(oldPC));
            }

            executingInstruction.Opcode.Execute(executingInstruction, this);

            var newPC = reader.Address;

            var steppedHandler = Stepped;
            if (steppedHandler != null)
            {
                steppedHandler(this, new ProcessorSteppedEventArgs(oldPC, newPC));
            }

            executingInstruction = null;
        }

        public int PC
        {
            get { return reader.Address; }
        }

        /// <summary>
        /// The Instruction that is being executed (only valid during a step).
        /// </summary>
        public Instruction ExecutingInstruction
        {
            get { return executingInstruction; }
        }

        public event EventHandler<ProcessorSteppingEventArgs> Stepping;
        public event EventHandler<ProcessorSteppedEventArgs> Stepped;

        Value IExecutionContext.GetOperandValue(Operand operand)
        {
            return GetOperandValue(operand);
        }

        void IExecutionContext.Call(int address, Operand[] operands, Variable storeVariable)
        {
            Call(address, operands, storeVariable);
        }

        int IExecutionContext.UnpackRoutineAddress(ushort byteAddress)
        {
            return story.UnpackRoutineAddress(byteAddress);
        }
    }
}
