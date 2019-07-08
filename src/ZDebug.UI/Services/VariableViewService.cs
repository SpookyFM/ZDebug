using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core.Routines;
using ZDebug.UI.Visualizers.Services;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class VariableViewService : IService, IPersistable
    {
        private readonly StoryService storyService;
        // private readonly VisualizerService visualizerService;
        private Dictionary<KeyValuePair<int, int>, VariableView> localsViews;
        private Dictionary<int, VariableView> globalsViews;
        


        [ImportingConstructor]
        public VariableViewService(
            StoryService storyService)
            //VisualizerService visualizerService)
        {
            this.storyService = storyService;

            localsViews = new Dictionary<KeyValuePair<int, int>, VariableView>();
            globalsViews = new Dictionary<int, VariableView>();
            // visualizerService
        }

        public void SetViewForLocal(ZRoutine routine, int localIndex, VariableView view)
        {
            SetViewForLocal(routine.Address, localIndex, view);
        }

        public void SetViewForLocal(int address, int localIndex, VariableView view)
        {
            var index = new KeyValuePair<int, int>(address, localIndex);
            localsViews[index] = view;
            LocalViewChanged?.Invoke(this, new LocalViewChangedArgs(address, localIndex, view));
        }

        public VariableView GetViewForLocal(ZRoutine routine, int localIndex)
        {
            var index = new KeyValuePair<int, int>(routine.Address, localIndex);
            VariableView result = null;
            localsViews.TryGetValue(index, out result);
            return result;
        }

        public void SetViewForGlobal(int globalIndex, VariableView view)
        {
            globalsViews[globalIndex] = view;
            GlobalViewChanged?.Invoke(this, new GlobalViewChangedArgs(globalIndex, view));
        }

        public VariableView GetViewForGlobal(int globalIndex)
        {
            VariableView result = null;
            globalsViews.TryGetValue(globalIndex, out result);
            return result;
        }

        public event EventHandler<LocalViewChangedArgs> LocalViewChanged;
        public event EventHandler<GlobalViewChangedArgs> GlobalViewChanged;

        void IPersistable.Load(XElement xml)
        {
            var variableViewsElem = xml.Element("variableViews");
            if (variableViewsElem != null)
            {
                var localsViewsElem = variableViewsElem.Element("localsViews");
                if (localsViewsElem != null)
                {
                    var viewsDictionary = VariableViews.StringsByID;
                    foreach (var localViewElem in localsViewsElem.Elements("localView"))
                    {
                        var addressAttr = localViewElem.Attribute("address");
                        var localNumberAttr = localViewElem.Attribute("localNumber");
                        var variableViewAttr = localViewElem.Attribute("variableView");

                        SetViewForLocal((int)addressAttr, (int)localNumberAttr, viewsDictionary[variableViewAttr.Value]);
                    }
                }
                var globalsViewsElem = variableViewsElem.Element("globalsViews");
                if (globalsViewsElem != null)
                {
                    var viewsDictionary = VariableViews.StringsByID;
                    foreach (var globalViewElem in globalsViewsElem.Elements("globalView"))
                    {
                        var indexAttr = globalViewElem.Attribute("index");
                        var variableViewAttr = globalViewElem.Attribute("variableView");

                        SetViewForGlobal((int)indexAttr, viewsDictionary[variableViewAttr.Value]);
                    }
                }
            }
            
           
        }

        XElement IPersistable.Store()
        {
            return new XElement("variableViews",
                new XElement("localsViews",
                localsViews.Select(kvp => new XElement("localView",
                    new XAttribute("address", kvp.Key.Key),
                    new XAttribute("localNumber", kvp.Key.Value),
                    new XAttribute("variableView", kvp.Value.ID)))),
                new XElement("globalsViews",
                    globalsViews.Select(kvp => new XElement("globalView",
                        new XAttribute("index", kvp.Key),
                        new XAttribute("variableView", kvp.Value.ID))))
                    );
        }
    }
}
