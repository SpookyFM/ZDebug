﻿using System;
using ZDebug.Core.Objects;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class PropertyViewModel : PropertyViewModelBase
    {
        private readonly ZProperty property;

        public PropertyViewModel(ZProperty property)
        {
            this.property = property;
        }

        public override int Number
        {
            get { return this.property.Number; }
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
                byte[] bytes= property.ReadAsBytes();
                return propertyView.ConvertToString(bytes, storyService.Story.Memory);
            }
        }
    }
}
