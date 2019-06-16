using System.Collections.Generic;
using System.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.UI.Collections;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class ObjectsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly PropertyViewService propertyViewService;
        private readonly BulkObservableCollection<ObjectViewModelBase> objects;

        [ImportingConstructor]
        public ObjectsViewModel(
            StoryService storyService,
            PropertyViewService propertyViewService)
            : base("ObjectsView")
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            this.propertyViewService = propertyViewService;

            this.NavigateCommand = RegisterCommand<int>(
                text: "Navigate",
                name: "Navigate",
                executed: NavigateExecuted,
                canExecute: CanNavigateExecute);

            this.SetPropertyViewCommand = RegisterCommand<KeyValuePair<PropertyViewModel, PropertyView>>(
                text: "Set Property View",
                name: "SetPropertyView",
                executed: SetPropertyViewExecuted,
                canExecute: CanSetPropertyViewExecute);

            objects = new BulkObservableCollection<ObjectViewModelBase>();
        }

        private bool CanNavigateExecute(int number)
        {
            return number > 0;
        }

        private void NavigateExecuted(int number)
        {
            var listObjects = this.View.FindName<ListBox>("listObjects");
            listObjects.SelectedIndex = number - 1; // object indeces are 1-based
            listObjects.ScrollIntoView(listObjects.SelectedItem);
        }

        private bool CanSetPropertyViewExecute(KeyValuePair<PropertyViewModel, PropertyView> argument)
        {
            return argument.Key != null && argument.Value != null;
        }

        private void SetPropertyViewExecuted(KeyValuePair<PropertyViewModel, PropertyView> argument)
        {
            propertyViewService.SetViewForProperty(argument.Key.Number, argument.Value);
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            var unusedProperties = new List<int>();
            for (var i = 0; i < 64; i++)
            {
                unusedProperties.Add(i);
            }
            objects.BeginBulkOperation();
            try
            {
                // Add one for the default object
                objects.Add(new ObjectViewModelDefault());
                byte? propertyToTest = null; // 0x30;
                foreach (var obj in e.Story.ObjectTable)
                {
                    for (var i = 0; i < 64; i++)
                    {
                        if (obj.PropertyTable.GetByNumber(i) != null) {
                            unusedProperties.Remove(i);
                        }
                    }
                    if (!propertyToTest.HasValue || obj.PropertyTable.GetByNumber(propertyToTest ?? 0) != null)
                    {
                        objects.Add(new ObjectViewModel(obj));
                    }
                }
            }
            finally
            {
                objects.EndBulkOperation();
            }

            PropertyChanged("HasStory");
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            objects.Clear();

            PropertyChanged("HasStory");
        }

        public ICommand NavigateCommand { get; private set; }
        public ICommand SetPropertyViewCommand { get; private set; }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public BulkObservableCollection<ObjectViewModelBase> Objects
        {
            get { return objects; }
        }
    }
}
