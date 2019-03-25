using System;
using System.Collections.Generic;
using System.Reflection;
using ZDebug.Core.Basics;
using ZDebug.UI.ViewModel;

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

            FunctionView = new VariableView("func", "Function view", (value, memory) =>
            {
                return "not yet implemented";
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