using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Objects;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class ObjectViewModelDefault : ObjectViewModelBase
    {
        private readonly ReadOnlyCollection<PropertyViewModelBase> properties;

        public ObjectViewModelDefault()
        {
            var props = new List<PropertyViewModelBase>();
            var storyService = App.Current.GetService<StoryService>();
            // We need to fill the table from the defaults
            for (var currentNumber = 1; currentNumber < storyService.Story.ObjectTable.MaxProperties; currentNumber++)
            {
                props.Add(new PropertyViewModelDefault(currentNumber));
            }

            properties = new ReadOnlyCollection<PropertyViewModelBase>(props);
        }

        public ReadOnlyCollection<PropertyViewModelBase> Properties
        {
            get { return properties; }
        }
    }
}
