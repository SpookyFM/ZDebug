﻿using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Inform
{
    public sealed class InformData
    {
        private readonly byte[] memory;
        private readonly MemoryMap memoryMap;
        private readonly int version;
        private readonly ZText ztext;

        public InformData(byte[] memory, MemoryMap memoryMap, ZText ztext)
        {
            this.memory = memory;
            this.memoryMap = memoryMap;
            this.version = Header.ReadInformVersionNumber(memory);
            this.ztext = ztext;
        }

        public int Version
        {
            get { return version; }
        }

        public string GetPropertyName(int propNum)
        {
            var address = memoryMap[MemoryMapRegionKind.PropertyNamesTable].Base + (propNum * 2);
            var propNamePackedAddress = memory.ReadWord(address);
            var propNameAddress = Header.UnpackStringAddress(memory, propNamePackedAddress);

            var propNameZWords = ZText.ReadZWords(memory, propNameAddress);

            return ztext.ZWordsAsString(propNameZWords, ZTextFlags.None);
        }
    }
}
