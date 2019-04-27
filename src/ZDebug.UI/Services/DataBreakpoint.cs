using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZDebug.UI.Services
{
    
    class DataBreakpoint: IComparable<DataBreakpoint>
    {
        int address;
        int length;
        byte[] previousValue;

        public DataBreakpoint(int address, int length)
        {
            this.address = address;
            this.length = length;
        }

        public int Address
        {
            get
            {
                return address;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }


        // Updates the data breakpoint and returns true if the value changed
        public bool UpdateFromMemory(byte[] memory)
        {
            byte[] temp = new byte[length];
            Array.Copy(memory, address, temp, 0, length);
            if (previousValue == null)
            {
                previousValue = temp;
                return false;
            }
            bool equals = ((IStructuralEquatable)previousValue).Equals(temp, StructuralComparisons.StructuralEqualityComparer);
            if (!equals)
            {
                previousValue = temp;
            }
            return !equals;
        }

        public int CompareTo(DataBreakpoint other)
        {
            return this.address.CompareTo(other.address);
        }
    }
}
