using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Dictionary;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DictionaryEntryViewModel : ViewModelBase
    {
        private readonly ZDictionaryEntry entry;

        public DictionaryEntryViewModel(ZDictionaryEntry entry)
        {
            this.entry = entry;
        }

        public int Index
        {
            get { return entry.Index; }
        }

        public string ZText
        {
            get { return entry.ZText; }
        }

        public byte[] Data
        {
            get { return entry.Data; }
        }

        public string DataString
        {
            get
            {
                var byteStrings = Array.ConvertAll(Data, b => b.ToString("x2"));
                return string.Join(" ", byteStrings);
            }
        }

        public bool PassesTest(BitmapTest test)
        {
            byte targetByte = entry.Data[test.ByteToApplyTo];
            byte bitmap = test.Bitmap;
            bool result = (bitmap & targetByte) == bitmap;
            return true;
            // return result;
            // return entry.Data[test.ByteToApplyTo] == bitmap;
        }
    }
}
