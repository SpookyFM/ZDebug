﻿using System.Composition;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class StorageService : IService
    {
        private readonly IPersistable storyPersistence;
        private readonly IPersistable breakpointPersistence;
        private readonly IPersistable dataBreakpointPersistence;
        private readonly IPersistable gameScriptPersistence;
        private readonly IPersistable routinePersistence;
        

        [ImportingConstructor]
        public StorageService(
            StoryService storyService,
            BreakpointService breakpointService,
            DataBreakpointService dataBreakpointService,
            GameScriptService gameScriptService,
            RoutineService routineService)
        {
            this.storyPersistence = storyService;
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
            this.breakpointPersistence = breakpointService;
            this.dataBreakpointPersistence = dataBreakpointService;
            this.gameScriptPersistence = gameScriptService;
            this.routinePersistence = routineService;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            LoadSettings(e.Story);
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            SaveSettings(e.Story);
        }

        private void LoadSettings(Story story)
        {
            var xml = Storage.RestoreStorySettings(story);

            breakpointPersistence.Load(xml);
            dataBreakpointPersistence.Load(xml);
            gameScriptPersistence.Load(xml);
            routinePersistence.Load(xml);
        }

        private void SaveSettings(Story story)
        {
            var xml =
                new XElement("settings",
                    storyPersistence.Store(),
                    breakpointPersistence.Store(),
                    dataBreakpointPersistence.Store(),
                    gameScriptPersistence.Store(),
                    routinePersistence.Store());

            Storage.SaveStorySettings(story, xml);
        }
    }
}
