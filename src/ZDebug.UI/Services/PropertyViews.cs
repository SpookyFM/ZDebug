using System;
using System.Collections.Generic;
using System.Reflection;
using ZDebug.Core.Basics;
using ZDebug.UI.ViewModel;
using ZDebug.UI.Visualizers.Services;

namespace ZDebug.UI.Services
{
    internal sealed class PropertyViews
    {
        public static PropertyView HexadecimalView { get; }
        public static PropertyView StringView { get; }
        public static PropertyView PackedStringView { get; }
        public static PropertyView TableView { get; }
        public static PropertyView RoutineView { get; }

        static PropertyViews()
        {
            HexadecimalView = new PropertyView("hex", "Hexadecimal view", (value, memory) =>
            {
                var bytes = value;
                var byteStrings = Array.ConvertAll(bytes, b => b.ToString("x2"));
                return string.Join(" ", byteStrings);
            });

            StringView = new PropertyView("str", "String view", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                var firstByte = value[0];
                var secondByte = value[1];
                ushort currentWord = (ushort)(((ushort)(firstByte) << 8) + secondByte);
                var zWords = storyService.Story.ZText.ReadZWords(currentWord);
                return storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All);
            });

            PackedStringView = new PropertyView("strp", "String view (packed)", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                var firstByte = value[0];
                var secondByte = value[1];
                ushort currentWord = (ushort)(((ushort)(firstByte) << 8) + secondByte);
                var zWords = storyService.Story.ZText.ReadZWords(Header.UnpackStringAddress(memory, currentWord));
                return storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All);
            });

            TableView = new PropertyView("tab", "Table view", (value, memory) =>
            {
                var storyService = App.Current.GetService<StoryService>();
                var result = new List<String>();
                for (var i = 0; i < value.Length; i+=2)
                {
                    var firstByte = value[i];
                    var secondByte = value[i + 1];
                    ushort currentWord = (ushort)(((ushort)(firstByte) << 8) + secondByte);
                    var zWords = storyService.Story.ZText.ReadZWords(currentWord);
                    result.Add(storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All));
                }
                return string.Join(", ", result);                
            });

            RoutineView = new PropertyView("rout", "Routine view", (value, memory) =>
            {
                var firstByte = value[0];
                var secondByte = value[1];
                ushort firstWord = (ushort)(((ushort)(firstByte) << 8) + secondByte);
                var address = Header.UnpackRoutineAddress(memory, firstWord);
                return address.ToString("x");
            });
        }

        public static IEnumerable<PropertyView> AllViews
        {
            get
            {
                PropertyInfo[] properties = typeof(PropertyViews).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType == typeof(PropertyView))
                    {
                        yield return (PropertyView)property.GetValue(null);
                    }
                }
                var service = App.Current.GetService<VisualizerService>();
                var program = service.AllPrograms[0];
                var testView = new PropertyView("test", program.Name, (value, memory) =>
                {
                    var context = new PropertyViewExecutionContext(value, memory);
                    program.Execute(context);
                    return context.Result;
                });
                yield return testView;
            }
        }

        public static Dictionary<string, PropertyView> StringsByID
        {
            get
            {
                var result = new Dictionary<string, PropertyView>();
                foreach (var currentView in AllViews)
                {
                    result[currentView.ID] = currentView;
                }
                return result;
            }
        }
    }
}
