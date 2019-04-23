using System.Collections.Generic;
using System.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.UI.Collections;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{

    struct BitmapTest
    {
        public int ByteToApplyTo;
        public byte Bitmap;
    }

    [Export, Shared]
    internal sealed class DictionaryViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly BulkObservableCollection<DictionaryEntryViewModel> entries;
        private readonly BitmapTest test;

        [ImportingConstructor]
        public DictionaryViewModel(
            StoryService storyService,
            PropertyViewService propertyViewService)
            : base("DictionaryView")
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            entries = new BulkObservableCollection<DictionaryEntryViewModel>();
            test.Bitmap = 0xcb;
            test.ByteToApplyTo = 1;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            entries.BeginBulkOperation();
            try
            {
                foreach (var entry in e.Story.Dictionary)
                {
                    var vm = new DictionaryEntryViewModel(entry);
                    if (vm.PassesTest(test))
                    {
                        entries.Add(new DictionaryEntryViewModel(entry));
                    }
                }
            }
            finally
            {
                entries.EndBulkOperation();
            }

            PropertyChanged("HasStory");
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            entries.Clear();

            PropertyChanged("HasStory");
        }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public BulkObservableCollection<DictionaryEntryViewModel> Entries
        {
            get { return entries; }
        }
    }
}
