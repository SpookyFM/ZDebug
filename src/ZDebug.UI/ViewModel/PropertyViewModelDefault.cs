using System;
using ZDebug.Core.Objects;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class PropertyViewModelDefault : PropertyViewModelBase
    {

        private readonly int number;
        private readonly byte[] value;

        public PropertyViewModelDefault(int number)
        {
            this.number = number;
            var storyService = App.Current.GetService<StoryService>();
            var defaultValue = storyService.Story.ObjectTable.GetPropertyDefault(Number);
            value = BitConverter.GetBytes(defaultValue);
        }

        public override int Number
        {
            get { return number; }
        }

        public string DataDisplayText
        {
            get
            {
                var propertyViewService = App.Current.GetService<PropertyViewService>();
                var propertyView = propertyViewService.GetViewForProperty(Number);

                
                if (propertyView == null)
                {
                    propertyView = PropertyViews.HexadecimalView;
                }

                var storyService = App.Current.GetService<StoryService>();
                return propertyView.ConvertToString(value, storyService.Story.Memory);
            }
        }
    }
}
