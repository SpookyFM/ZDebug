using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// A ValueSource allows retrieving a value
    /// </summary>
    interface IValueSource
    {
        ushort GetValue(ExecutionContext context);
    }
}
