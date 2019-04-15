using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class CommandLineArgumentsService : IService
    {

        private readonly StoryService storyService;
        private readonly DebuggerService debuggerService;

        [ImportingConstructor]
        public CommandLineArgumentsService(StoryService storyService, DebuggerService debuggerService)
        {
            this.storyService = storyService;
            this.debuggerService = debuggerService;
        }

        public bool HandleArguments(StartupEventArgs arguments)
        {
            var parameters = new Dictionary<string, string>();
            var flags = new Dictionary<string, bool?>();
            flags["autorun"] = null;
            flags["help"] = null;
            var singleArguments = new List<string>();

            string currentKey = null;
            foreach (var currentArg in arguments.Args)
            {
                if (currentArg.StartsWith("--"))
                {
                    currentKey = currentArg.Substring(2).ToLower();
                    if (flags.ContainsKey(currentKey))
                    {
                        flags[currentKey] = true;
                        currentKey = null;
                    }
                    else
                    {
                        if (!parameters.ContainsKey(currentKey))
                        {
                            Console.WriteLine("Unknown parameter: " + currentKey);
                            return false;
                        }
                    }
                }
                else
                {
                    if (currentKey != null)
                    {
                        parameters[currentKey] = currentArg;
                    }
                    else
                    {
                        singleArguments.Add(currentArg);
                    }
                }
            }

            if (flags["help"].GetValueOrDefault(false))
            {
                PrintUsage();
                return false;
            }

            if (singleArguments.Count == 1)
            {
                if (flags["autorun"].GetValueOrDefault(false))
                {
                    storyService.StoryOpened += StoryService_StoryOpened;
                }
                storyService.OpenStory(singleArguments[0]);
            }


            return true;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            debuggerService.StartDebugging();
        }

        public void PrintUsage()
        {
            Console.WriteLine("zDebug - Debugger for z-machine code");
            Console.WriteLine("zDebug.exe [flags] [z-file]");
            Console.WriteLine("Arguments:");
            Console.WriteLine("--help: Prints this usage guide");
            Console.WriteLine("--autorun: Starts the provided z-machine file automatically");
            Console.WriteLine("[z-file]: Path to a z-machine file that will be loaded in automatically");
        }

    }
}
