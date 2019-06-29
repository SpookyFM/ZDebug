using Sprache;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.UI.Visualizers.Execution;
using ZDebug.UI.Visualizers.Types;

namespace ZDebug.UI.Visualizers
{
    class SpracheParser
    {
        /// <summary>
        /// Keywords used in the language
        /// </summary>
        internal static List<string> Keywords = new List<string>(new string[] { "byte", "word", "for" });

        /// <summary>
        /// An Identifier is a sequence of letters, e.g. testVariable
        /// </summary>
        internal static Parser<string> IdentifierParser =
            from identifier in Parse.Letter.AtLeastOnce().Text().Token()
            where !Keywords.Contains(identifier)
            select identifier;

        internal static Parser<VariableReference> VariableReferenceParser =
            from variableName in IdentifierParser
            select new VariableReference(variableName);

        internal static Parser<string> WordParser = Parse.String("word").Text();
        internal static Parser<string> ByteParser = Parse.String("byte").Text();
        internal static Parser<string> VariableTypeParser = WordParser.Or(ByteParser);


        internal static Parser<Declaration> DeclarationParser =
            from t in VariableTypeParser
            from ws in Parse.WhiteSpace.Many()
            from i in IdentifierParser
            select new Declaration(t, i);

        internal static Parser<Literal> LiteralParser =
            from number in Parse.Digit.AtLeastOnce().Text().Token()
            select new Literal(ushort.Parse(number));

        internal static Parser<IValueSource> LiteralValueSourceParser =
            from c in LiteralParser
            select c;

        internal static Parser<FunctionCall> FunctionCallParser =
            from identifier in IdentifierParser
            from lBracket in Parse.Char('(')
            from arguments in ValueSourceParser.DelimitedBy(Parse.Char(',').Token()).Token().Optional()
            from rBracket in Parse.Char(')')
            select new FunctionCall(identifier, new List<IValueSource>(arguments.GetOrElse(new IValueSource[] { })));

        internal static Parser<IValueSource> FunctionCallValueSourceParser =
            from f in FunctionCallParser
            select f;

        internal static Parser<IValueSource> ValueSourceParser =
            LiteralValueSourceParser.Or(FunctionCallValueSourceParser).Or(VariableReferenceParser);

        internal static Parser<Assignment> AssignmentParser =
            from i in IdentifierParser
            from equality in Parse.Char('=')
            from v in ValueSourceParser
            select new Assignment(i, v);

        internal static Parser<ForLoop> ForLoopParser =
            from leading in Parse.WhiteSpace.Many().Optional()
            from f in Parse.String("for")
            from ws in Parse.WhiteSpace.Many()
            from openParentheses in Parse.Char('(')
            from loopVariable in VariableReferenceParser
            from equality in Parse.Char('=')
            from rangeStart in ValueSourceParser
            from dots in Parse.Char('.').Repeat(2)
            from rangeEnd in ValueSourceParser
            from closeParentheses in Parse.Char(')')
            from ws2 in Parse.WhiteSpace.Many().Optional()
            from openBrackets in Parse.Char('{')
            from block in BlockParser
            from closeBrackets in Parse.Char('}')
            select new ForLoop(loopVariable, rangeStart, rangeEnd, block);


        internal static Parser<Expression> ExpressionParser =
            DeclarationParser.Or<Expression>(AssignmentParser).Or<Expression>(ForLoopParser).Or(FunctionCallParser).Token();


        internal static Parser<Block> BlockParser =
            from items in ExpressionParser.AtLeastOnce()
            select new Block(new List<Expression>(items));

        internal static Parser<Program> ProgramParser =
            from items in ExpressionParser.AtLeastOnce()
            select new Program(new List<Expression>(items));

        internal static ExecutionContext context = new ExecutionContext();

        static void DoParse()
        {
            StringBuilder source = new StringBuilder();
            source.AppendLine("byte length");
            source.AppendLine("length = 7");
            source.AppendLine("word start");
            source.AppendLine("start = readWord()");
            source.AppendLine("seek(start)");
            source.AppendLine("byte currentValue");

            source.AppendLine("for (i = 0..length) {");
            source.AppendLine("   currentValue = readByte()");
            source.AppendLine("   print(currentValue, 16)");
            source.AppendLine("}");

            var program = BlockParser.Parse(source.ToString());

            program.Execute(context);
        }

        public static Program ParseProgram(string contents)
        {
            return ProgramParser.Parse(contents);
        }

       

    }
}
