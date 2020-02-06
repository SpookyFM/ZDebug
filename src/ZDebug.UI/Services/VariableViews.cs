using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ZDebug.Core.Basics;
using ZDebug.UI.ViewModel;
using ZDebug.UI.Visualizers.Services;

namespace ZDebug.UI.Services
{
    internal sealed class VariableViews
    {
        public static VariableView HexadecimalView { get; }
        public static VariableView DecimalView { get; }
        public static VariableView BooleanView { get; }
        public static VariableView StringView { get; }
        public static VariableView PackedStringView { get; }
        public static VariableView ObjectViewAddress { get; }
        public static VariableView ObjectViewNumber { get; }
        public static VariableView FunctionView { get; }
        public static VariableView ParseEntryView { get; }
        public static VariableView ParseBufferView { get; }
        public static VariableView TextBufferView { get; }
        public static VariableView DictionaryEntryView { get; }
        public static VariableView TableView { get; }
        public static VariableView WorldListView { get; }
        public static VariableView InterruptView { get; }

        static VariableViews()
        {
            var mainWindowViewModel = (MainWindowViewModel)App.Current.MainWindow.DataContext;
            HexadecimalView = new VariableView("hex", "Hexadecimal view", (value, memory) =>
            {
                return String.Format("{0:x4}", value);
            });

            DecimalView = new VariableView("dec", "Decimal view", (value, memory) =>
            {
                return value.ToString();
            });

            BooleanView = new VariableView("bool", "Boolean view", (value, memory) =>
            {
                return value > 0 ? "true" : "false";
            });

            // TODO: Also needs a direct string value
            StringView = new VariableView("str", "String view", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                var zWords = storyService.Story.ZText.ReadZWords(value);
                return storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All);
            });

            PackedStringView = new VariableView("strp", "String view (packed)", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                var zWords = storyService.Story.ZText.ReadZWords(Header.UnpackStringAddress(memory, value));
                return storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All);
            });

            ObjectViewAddress = new VariableView("obja", "Object view (address)", (value, memory) =>
            {
                // TODO: Does this one work?
                var storyService = App.Current.GetService<StoryService>();
                var zObject = storyService.Story.ObjectTable.GetObjectAtAddress(value);
                if (zObject != null)
                {
                    return zObject.Number.ToString() + ": " + zObject.ShortName;
                }
                else
                {
                    return "-no object-";
                }

            });

            ObjectViewNumber = new VariableView("obj", "Object view", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                if (storyService == null)
                {
                    return "----";
                }
                if (!storyService.IsStoryOpen)
                {
                    return "----";
                }
                var objectTable = storyService.Story.ObjectTable;
                if (value >= objectTable.Count)
                {
                    return "-no object-";
                }
                if (value == 0)
                {
                    return "0: nothing";
                }
                var zObject = objectTable[value - 1];
                return zObject.Number.ToString() + ": " + zObject.ShortName;
            });

            ParseEntryView = new VariableView("pe", "Parse entry", (value, memory) =>
            {
                // Each block consists of the byte address of the word in the dictionary, if it is in the dictionary, or 0 if it isn't; followed by a byte giving the number of letters in the word; and finally a byte giving the position in the text-buffer of the first letter of the word.
                // First two bytes are the dictionary entry

                var storyService = App.Current.GetService<StoryService>();
                if (storyService == null)
                {
                    return "----";
                }
                if (!storyService.IsStoryOpen)
                {
                    return "----";
                }
                var dictionary = storyService.Story.Dictionary;

                ushort currentPosition = value;
                var memoryReader = new MemoryReader(memory, currentPosition);

                ushort dictionaryEntryAddress = memoryReader.NextWord();
                currentPosition += 2;
                byte numLetters = memory[currentPosition++];
                byte position = memory[currentPosition++];

                var dictionaryEntry = dictionary.GetEntryFromAddress(dictionaryEntryAddress);

                var word = dictionaryEntry != null ? dictionaryEntry.ZText : "---";

                return word + " - " + numLetters + " - " + position;
            });

            ParseBufferView = new VariableView("pb", "Parse buffer", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                if (storyService == null)
                {
                    return "----";
                }
                if (!storyService.IsStoryOpen)
                {
                    return "----";
                }
                var dictionary = storyService.Story.Dictionary;

                //  The number of words is written in byte 1 and one 4-byte block is written for each word, from byte 2 onwards (except that it should stop before going beyond the maximum number of words specified). 
                ushort currentPosition = value;
                byte maxNumWords = memory[currentPosition++];
                byte numWords = memory[currentPosition++];

                StringBuilder builder = new StringBuilder();
                builder.Append(maxNumWords + " - " + numWords + ": ");

                var memoryReader = new MemoryReader(memory, currentPosition);

                for (int i = 0; i < numWords; i++)
                {
                    // First two bytes are the dictionary entry
                    ushort dictionaryEntryAddress = memoryReader.NextWord();
                    var dictionaryEntry = dictionary.GetEntryFromAddress(dictionaryEntryAddress);

                    byte numLetters = memoryReader.NextByte();
                    byte position = memoryReader.NextByte();

                    var word = dictionaryEntry != null ? dictionaryEntry.ZText : "---";

                    builder.Append(word + "(" + numLetters + ", " + position + ")");

                }
                return builder.ToString();
            });

            TextBufferView = new VariableView("tb", "Text buffer", (value, memory) =>
            {
                StringBuilder result = new StringBuilder();

                //  The maximal number of characters - 1 is in byte 0
                ushort currentPosition = value;
                byte maxNumChars = memory[currentPosition++];
                byte currentByte = memory[currentPosition++];
                List<Byte> zchars = new List<byte>();
                while (currentByte != 0)
                {
                    zchars.Add(currentByte);
                    currentByte = memory[currentPosition++];
                }

                var storyService = App.Current.GetService<StoryService>();
                var currentString = storyService.Story.ZText.ZSCIIAsString(zchars.ToArray());
                return String.Format("Max chars: {0}: \"{1}\"", maxNumChars, currentString);

            });

            DictionaryEntryView = new VariableView("de", "Dictionary entry", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                if (storyService == null)
                {
                    return "----";
                }
                if (!storyService.IsStoryOpen)
                {
                    return "----";
                }
                var dictionary = storyService.Story.Dictionary;
                var entry = dictionary.GetEntryFromAddress(value);
                if (entry != null)
                {
                    var byteStrings = Array.ConvertAll(entry.Data, b => b.ToString("x2"));
                    var dataString = string.Join(" ", byteStrings);
                    return entry.ZText + "[" + dataString + "]";
                } else
                {
                    return "----";
                }
            });

            TableView = new VariableView("table", "Table view", (value, memory) =>
            {
                int NumEntries = 10;
                var reader = new MemoryReader(memory, value);
                var words = new List<String>();

                for (var i = 0; i < NumEntries; i++)
                {
                    var currentWord = reader.NextWord();
                    var word = StringView.ConvertToString(currentWord, memory);
                    words.Add(word);
                }
                return string.Join(",", words);
            });

            // This is for something found in Trinity - ideally, something game-specific like this should be implemented as a configurable view
            WorldListView = new VariableView("wl", "Word list view", (value, memory) =>
            {
                var reader = new MemoryReader(memory, value);
                // TODO: Temporary code to get to an array of word list pointers
                /* var startAddress = reader.NextWord();
                startAddress = reader.NextWord();
                reader = new MemoryReader(memory, startAddress); */
                var maxAddress = reader.NextWord();
                var currentAddress = reader.NextWord();

                var numEntries = maxAddress - 1;
                var currentIndex = currentAddress - 2;

                var words = new List<String>();

                for (var i = 0; i < numEntries; i++)
                {
                    var currentWord = reader.NextWord();
                    var word = PackedStringView.ConvertToString(currentWord, memory);
                    words.Add(word);
                }
                return "Index: " + currentIndex + ": " + string.Join(",", words);
            });



            FunctionView = new VariableView("func", "Function view", (value, memory) =>
            {
                return "not yet implemented";
            });

            InterruptView = new VariableView("interrupt", "Interrupt view", (value, memory) =>
            {
                StringBuilder result = new StringBuilder();
                int maxInterrupts = 0x64 / 4;
                var reader = new MemoryReader(memory, value);
                for (int currentIndex = 0; currentIndex < maxInterrupts; currentIndex++)
                {
                    ushort function = reader.NextWord();
                    ushort counter = reader.NextWord();
                    string s = String.Format("[{0:X};{1}]", function * 4, (short) counter);
                    result.Append(s);
                }
                return result.ToString();
            });
        }

        // TODO: Is there a better way?
        public static IEnumerable<VariableView> AllViews
        {
            get
            {
                PropertyInfo[] properties = typeof(VariableViews).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType == typeof(VariableView))
                    {
                        yield return (VariableView)property.GetValue(null);
                    }
                }
                var service = App.Current.GetService<VisualizerService>();
                foreach (var program in service.AllPrograms)
                {
                    var currentView = new VariableView(program.ShortName, program.Name, (value, memory) =>
                    {
                        var context = new VariableViewExecutionContext(value, memory);
                        bool ok = program.Execute(context);
                        return ok ? context.Result : "ERROR";
                    });
                    yield return currentView;
                }
               
               
            }
        }

        public static Dictionary<string, VariableView> StringsByID
        {
            get
            {
                var result = new Dictionary<string, VariableView>();
                foreach (var currentView in AllViews)
                {
                    result[currentView.ID] = currentView;
                }
                return result;
            }
        }
    }
}