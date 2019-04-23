using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Objects;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class ObjectViewModel : ObjectViewModelBase
    {
        private readonly ZObject obj;
        private readonly ReadOnlyCollection<PropertyViewModelBase> properties;

        public ObjectViewModel(ZObject obj)
        {
            this.obj = obj;

            var props = new List<PropertyViewModelBase>();
            foreach (var prop in obj.PropertyTable)
            {
                props.Add(new PropertyViewModel(prop));
            }

            properties = new ReadOnlyCollection<PropertyViewModelBase>(props);
        }

        public int Number
        {
            get { return obj.Number; }
        }

        public int Parent
        {
            get { return obj.HasParent ? obj.Parent.Number : 0; }
        }

        public int Sibling
        {
            get { return obj.HasSibling ? obj.Sibling.Number : 0; }
        }

        public int Child
        {
            get { return obj.HasChild ? obj.Child.Number : 0; }
        }

        public string ShortName
        {
            get { return obj.ShortName; }
        }

        public string Attributes
        {
            get
            {
                var attributes = obj.GetAllAttributes();

                var list = new List<string>();
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i])
                    {
                        list.Add(i.ToString());
                    }
                }

                if (list.Count > 0)
                {
                    return string.Join(", ", list);
                }
                else
                {
                    return "None";
                }
            }
        }

        public int PropertyTableAddress
        {
            get { return obj.PropertyTable.Address; }
        }

        public ReadOnlyCollection<PropertyViewModelBase> Properties
        {
            get { return properties; }
        }
    }
}
