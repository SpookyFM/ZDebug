using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;
using ZDebug.IO.Services;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class InstructionTextBuilder
        {
            private static readonly TextFormatter formatter = TextFormatter.Create(TextFormattingMode.Display);

            private readonly TextParagraphProperties defaultParagraphProps;
            private readonly InstructionTextSource textSource;
            private readonly LabelService labelService;

            private List<TextLine> textLines;

            public InstructionTextBuilder()
            {
                this.defaultParagraphProps = new SimpleTextParagraphProperties(FontsAndColorsService.DefaultSetting);
                this.labelService = App.Current.GetService<LabelService>();

                textSource = new InstructionTextSource();
                textLines = new List<TextLine>();
            }

            public object GetTagFromIndex(int index)
            {
                var span = textSource.GetSpan(index);
                return span.Value.Tag;
            }

            public void Clear()
            {
                textSource.Clear();
            }

            public List<TextLine> Lines
            {
                get
                {
                    return textLines;
                }
            }

            public Size Measure(double width)
            {
                var height = 0.0;

                int textSourcePosition = 0;
                while (textSourcePosition < textSource.Length)
                {
                    using (var line = formatter.FormatLine(textSource, textSourcePosition, width, defaultParagraphProps, previousLineBreak: null, textRunCache: textSource.Cache))
                    {
                        height += line.Height;
                        textSourcePosition += line.Length;
                    }
                }

                return new Size(width, height);
            }

            public void Draw(DrawingContext context, double width)
            {
                var top = 0.0;

                int textSourcePosition = 0;
                textLines.Clear();
                while (textSourcePosition < textSource.Length)
                {
                    using (var line = formatter.FormatLine(textSource, textSourcePosition, width, defaultParagraphProps, previousLineBreak: null, textRunCache: textSource.Cache))
                    {
                        textLines.Add(line);
                        line.Draw(context, new Point(0.0, top), InvertAxes.None);
                        top += line.Height;
                        textSourcePosition += line.Length;
                    }
                }
            }

            private void AddText(string text, FontAndColorSetting setting, object tag = null)
            {
                if (setting == null)
                {
                    throw new ArgumentNullException("setting");
                }

                textSource.Add(text, setting, tag);
            }

            public void AddAddress(int address)
            {
                AddText(address.ToString("x4"), FontsAndColorsService.AddressSetting, address);
            }

            public void AddLabel(int label)
            {
                AddText(label.ToString("\\L00"), FontsAndColorsService.AddressSetting, label);
            }

            public void AddBranch(Instruction instruction)
            {
                var branch = instruction.Branch;

                AddSeparator("[");
                AddKeyword(branch.Condition ? "TRUE" : "FALSE");
                AddSeparator("] ");

                if (branch.Kind == BranchKind.RFalse)
                {
                    AddKeyword("rfalse");
                }
                else if (branch.Kind == BranchKind.RTrue)
                {
                    AddKeyword("rtrue");
                }
                else // BranchKind.Address
                {
                    var targetAddress = instruction.Address + instruction.Length + branch.Offset - 2;
                    var targetLabel = labelService.GetLabel(targetAddress);
                    if (targetLabel == null)
                    {
                        AddAddress(targetAddress);
                    } else
                    {
                        AddLabel(targetLabel.Value);
                    }
                    
                }
            }

            public void AddByRefOperand(Operand operand)
            {
                if (operand.Kind == OperandKind.SmallConstant)
                {
                    AddVariable(Variable.FromByte((byte)operand.Value));
                }
                else if (operand.Kind == OperandKind.Variable)
                {
                    AddSeparator("[");
                    AddOperand(operand);
                    AddSeparator("]");
                }
                else // OperandKind.LargeConstant
                {
                    throw new InvalidOperationException("ByRef operand must be a small constant or a variable.");
                }
            }

            public void AddConstant(ushort value)
            {
                AddText("#" + value.ToString("x4"), FontsAndColorsService.ConstantSetting, value);
            }

            public void AddConstant(byte value)
            {
                AddText("#" + value.ToString("x2"), FontsAndColorsService.ConstantSetting, value);
            }

            public void AddKeyword(string text)
            {
                AddText(text, FontsAndColorsService.KeywordSetting, null);
            }

            public void AddOperand(Operand operand)
            {
                if (operand.Kind == OperandKind.LargeConstant)
                {
                    AddConstant(operand.Value);
                }
                else if (operand.Kind == OperandKind.SmallConstant)
                {
                    AddConstant((byte)operand.Value);
                }
                else // OperandKind.Variable
                {
                    AddVariable(Variable.FromByte((byte)operand.Value));
                }
            }

            public void AddOperands(ReadOnlyArray<Operand> operands)
            {
                var firstOpAdded = false;

                foreach (var op in operands)
                {
                    if (firstOpAdded)
                    {
                        AddSeparator(", ");
                    }

                    AddOperand(op);

                    firstOpAdded = true;
                }
            }

            public void AddSeparator(string text)
            {
                AddText(text, FontsAndColorsService.SeparatorSetting, null);
            }

            public void AddVariable(Variable variable, bool @out = false)
            {
                if (variable.Kind == VariableKind.Stack)
                {
                    if (@out)
                    {
                        AddSeparator("-(");
                        AddText("SP", FontsAndColorsService.StackVariableSetting);
                        AddSeparator(")");
                    }
                    else
                    {
                        AddSeparator("(");
                        AddText("SP", FontsAndColorsService.StackVariableSetting);
                        AddSeparator(")+");
                    }
                }
                else if (variable.Kind == VariableKind.Local)
                {
                    AddText(variable.ToString(), FontsAndColorsService.LocalVariableSetting, variable);
                }
                else // VariableKind.Global
                {
                    AddText(variable.ToString(), FontsAndColorsService.GlobalVariableSetting, variable);
                }
            }

            public void AddZText(string ztext)
            {
                AddText(ztext, FontsAndColorsService.ZTextSetting);
            }
        }
    }
}
