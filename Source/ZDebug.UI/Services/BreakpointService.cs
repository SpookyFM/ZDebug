﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    [Export]
    internal class BreakpointService : IService, IPersistable
    {
        private readonly StoryService storyService;
        private readonly SortedSet<int> breakpoints = new SortedSet<int>();

        [ImportingConstructor]
        private BreakpointService(StoryService storyService)
        {
            this.storyService = storyService;
            this.storyService.StoryClosed += StoryService_StoryClosed;
        }

        private void StoryService_StoryClosed(object sender, StoryClosedEventArgs e)
        {
            breakpoints.Clear();
        }

        private void OnAdded(int address)
        {
            var handler = Added;
            if (handler != null)
            {
                handler(this, new BreakpointEventArgs(address));
            }
        }

        private void OnRemoved(int address)
        {
            var handler = Removed;
            if (handler != null)
            {
                handler(this, new BreakpointEventArgs(address));
            }
        }

        private void OnReset()
        {
            var handler = Reset;
            if (handler != null)
            {
                handler(this, new ResetEventArgs());
            }
        }

        public void Add(int address)
        {
            breakpoints.Add(address);
            OnAdded(address);
        }

        public void Remove(int address)
        {
            breakpoints.Remove(address);
            OnRemoved(address);
        }

        public void Toggle(int address)
        {
            if (breakpoints.Contains(address))
            {
                Remove(address);
            }
            else
            {
                Add(address);
            }
        }

        public bool Exists(int address)
        {
            return breakpoints.Contains(address);
        }

        public void Clear()
        {
            breakpoints.Clear();
            OnReset();
        }

        public IEnumerable<int> Breakpoints
        {
            get
            {
                foreach (var address in breakpoints)
                {
                    yield return address;
                }
            }
        }

        public void Load(XElement xml)
        {
            breakpoints.Clear();

            var bpsElem = xml.Element("breakpoints");
            if (bpsElem != null)
            {
                foreach (var bpElem in bpsElem.Elements("breakpoint"))
                {
                    var addAttr = bpElem.Attribute("address");
                    breakpoints.Add((int)addAttr);
                }
            }

            OnReset();
        }

        public XElement Store()
        {
            return new XElement("breakpoints",
                breakpoints.Select(b => new XElement("breakpoint", new XAttribute("address", b))));
        }

        public event EventHandler<BreakpointEventArgs> Added;
        public event EventHandler<BreakpointEventArgs> Removed;
        public event EventHandler<ResetEventArgs> Reset;
    }
}