﻿using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private const int stackSize = 16384;

        private readonly Story story;
        private readonly Memory memory;
        private readonly byte version;
        private readonly byte[] bytes;
        private readonly ZText ztext;
        private readonly Opcode[] opcodes;

        private int pc;

        private readonly ZObjectTable objectTable;
        private readonly ushort globalVariableTableAddress;

        // stack and routine call state
        private readonly int[] stack = new int[stackSize];
        private int stackPointer = -1;
        private int callFrame = -1;
        private readonly int[] callFrames = new int[stackSize];
        private int callFramePointer = -1;
        private readonly ushort[] locals = new ushort[15];
        private int localCount;
        private int argumentCount;
        private bool hasCallStoreVariable;
        private byte callStoreVariable;

        private readonly ushort[] empty = new ushort[0];

        private readonly OutputStreams outputStreams;
        private Random random = new Random();
        private IScreen screen;
        private IMessageLog messageLog;

        private int instructionCount;
        private int callCount;

        internal Processor(Story story, ZText ztext)
        {
            this.story = story;
            this.memory = story.Memory;
            this.version = story.Version;
            this.bytes = this.memory.Bytes;
            this.ztext = ztext;
            this.objectTable = story.ObjectTable;
            this.globalVariableTableAddress = this.memory.ReadGlobalVariableTableAddress();

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            this.pc = this.memory.ReadMainRoutineAddress();
            this.opcodes = OpcodeTables.GetOpcodeTable(this.version).opcodes;

            this.localCount = memory.ReadByte(ref pc);
        }

        /// <summary>
        /// Push values of current frame to the stack. They are pushed in the following order:
        /// 
        /// * argument count
        /// * local variable values (in reverse order)
        /// * local variable count
        /// * return address
        /// * store variable (encoded as byte; -1 for no variable)
        /// </summary>
        private void PushFrame()
        {
            callFrames[++callFramePointer] = callFrame;
            stack[++stackPointer] = argumentCount;

            for (int i = localCount - 1; i >= 0; i--)
            {
                stack[++stackPointer] = locals[i];
            }

            stack[++stackPointer] = localCount;
            stack[++stackPointer] = pc;
            stack[++stackPointer] = hasCallStoreVariable ? callStoreVariable : -1;
            callFrame = stackPointer;
        }

        /// <summary>
        /// Pop values from the stack to set up the current frame. They are popped in the following order:
        /// 
        /// * store variable (encoded as byte; -1 for no variable)
        /// * return address
        /// * local variable count
        /// * local variable values
        /// * argument count
        /// </summary>
        private void PopFrame()
        {
            // First, throw away any existing local stack
            stackPointer = callFrame;

            var variableIndex = stack[stackPointer--];
            hasCallStoreVariable = variableIndex >= 0;
            if (hasCallStoreVariable)
            {
                callStoreVariable = (byte)variableIndex;
            }

            pc = stack[stackPointer--];
            localCount = stack[stackPointer--];

            for (int i = 0; i < localCount; i++)
            {
                locals[i] = (ushort)stack[stackPointer--];
            }

            argumentCount = stack[stackPointer--];
            callFrame = callFrames[callFramePointer--];
        }

        private ushort ReadVariableValue(byte variableIndex)
        {
            if (variableIndex < 16)
            {
                if (variableIndex > 0)
                {
                    return locals[variableIndex - 0x01];
                }
                else
                {
                    if (stackPointer == callFrame)
                    {
                        throw new InvalidOperationException("Local stack is empty.");
                    }

                    return (ushort)stack[stackPointer--];
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return bytes.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private ushort ReadVariableValueIndirectly(byte variableIndex)
        {
            if (variableIndex < 16)
            {
                if (variableIndex > 0)
                {
                    return locals[variableIndex - 0x01];
                }
                else
                {
                    if (stackPointer == callFrame)
                    {
                        throw new InvalidOperationException("Local stack is empty.");
                    }

                    return (ushort)stack[stackPointer];
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return bytes.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private void WriteVariableValue(byte variableIndex, ushort value)
        {
            if (variableIndex == 0x00) // stack
            {
                stack[++stackPointer] = value;
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                locals[variableIndex - 0x01] = value;
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var address = globalVariableTableAddress + ((variableIndex - 0x10) * 2);
                bytes.WriteWord(address, value);
            }
        }

        private void WriteVariableValueIndirectly(byte variableIndex, ushort value)
        {
            if (variableIndex == 0x00) // stack
            {
                if (stackPointer == callFrame)
                {
                    throw new InvalidOperationException("Stack is empty.");
                }

                stack[stackPointer] = value;
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                locals[variableIndex - 0x01] = value;
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var address = globalVariableTableAddress + ((variableIndex - 0x10) * 2);
                bytes.WriteWord(address, value);
            }
        }

        private void Call(int address, short storeVarIndex = -1)
        {
            if (address < 0)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            if (address == 0)
            {
                // SPECIAL CASE: A routine call to packed address 0 is legal: it does nothing and returns false (0). Otherwise it is
                // illegal to call a packed address where no routine is present.

                // If there is a store variable, write 0 to it.
                if (storeVarIndex >= 0)
                {
                    WriteVariableValue((byte)storeVarIndex, 0);
                }
            }
            else
            {
                PushFrame();

                var returnAddress = pc;
                pc = address;

                argumentCount = operandCount - 1;

                // read locals
                localCount = bytes[pc++];
                if (version <= 4)
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = memory.ReadWord(ref pc);
                    }
                }
                else
                {
                    Array.Clear(locals, 0, localCount);
                }

                var numberToCopy = Math.Min(argumentCount, locals.Length);
                Array.Copy(operandValues, 1, locals, 0, numberToCopy);

                hasCallStoreVariable = storeVarIndex >= 0;
                if (hasCallStoreVariable)
                {
                    callStoreVariable = (byte)storeVarIndex;
                }

                var handler = EnterStackFrame;
                if (handler != null)
                {
                    handler(this, new StackFrameEventArgs(address, returnAddress));
                }
            }

            callCount++;
        }

        private void Return(ushort value)
        {
            var hasStoreVar = hasCallStoreVariable;
            var storeVar = callStoreVariable;
            var oldAddress = pc;

            PopFrame();

            if (hasStoreVar)
            {
                WriteVariableValue(storeVar, value);
            }

            var handler = ExitStackFrame;
            if (handler != null)
            {
                handler(this, new StackFrameEventArgs(pc, oldAddress));
            }
        }

        private void Jump(short offset)
        {
            pc += offset - 2;
        }

        private void Branch(bool condition)
        {
            /* Instructions which test a condition are called "branch" instructions. The branch information is
             * stored in one or two bytes, indicating what to do with the result of the test. If bit 7 of the first
             * byte is 0, a branch occurs when the condition was false; if 1, then branch is on true. If bit 6 is set,
             * then the branch occupies 1 byte only, and the "offset" is in the range 0 to 63, given in the bottom
             * 6 bits. If bit 6 is clear, then the offset is a signed 14-bit number given in bits 0 to 5 of the first
             * byte followed by all 8 of the second. */

            byte specifier = bytes[pc++];

            byte offset1 = (byte)(specifier & 0x3f);

            if (!condition)
            {
                specifier ^= 0x80;
            }

            ushort offset;
            if ((specifier & 0x40) == 0) // long branch
            {
                if ((offset1 & 0x20) != 0) // propogate sign bit
                {
                    offset1 |= 0xc0;
                }

                byte offset2 = bytes[pc++];

                offset = (ushort)((offset1 << 8) | offset2);
            }
            else // short branchOffset
            {
                offset = offset1;
            }

            if ((specifier & 0x80) != 0)
            {
                if (offset > 1)
                {
                    pc += (short)offset - 2;
                }
                else
                {
                    Return(offset);
                }
            }
        }

        private void Store(ushort value)
        {
            var storeVariable = bytes[pc++];

            WriteVariableValue(storeVariable, value);
        }

        private string DecodeEmbeddedText()
        {
            int count = 0;
            while (true)
            {
                var zword = bytes.ReadWord(pc + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            var zwords = memory.ReadWords(ref pc, count);

            return ztext.ZWordsAsString(zwords, ZTextFlags.All);
        }

        private void WriteProperty(int objNum, int propNum, ushort value)
        {
            var obj = objectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            if (prop.DataLength == 2)
            {
                story.Memory.WriteWord(prop.DataAddress, value);
            }
            else if (prop.DataLength == 1)
            {
                story.Memory.WriteByte(prop.DataAddress, (byte)(value & 0x00ff));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public int Step()
        {
            ReadNextInstruction();

            Execute();

            instructionCount++;

            return pc;
        }

        private void SetScreenDimensions()
        {
            if (story.Version >= 4)
            {
                story.Memory.WriteScreenHeightInLines(screen.ScreenHeightInLines);
                story.Memory.WriteScreenWidthInColumns(screen.ScreenWidthInColumns);
            }

            if (story.Version >= 5)
            {
                story.Memory.WriteScreenHeightInUnits(screen.ScreenHeightInUnits);
                story.Memory.WriteScreenWidthInUnits(screen.ScreenWidthInUnits);
                story.Memory.WriteFontHeightInUnits(screen.FontHeightInUnits);
                story.Memory.WriteFontWidthInUnits(screen.FontWidthInUnits);
            }
        }

        public void RegisterScreen(IScreen screen)
        {
            if (screen == null)
            {
                throw new ArgumentNullException("screen");
            }

            this.screen = screen;
            SetScreenDimensions();

            if (story.Version >= 5)
            {
                var flags1 = story.Memory.ReadByte(0x01);
                flags1 = screen.SupportsColors
                    ? (byte)(flags1 | 0x01)
                    : (byte)(flags1 & ~0x01);
                story.Memory.WriteByte(0x01, flags1);

                story.Memory.WriteByte(0x2c, (byte)screen.DefaultBackgroundColor);
                story.Memory.WriteByte(0x2d, (byte)screen.DefaultForegroundColor);
            }

            if (story.Version >= 4)
            {
                var flags1 = story.Memory.ReadByte(0x01);

                flags1 = screen.SupportsBold
                    ? (byte)(flags1 | 0x04)
                    : (byte)(flags1 & ~0x04);

                flags1 = screen.SupportsItalic
                    ? (byte)(flags1 | 0x08)
                    : (byte)(flags1 & ~0x08);

                flags1 = screen.SupportsFixedFont
                    ? (byte)(flags1 | 0x10)
                    : (byte)(flags1 & ~0x10);

                story.Memory.WriteByte(0x01, flags1);
            }

            outputStreams.RegisterScreen(screen);
        }

        public void RegisterMessageLog(IMessageLog messageLog)
        {
            if (messageLog == null)
            {
                throw new ArgumentNullException("messageLog");
            }

            this.messageLog = messageLog;
        }

        public void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }

        public int PC
        {
            get { return pc; }
        }

        public ushort[] Locals
        {
            get { return locals; }
        }

        public int LocalCount
        {
            get { return localCount; }
        }

        public ushort GetOperandValue(int index)
        {
            return operandValues[index];
        }

        public ushort[] GetStackValues()
        {
            var localStackSize = stackPointer - callFrame;
            if (localStackSize == 0)
            {
                return empty;
            }

            var result = new ushort[localStackSize];

            for (int i = localStackSize - 1; i >= 0; i--)
            {
                result[i] = (ushort)stack[stackPointer - i];
            }

            return result;
        }

        public int InstructionCount
        {
            get { return instructionCount; }
        }

        public int CallCount
        {
            get { return callCount; }
        }

        /// <summary>
        /// The address of the instruction that is being executed (only valid during a step).
        /// </summary>
        public int ExecutingAddress
        {
            get { return startAddress; }
        }

        public event EventHandler<StackFrameEventArgs> EnterStackFrame;
        public event EventHandler<StackFrameEventArgs> ExitStackFrame;

        public event EventHandler Quit;
    }
}
