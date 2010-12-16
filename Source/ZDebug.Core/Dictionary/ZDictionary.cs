﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Dictionary
{
    public sealed class ZDictionary : IIndexedEnumerable<ZDictionaryEntry>
    {
        private readonly Story story;
        private readonly int address;

        private readonly ReadOnlyCollection<char> wordSeparators;

        private readonly List<ZDictionaryEntry> entries;

        internal ZDictionary(Story story)
        {
            this.story = story;

            var memory = story.Memory;
            this.address = memory.ReadDictionaryAddress();

            var reader = memory.CreateReader(address);

            int wordSepCount = reader.NextByte();
            this.wordSeparators = reader.NextBytes(wordSepCount).ConvertAll(b => (char)b).AsReadOnly();

            int entryLength = reader.NextByte();
            int entryCount = reader.NextWord();

            int zwordsSize = story.Version <= 3 ? 2 : 3;
            int dataSize = entryLength - zwordsSize;

            this.entries = new List<ZDictionaryEntry>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                var entryAddress = reader.Address;
                var entryZWords = reader.NextWords(zwordsSize);
                var entryData = reader.NextBytes(dataSize);
                entries.Add(new ZDictionaryEntry(entryAddress, i, entryZWords, entryData));
            }
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