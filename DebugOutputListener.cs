using System.Diagnostics;

namespace SkyboundLauncher
{
    /// <summary>
    /// Global listener for capturing Debug output
    /// </summary>
    public class DebugOutputListener : TraceListener
    {
        public static event Action<string>? OnDebugMessage;

        public override void Write(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                OnDebugMessage?.Invoke(message);
            }
        }

        public override void WriteLine(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                OnDebugMessage?.Invoke(message + Environment.NewLine);
            }
        }
    }
}
