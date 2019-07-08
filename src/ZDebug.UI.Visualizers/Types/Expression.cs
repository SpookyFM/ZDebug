using ZDebug.UI.Visualizers.Execution;

namespace ZDebug.UI.Visualizers.Types
{
    /// <summary>
    /// Any expression in the language
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Execute this expression and save the updated state to the supplied ExecutionContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if the execution was successful</returns>
        public abstract bool Execute(ExecutionContext context);
    }
}
