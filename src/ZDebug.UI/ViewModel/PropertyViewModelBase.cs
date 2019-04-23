using System;
using ZDebug.Core.Objects;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal abstract class PropertyViewModelBase : ViewModelBase
    {

        public PropertyViewModelBase()
        {
            var propertyViewService = App.Current.GetService<PropertyViewService>();
            propertyViewService.PropertyViewChanged += PropertyViewService_PropertyViewChanged;
        }

        private void PropertyViewService_PropertyViewChanged(object sender, PropertyViewChangedArgs e)
        {
            if (e.Number == Number)
            {
                PropertyChanged("DataDisplayText");
            }
        }

        public virtual int Number
        {
            get;
        }
    }
}
