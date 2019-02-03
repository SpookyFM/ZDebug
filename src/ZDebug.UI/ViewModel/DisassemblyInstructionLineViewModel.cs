using System;
using ZDebug.Core.Instructions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyInstructionLineViewModel : DisassemblyLineViewModel
    {
        private readonly Instruction instruction;
        private readonly LabelService labelService;
        private readonly bool isLast;

        public DisassemblyInstructionLineViewModel(Instruction instruction, bool isLast)
        {
            this.instruction = instruction;
            this.isLast = isLast;
            this.labelService = App.Current.GetService<LabelService>();
        }

        public int Address
        {
            get { return instruction.Address; }
        }

        public bool IsLast
        {
            get { return isLast; }
        }

        public string OpcodeName
        {
            get { return instruction.Opcode.Name; }
        }

        public Instruction Instruction
        {
            get { return instruction; }
        }

        public String LabelText
        {
            get
            {
                int? label = labelService.GetLabel(Instruction.Address);
                return label == null ? "   " : label.Value.ToString("\\L00");
            }
        }
    }
}
