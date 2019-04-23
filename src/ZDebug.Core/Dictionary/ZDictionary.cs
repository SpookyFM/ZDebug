using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Extensions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Dictionary
{
    public sealed class ZDictionary : IIndexedEnumerable<ZDictionaryEntry>
    {
        private readonly Story story;
        private readonly ZText ztext;
        private readonly int address;
        private readonly int entryStride;

        private readonly ReadOnlyCollection<char> wordSeparators;

        private readonly List<ZDictionaryEntry> entries;

        internal ZDictionary(Story story, ZText ztext)
        {
            this.story = story;
            this.ztext = ztext;

            this.address = Header.ReadDictionaryAddress(story.Memory);

            var reader = new MemoryReader(story.Memory, address);

            int wordSepCount = reader.NextByte();
            this.wordSeparators = reader.NextBytes(wordSepCount).ConvertAll(b => (char)b).AsReadOnly();

            int entryLength = reader.NextByte();
            int entryCount = reader.NextWord();

            int zwordsSize = story.Version <= 3 ? 2 : 3;
            int dataSize = entryLength - (zwordsSize * 2);

            this.entries = new List<ZDictionaryEntry>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                var entryAddress = reader.Address;
                var entryZWords = reader.NextWords(zwordsSize);
                var entryData = reader.NextBytes(dataSize);
                var entryZText = ztext.ZWordsAsString(entryZWords, ZTextFlags.All);
                entries.Add(new ZDictionaryEntry(entryAddress, i, entryZWords, entryZText, entryData));
            }

            entryStride = zwordsSize * 2 + dataSize;
        }

        public bool TryLookupWord(string word, out ushort address)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                if (word.StartsWith(e.ZText))
                {
                    address = (ushort)e.Address;
                    return true;
                }
            }

            address = 0;
            return false;
        }

        public int FirstEntryAddress
        {
            get
            {
                return entries[0].Address;
            }
        }

        public int EntryStride
        {
            get
            {
                return entryStride;
            }
        }

        public ZDictionaryEntry GetEntryFromAddress(ushort address)
        {
            int index = (address - FirstEntryAddress) / EntryStride;
            int remainder = (address - FirstEntryAddress) % EntryStride;
            if (remainder != 0)
            {
                return null;
            }
            if (index < 0 || index >= entries.Count)
            {
                return null;
            }
            return entries[index];
        }

        public ReadOnlyCollection<char> WordSeparators
        {
            get { return wordSeparators; }
        }

        public ZDictionaryEntry this[int index]
        {
            get { return entries[index]; }
        }

        public int Count
        {
            get { return entries.Count; }
        }

        public IEnumerator<ZDictionaryEntry> GetEnumerator()
        {
            foreach (var entry in entries)
            {
                yield return entry;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
