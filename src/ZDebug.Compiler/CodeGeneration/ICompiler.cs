﻿using ZDebug.Compiler.Generate;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal interface ICompiler
    {
        /// <summary>
        /// Retrieves a label for the given Z-code address.
        /// </summary>
        ILabel GetLabel(int address);

        /// <summary>
        /// Emits code to loads an operand onto the evaluation stack.
        /// </summary>
        /// <param name="operand">The operand to load onto the evaluation stack.</param>
        void EmitLoadOperand(Operand operand, bool convertResult = true);

        /// <summary>
        /// Emits code to execute the given branch using the value on top of the evaluation stack.
        /// </summary>
        void EmitBranch(Branch branch);

        /// <summary>
        /// Emits code to return from the current routine.
        /// </summary>
        void EmitReturn();

        /// <summary>
        /// Emits code to call a routine.
        /// </summary>
        void EmitCall(Operand address, ReadOnlyArray<Operand> args, bool reuse = false);

        /// <summary>
        /// Emits code to load a byte from Z-machine memory at the given address.
        /// </summary>
        void EmitLoadMemoryByte(int address);

        /// <summary>
        /// Emits code to load a byte from Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitLoadMemoryByte(ILocal address);

        /// <summary>
        /// Emits code to load a word from Z-machine memory at the given address.
        /// </summary>
        void EmitLoadMemoryWord(int address, bool convertResult = true);

        /// <summary>
        /// Emits code to load a word from Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitLoadMemoryWord(ILocal address, bool convertResult = true);

        /// <summary>
        /// Emits code to store an IL local as a byte in Z-machine memory at the given address.
        /// </summary>
        void EmitStoreMemoryByte(int address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a byte in Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitStoreMemoryByte(ILocal address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a word in Z-machine memory at the given address.
        /// </summary>
        void EmitStoreMemoryWord(int address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a word in Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitStoreMemoryWord(ILocal address, ILocal value);

        /// <summary>
        /// Emits code to pop the Z-machine stack.
        /// </summary>
        void EmitPopStack(bool indirect = false);

        /// <summary>
        /// Emits code to push an IL local onto the Z-machine stack.
        /// </summary>
        void EmitPushStack(ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to load the Z-machine variable at the given index.
        /// </summary>
        void EmitLoadVariable(byte variableIndex, bool indirect = false, bool convertResult = true);

        /// <summary>
        /// Emits code to load the Z-machine variable at the index stored in the given IL local.
        /// </summary>
        void EmitLoadVariable(ILocal variableIndex, bool indirect = false, bool convertResult = true);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine variable at the given index.
        /// </summary>
        void EmitStoreVariable(byte variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the specified Z-machine variable.
        /// </summary>
        void EmitStoreVariable(Variable variable, ILocal value, bool indirect = false, bool reuse = false);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine variable at the index stored in the given IL local.
        /// </summary>
        void EmitStoreVariable(ILocal variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to load the unpacked string address in the specified operand onto the evaluation stack.
        /// </summary>
        void EmitLoadUnpackedStringAddress(Operand operand);

        /// <summary>
        /// Emits code to load the object number from the specified operand value and branch to the given label
        /// if it's invalid.
        /// </summary>
        void EmitLoadValidObject(Operand operand, ILabel invalidObject, bool reuse = false);

        /// <summary>
        /// Emits code to load the Z-Words for the given object's short name onto the evaluation stack.
        /// </summary>
        void EmitLoadObjectShortName(Operand operand, bool reuse = false);

        /// <summary>
        /// Emits code to load the object parent of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitLoadObjectParent(Operand operand, bool reuse = false);

        /// <summary>
        /// Emits code to load the object sibling of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitLoadObjectSibling(Operand operand, bool reuse = false);

        /// <summary>
        /// Emits code to load the object child of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitLoadObjectChild(Operand operand, bool reuse = false);

        /// <summary>
        /// Emits code to test whether the specified object has the given attribute.
        /// </summary>
        void EmitObjectHasAttribute(ILocal objectNumber, ILocal attributeNumber);

        /// <summary>
        /// Emits code to set or clear the given attribute on the specified object.
        /// </summary>
        void EmitObjectChangeAttribute(ILocal objectNumber, ILocal attributeNumber, bool value);

        /// <summary>
        /// Emits code to load the first property address of the specified object onto the evaluation stack.
        /// </summary>
        void EmitLoadFirstPropertyAddress(ILocal objectNumber);

        /// <summary>
        /// Emits code to load the next property address onto the evaluation stack.
        /// </summary>
        void EmitLoadNextPropertyAddress();

        /// <summary>
        /// Emits code to load the address of the specified property default onto the evaluation stack.
        /// </summary>
        void EmitLoadDefaultPropertyAddress(ILocal propertyNumber);

        /// <summary>
        /// Emits code to remove the specified object from its parent.
        /// </summary>
        void EmitObjectRemoveFromParent(ILocal objectNumber);

        /// <summary>
        /// Emits code to move the specified object to the given destination.
        /// </summary>
        void EmitObjectMoveToDestination(ILocal objectNumber, ILocal destinationNumber);

        /// <summary>
        /// Emits code to show status line.
        /// </summary>
        void EmitShowStatus();

        /// <summary>
        /// Emits code to set the current text style to the specified operand.
        /// </summary>
        void EmitSetTextStyle(Operand op);

        /// <summary>
        /// Emits code to split the window using the specified operand.
        /// </summary>
        void EmitSplitWindow(Operand op);

        /// <summary>
        /// Emits code to set the window using the specified operand.
        /// </summary>
        void EmitSetWindow(Operand op);

        /// <summary>
        /// Emits code to erase the window using the specified operand.
        /// </summary>
        void EmitEraseWindow(Operand op);

        /// <summary>
        /// Emits code to set the cursor to the specified line and column.
        /// </summary>
        void EmitSetCursor(ILocal line, ILocal column);

        /// <summary>
        /// Emits code to set the color to the specified foreground and background.
        /// </summary>
        void EmitSetColor(ILocal foreground, ILocal background);

        /// <summary>
        /// Emits code to select the screen stream.
        /// </summary>
        void EmitSelectScreenStream();

        /// <summary>
        /// Emits code to deselect the screen stream.
        /// </summary>
        void EmitDeselectScreenStream();

        /// <summary>
        /// Emits code to select the transcript stream.
        /// </summary>
        void EmitSelectTranscriptStream();

        /// <summary>
        /// Emits code to deselect the transcript stream.
        /// </summary>
        void EmitDeselectTranscriptStream();

        /// <summary>
        /// Emits code to select a memory stream using the specified operand.
        /// </summary>
        void EmitSelectMemoryStream(Operand operand);

        /// <summary>
        /// Emits code to deselect the last memory stream.
        /// </summary>
        void EmitDeselectMemoryStream();

        /// <summary>
        /// Emits code to print the specified Z-Words to the screen.
        /// </summary>
        void EmitPrintZWords(ushort[] zwords);

        /// <summary>
        /// Emits code to print the text on the evaluation stack to the screen.
        /// </summary>
        void EmitPrintText();

        /// <summary>
        /// Emits code to print the char on the evaluation stack to the screen.
        /// </summary>
        void EmitPrintChar();

        /// <summary>
        /// Emits code to print the specified char to the screen.
        /// </summary>
        void EmitPrintChar(char ch);

        /// <summary>
        /// Emits code to quit the Z-Machine.
        /// </summary>
        void Quit();

        /// <summary>
        /// Returns the Z-Machine version.
        /// </summary>
        byte Version { get; }
    }
}
