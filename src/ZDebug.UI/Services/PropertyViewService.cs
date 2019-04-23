using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core.Routines;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class PropertyViewService : IService, IPersistable
    {
        private readonly StoryService storyService;
        private Dictionary<int, PropertyView> propertyViews;


        [ImportingConstructor]
        public PropertyViewService(
            StoryService storyService)
        {
            this.storyService = storyService;

            propertyViews = new Dictionary<int, PropertyView>();
        }

        public void SetViewForProperty(int propertyNumber, PropertyView view)
        {
            propertyViews[propertyNumber] = view;
            PropertyViewChanged?.Invoke(this, new PropertyViewChangedArgs(propertyNumber, view));
        }

        public PropertyView GetViewForProperty(int propertyNumber)
        {
            PropertyView result = null;
            propertyViews.TryGetValue(propertyNumber, out result);
            return result;

        }

        public event EventHandler<PropertyViewChangedArgs> PropertyViewChanged;

        void IPersistable.Load(XElement xml)
        {
            var propertyViewsElem = xml.Element("propertyViews");
            if (propertyViewsElem != null)
            {
                var viewsDictionary = PropertyViews.StringsByID;
                foreach (var viewElem in propertyViewsElem.Elements("propertyView"))
                {
                    var numberAttr = viewElem.Attribute("number");
                    var viewAttr = viewElem.Attribute("view");

                    SetViewForProperty((int)numberAttr, viewsDictionary[viewAttr.Value]);
                }
            }
        }

        XElement IPersistable.Store()
        {
            return new XElement("propertyViews",
                propertyViews.Select(kvp => new XElement("propertyView",
                    new XAttribute("number", kvp.Key),
                    new XAttribute("view", kvp.Value.ID)))
                );
        }
    }
}
