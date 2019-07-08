using System.Collections.Generic;
using System.Composition;
using System.IO;
using ZDebug.UI.Services;
using ZDebug.UI.Visualizers.Types;

namespace ZDebug.UI.Visualizers.Services
{
    [Export, Shared]
    public class VisualizerService: IService
    {
        StoryService storyService;

        [ImportingConstructor]
        public VisualizerService(StoryService storyService)
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;

            AllPrograms = new List<Program>();

            ParseAllVisualizers();
        }

        public List<Program> AllPrograms;

        private Program ParseProgram(string filename)
        {
            string contents = File.ReadAllText(filename);
            Program result = SpracheParser.ParseProgram(contents);
            result.Name = filename;
            return result;
        }

        private void ParseAllVisualizers()
        {
            string folderName = "vis";
            string fullPath = Path.Combine(System.AppContext.BaseDirectory, folderName);
            DirectoryInfo di = new DirectoryInfo(fullPath);
            var files = di.GetFiles("*.zvis");
            foreach (FileInfo fi in files)
            {
                AllPrograms.Add(ParseProgram(fi.FullName));
            }
        }

        private string GetSynonyms(ZDebug.Core.Objects.ZObject obj)
        {
            var property = obj.PropertyTable.GetByNumber(0x33);
            if (property == null)
            {
                return string.Empty;
            }
            var result = new List<string>();
            var value = property.ReadAsBytes();
            for (var i = 0; i < value.Length / 2; i += 2)
            {
                var firstByte = value[i];
                var secondByte = value[i + 1];
                ushort currentWord = (ushort)(((ushort)(firstByte) << 8) + secondByte);
                var zWords = storyService.Story.ZText.ReadZWords(currentWord);
                result.Add(storyService.Story.ZText.ZWordsAsString(zWords, Core.Text.ZTextFlags.All));
            }
            return string.Join(", ", result);
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            // ParseAllVisualizers();

            /* 
            List<string> result = new List<string>();

            // Temporary code for exporting all synonyms
            foreach (var currentObject in storyService.Story.ObjectTable)
            {
                string synonyms = GetSynonyms(currentObject);
                result.Add(currentObject.Number + ": " + synonyms);
            }

            string folderName = "out";
            string fullPath = Path.Combine(System.AppContext.BaseDirectory, folderName);
            File.WriteAllLines(fullPath + "\\out.txt", result); */
        }
    }
}
