using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core.Basics;

namespace ZDebug.UI.Services
{

    [Export, Shared]
    internal class DataBreakpointService : IService, IPersistable
    {
        private readonly StoryService storyService;
        private readonly SortedSet<DataBreakpoint> breakpoints = new SortedSet<DataBreakpoint>();

        [ImportingConstructor]
        public DataBreakpointService(StoryService storyService)
        {
            this.storyService = storyService;
            this.storyService.StoryClosed += StoryService_StoryClosed;
            this.storyService.StoryOpened += StoryService_StoryOpened;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            // Update all breakpoints as we might have created them before we had memory to load their initial values from
            UpdateAllBreakpoints(e.Story.Memory);
        }

        private void StoryService_StoryClosed(object sender, StoryClosedEventArgs e)
        {
            breakpoints.Clear();
        }

        public void Add(int address, int length, byte[] memory)
        {
            var newBreakpoint = new DataBreakpoint(address, length);
            newBreakpoint.UpdateFromMemory(memory);
            breakpoints.Add(newBreakpoint);
        }

        public void Add(int address, int length)
        {
            var newBreakpoint = new DataBreakpoint(address, length);
            breakpoints.Add(newBreakpoint);
        }

        public void AddGlobalVariable(int index, byte[] memory)
        {
            var startAddress = Header.ReadGlobalVariableTableAddress(memory);
            if (index < 0 || index > 239)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Add(startAddress + (index * 2), 2, memory);
        }

        
        public void Remove(int address, int length)
        {
            var tempBreakpoint = new DataBreakpoint(address, length);
            breakpoints.Remove(tempBreakpoint);
        }

        public void Toggle(int address, int length, byte[] memory)
        {
            var tempBreakpoint = new DataBreakpoint(address, length);
            if (breakpoints.Contains(tempBreakpoint))
            {
                Remove(address, length);
            }
            else
            {
                Add(address, length, memory);
            }
        }

        public bool Exists(int address, int length)
        {
            var tempBreakpoint = new DataBreakpoint(address, length);
            return breakpoints.Contains(tempBreakpoint);
        }

        // Updates and returns the all breakpoints that have triggered because of this
        public IEnumerable<DataBreakpoint> UpdateAllBreakpoints(byte[] memory)
        {
            foreach (var breakpoint in Breakpoints)
            {
                bool triggered = breakpoint.UpdateFromMemory(memory);
                if (triggered)
                {
                    yield return breakpoint;
                }
            }
        }

        public void Clear()
        {
            breakpoints.Clear();
        }

        public IEnumerable<DataBreakpoint> Breakpoints
        {
            get
            {
                foreach (var breakpoint in breakpoints)
                {
                    yield return breakpoint;
                }
            }
        }

        void IPersistable.Load(XElement xml)
        {
            breakpoints.Clear();

            var bpsElem = xml.Element("dataBreakpoints");
            if (bpsElem != null)
            {
                foreach (var bpElem in bpsElem.Elements("dataBreakpoint"))
                {
                    var addAttr = bpElem.Attribute("address");
                    var lengthAttr = bpElem.Attribute("length");
                    Add((int)addAttr, (int)lengthAttr);
                }
            }
        }

        XElement IPersistable.Store()
        {
            return new XElement("dataBreakpoints",
                breakpoints.Select(b => new XElement("dataBreakpoint",
                    new XAttribute("address", b.Address),
                    new XAttribute("length", b.Length)
                )));
        }
    }
}
