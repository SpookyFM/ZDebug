using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A ValueSource allows retrieving a value
    /// </summary>
    interface IValueSource
    {
        ushort GetWordValue(ExecutionContext context);

        string GetStringValue(ExecutionContext context);

        System.Type GetValueType();
    }
}
