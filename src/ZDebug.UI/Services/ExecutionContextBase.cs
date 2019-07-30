using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDebug.Core.Basics;
using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Services
{
    abstract class ExecutionContextBase : ExecutionContext
    {
        private MemoryReader reader;

        private StringBuilder result;

        private Stack<int> positionStack;

        public ExecutionContextBase(byte[] memory)
        {
            reader = new MemoryReader(memory, 0);
            result = new StringBuilder();
            positionStack = new Stack<int>();
        }

        public override ushort readWord()
        {
            return reader.NextWord();
        }

        public override void seek(ushort offset)
        {
            reader.Address = offset;
        }

        public override byte readByte()
        {
            return reader.NextByte();
        }

        public override void print(params object[] args)
        {
            if (args.Length == 2)
            {
                ushort value = (ushort)args[0];
                ushort radix = (ushort)args[1];
                result.Append(Convert.ToString(value, radix));
            } else
            {
                string value = (string)args[0];
                result.Append(value);
            }
        }

        public void printObject(ushort index)
        {
            var storyService = App.Current.GetService<StoryService>();
            var obj = storyService.Story.ObjectTable.GetByNumber(index);
            result.Append(obj.ShortName + "(" + obj.Number.ToString() + ")");
        }

        public void printPString(ushort pString)
        {
            var storyService = App.Current.GetService<StoryService>();
            int unpacked = storyService.Story.UnpackStringAddress(pString);
            var zWords = storyService.Story.ZText.ReadZWords(unpacked);
            var s = storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All);
            result.Append(s);
        }

        public void pushPosition()
        {
            positionStack.Push(reader.Address);
        }

        public void popPosition()
        {
            int popped = positionStack.Pop();
            reader.Address = popped;
        }

        public string Result
        {
            get
            {
                return result.ToString();
            }
        }
    }
}
