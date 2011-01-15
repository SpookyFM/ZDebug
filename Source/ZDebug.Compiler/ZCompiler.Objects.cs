﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        /// <summary>
        /// Calculates the address of an object given its 1-based object number.
        /// </summary>
        private ushort CalculateObjectAddress(int objNum)
        {
            return (ushort)(machine.ObjectEntriesAddress + ((objNum - 1) * machine.ObjectEntrySize));
        }

        /// <summary>
        /// Calculates the address of an object its 1-based object number and returns it on the evaluation stack.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void CalculateObjectAddress(LocalBuilder objNum = null)
        {
            if (objNum != null)
            {
                il.Emit(OpCodes.Ldloc, objNum);
            }

            // subtract 1 from object number
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);

            // multiply by the object entry size
            il.Emit(OpCodes.Ldc_I4, (int)machine.ObjectEntrySize);
            il.Emit(OpCodes.Mul);

            // add the object entries table address
            il.Emit(OpCodes.Ldc_I4, machine.ObjectEntriesAddress);
            il.Emit(OpCodes.Add);

            // finally, convert to an unsigned short
            il.Emit(OpCodes.Conv_U2);
        }

        private void ReadObjectNumber(ushort address)
        {
            if (machine.Version < 4)
            {
                ReadByte(address);
            }
            else
            {
                ReadWord(address);
            }
        }

        private void ReadObjectNumber()
        {
            if (machine.Version < 4)
            {
                ReadByte();
            }
            else
            {
                ReadWord();
            }
        }

        /// <summary>
        /// Reads the number of an object's parent, given its 1-based object number.
        /// </summary>
        private void ReadObjectParent(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectParentOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's parent, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectParent(LocalBuilder objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add parent number offset to the address on the evaluation stack.
            il.Emit(OpCodes.Ldc_I4, (int)machine.ObjectParentOffset);
            il.Emit(OpCodes.Add);

            ReadObjectNumber();
        }

        /// <summary>
        /// Reads the number of an object's sibling, given its 1-based object number.
        /// </summary>
        private void ReadObjectSibling(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectSiblingOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's sibling, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectSibling(LocalBuilder objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add sibling number offset to the address on the evaluation stack.
            il.Emit(OpCodes.Ldc_I4, (int)machine.ObjectSiblingOffset);
            il.Emit(OpCodes.Add);

            ReadObjectNumber();
        }

        /// <summary>
        /// Reads the number of an object's child, given its 1-based object number.
        /// </summary>
        private void ReadObjectChild(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectChildOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's child, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectChild(LocalBuilder objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add child number offset to the address on the evaluation stack.
            il.Emit(OpCodes.Ldc_I4, (int)machine.ObjectChildOffset);
            il.Emit(OpCodes.Add);

            ReadObjectNumber();
        }

        /// <summary>
        /// Reads the address of an object's property table, given its 1-based object number.
        /// </summary>
        private void ReadObjectPropertyTableAddress(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectPropertyTableAddressOffset);

            ReadWord(address);
        }

        /// <summary>
        /// Reads the address of an object's property, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectPropertyTableAddress(LocalBuilder objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add property table address offset to the address on the evaluation stack.
            il.Emit(OpCodes.Ldc_I4, (int)machine.ObjectPropertyTableAddressOffset);
            il.Emit(OpCodes.Add);

            ReadWord();
        }

        /// <summary>
        /// Given a property table address on the evaluation stack, puts the address of
        /// its first property on the stack.
        /// </summary>
        private void GetAddressOfFirstProperty()
        {
            using (var propAddress = localManager.AllocateTemp<ushort>())
            {
                il.Emit(OpCodes.Stloc, propAddress);

                ReadByte(propAddress); // name-length
                il.Emit(OpCodes.Conv_U2);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Ldloc, propAddress);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U2);
            }
        }

        private void FirstProperty(int objNum)
        {
            ReadObjectPropertyTableAddress(objNum);

            GetAddressOfFirstProperty();
        }

        private void FirstProperty(LocalBuilder objNum = null)
        {
            ReadObjectPropertyTableAddress(objNum);

            GetAddressOfFirstProperty();
        }

        private void NextProperty()
        {
            using (var propAddress = localManager.AllocateTemp<ushort>())
            using (var size = localManager.AllocateTemp<byte>())
            {
                // Stack should contain last property address
                il.Emit(OpCodes.Stloc, propAddress);

                // read size byte
                ReadByte(propAddress);
                il.Emit(OpCodes.Stloc, size);

                // increment propAddress
                il.Emit(OpCodes.Ldloc, propAddress);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, propAddress);

                if (machine.Version < 4)
                {
                    // size >>= 5
                    il.Emit(OpCodes.Ldloc, size);
                    il.Emit(OpCodes.Ldc_I4_5);
                    il.Emit(OpCodes.Shr);
                    il.Emit(OpCodes.Conv_U1);
                    il.Emit(OpCodes.Stloc, size);
                }
                else
                {
                    // if ((size & 0x80) != 0x80)
                    il.Emit(OpCodes.Ldloc, size);
                    il.Emit(OpCodes.Ldc_I4, 0x80);
                    il.Emit(OpCodes.And);
                    il.Emit(OpCodes.Ldc_I4, 0x80);

                    var secondSizeByte = il.DefineLabel();
                    il.Emit(OpCodes.Beq_S, secondSizeByte);

                    // size >>= 6
                    il.Emit(OpCodes.Ldloc, size);
                    il.Emit(OpCodes.Ldc_I4_6);
                    il.Emit(OpCodes.Shr);
                    il.Emit(OpCodes.Conv_U1);
                    il.Emit(OpCodes.Stloc, size);

                    var done = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, done);

                    il.MarkLabel(secondSizeByte);

                    // read second size byte
                    ReadByte(propAddress);
                    il.Emit(OpCodes.Stloc, size);

                    // size &= 0x3f
                    il.Emit(OpCodes.Ldloc, size);
                    il.Emit(OpCodes.Ldc_I4_S, 0x3f);
                    il.Emit(OpCodes.And);
                    il.Emit(OpCodes.Conv_U1);
                    il.Emit(OpCodes.Stloc, size);

                    // if (size == 0)
                    il.Emit(OpCodes.Ldloc, size);
                    il.Emit(OpCodes.Brtrue_S, done);

                    // size = 64
                    il.Emit(OpCodes.Ldc_I4_S, 64);
                    il.Emit(OpCodes.Stloc, size);

                    il.MarkLabel(done);
                }

                // (ushort)(propAddress + size + 1)
                il.Emit(OpCodes.Ldloc, propAddress);
                il.Emit(OpCodes.Ldloc, size);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U2);
            }
        }
    }
}
