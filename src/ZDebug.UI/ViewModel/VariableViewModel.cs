
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal class VariableViewModel : ViewModelBase
    {
        private ushort value;
        private bool isModified;
        private bool visible;
        private VariableView variableView;

        public VariableViewModel(ushort value)
        {
            this.value = value;
        }

        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (isModified != value)
                {
                    isModified = value;
                    PropertyChanged("IsModified");
                }
            }
        }

        public ushort Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    PropertyChanged("Value");
                    PropertyChanged("DisplayValue");
                }
            }
        }

        public VariableView VariableView
        {
            get
            {
                return variableView;
            }
            set
            {
                if (this.variableView != value)
                {
                    this.variableView = value;
                    PropertyChanged("VariableView");
                    PropertyChanged("DisplayValue");
                }
            }
        }

        public string DisplayValue
        {
            get
            {
                var storyService = App.Current.GetService<StoryService>();
                if (!storyService.IsStoryOpen)
                {

                    return VariableViews.HexadecimalView.ConvertToString(this.value, new byte[] { });
                }
                var memory = storyService.Story.Memory;
                if (VariableView == null)
                {
                    return VariableViews.HexadecimalView.ConvertToString(this.value, memory);
                }
                return VariableView.ConvertToString(this.value, memory);
            }
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    PropertyChanged("Visible");
                }
            }
        }
    }
}
