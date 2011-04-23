﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using ZDebug.Compiler.Analysis.ControlFlow;
using ZDebug.Compiler.CodeGeneration;
using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;
using ZDebug.Core.Utilities;

// Potential optimizations:
//
// * Speed up calculated calls
// * Create ref locals for globals directly accessed

namespace ZDebug.Compiler
{
    internal partial class ZCompiler
    {
        private readonly ZRoutine routine;
        private readonly CompiledZMachine machine;
        private bool debugging;

        private ILBuilder il;
        private ControlFlowGraph controlFlowGraph;
        private Dictionary<int, ILabel> addressToLabelMap;
        private List<ZRoutineCall> calls;

        private ILocal screen;
        private ILocal outputStreams;

        private bool usesStack;
        private bool usesMemory;
        private bool usesScreen;
        private bool usesOutputStreams;

        private int calculatedLoadVariableCount;
        private int calculatedStoreVariableCount;

        private ZCompiler(ZRoutine routine, CompiledZMachine machine, bool debugging = false)
        {
            this.routine = routine;
            this.machine = machine;
            this.debugging = debugging;
        }

        private static DynamicMethod CreateDynamicMethod(ZRoutine routine)
        {
            return new DynamicMethod(
                name: string.Format("{0:x4}_{1}_locals", routine.Address, routine.Locals.Length),
                returnType: typeof(ushort),
                parameterTypes: Types.Array<CompiledZMachine, byte[], ushort[], ushort[], int, ZRoutineCall[], int>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);
        }

        public ZCompilerResult Compile()
        {
            var sw = Stopwatch.StartNew();

            var dm = CreateDynamicMethod(routine);
            this.il = new ILBuilder(dm.GetILGenerator());

            this.calls = new List<ZRoutineCall>();

            Profiler_EnterRoutine();

            this.controlFlowGraph = ControlFlowGraph.Build(this.routine);
            this.addressToLabelMap = new Dictionary<int, ILabel>(this.controlFlowGraph.CodeBlocks.Count());

            // Determine whether stack, memory, screen and outputStreams are used.
            foreach (var codeBlock in this.controlFlowGraph.CodeBlocks)
            {
                this.addressToLabelMap.Add(codeBlock.Address, il.NewLabel());

                foreach (var i in codeBlock.Instructions)
                {
                    if (!this.usesStack && i.UsesStack())
                    {
                        this.usesStack = true;
                    }

                    if (!this.usesMemory && i.UsesMemory())
                    {
                        this.usesMemory = true;
                    }

                    if (!this.usesScreen && i.UsesScreen())
                    {
                        this.usesScreen = true;

                        // screen...
                        var screenField = Reflection<ZMachine>.GetField("Screen", @public: false);
                        this.screen = il.NewLocal<IScreen>(il.GenerateLoadInstanceField(screenField));
                    }

                    if (!this.usesOutputStreams && i.UsesOutputStreams())
                    {
                        this.usesOutputStreams = true;

                        // outputStreams...
                        var outputStreamsField = Reflection<ZMachine>.GetField("OutputStreams", @public: false);
                        this.outputStreams = il.NewLocal<IOutputStream>(il.GenerateLoadInstanceField(outputStreamsField));
                    }
                }
            }

            // Emit IL
            foreach (var codeBlock in this.controlFlowGraph.CodeBlocks)
            {
                var generators = codeBlock.Instructions.Select(i => OpcodeGenerator.GetGenerator(i, machine.Version));

                foreach (var generator in generators)
                {
                    ILabel label;
                    if (this.addressToLabelMap.TryGetValue(generator.Instruction.Address, out label))
                    {
                        label.Mark();
                    }

                    if (machine.Debugging)
                    {
                        il.Arguments.LoadMachine();
                        il.Call(Reflection<CompiledZMachine>.GetMethod("Tick", @public: false));
                    }

                    Profiler_ExecutingInstruction(generator.Instruction);
                    il.DebugWrite(generator.Instruction.PrettyPrint(machine));

                    generator.Generate(il, this);
                }
            }

            var code = (ZRoutineCode)dm.CreateDelegate(typeof(ZRoutineCode), machine);

            sw.Stop();

            var statistics = new RoutineCompilationStatistics(
                this.routine,
                il.OpcodeCount,
                il.LocalCount,
                il.Size,
                sw.Elapsed,
                calculatedLoadVariableCount,
                calculatedStoreVariableCount);

            return new ZCompilerResult(this.routine, calls.ToArray(), code, statistics);
        }

        private void Profiler_EnterRoutine()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(routine.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("EnterRoutine", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExitRoutine()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(routine.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExitRoutine", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExecutingInstruction(Instruction instruction)
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(instruction.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExecutingInstruction", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExecutedInstruction()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExecutedInstruction", Types.None, @public: false));
            }
        }

        private void Profiler_Quit()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("Quit", Types.None, @public: false));

                Profiler_ExecutedInstruction();
            }
        }

        private void Profiler_Interrupt()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("Interrupt", Types.None, @public: false));
            }
        }

        public static ZCompilerResult Compile(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
