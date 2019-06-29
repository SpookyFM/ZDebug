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
        }

        private Program ParseProgram(string filename)
        {
            string contents = File.ReadAllText(filename);
            Program result = SpracheParser.ParseProgram(contents);
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
                ParseProgram(fi.FullName);
            }
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            ParseAllVisualizers();
        }
    }
}
