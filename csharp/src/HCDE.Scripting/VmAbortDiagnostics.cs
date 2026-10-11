using System.Globalization;
using System.Text;

namespace HCDE.Scripting;

/// <summary>Native VM abort reason text and script stack formatting.</summary>
public static class VmAbortDiagnostics
{
    public static string FormatMessage(ushort reason, string? context = null) => "VM execution aborted: " + (reason switch
    {
        0 => "",
        1 => "tried to read from address zero.",
        2 => "tried to write to address zero.",
        3 => "too many try-catch blocks.",
        4 => "array access out of bounds.",
        5 => "division by zero.",
        6 => "invalid self pointer.",
        7 => "string format failed.",
        _ => "Unknown reason " + reason.ToString(CultureInfo.InvariantCulture),
    }) + (context is null ? "" : (reason == 0 ? "" : " ") + context);

    internal static string ScopeName(int side) => side switch
    {
        0 => "data", 1 => "ui", 2 => "play", 3 => "virtualscope", 4 => "clearscope", _ => "unknown",
    };

    public static string FormatStackTrace(IReadOnlyList<VmAbortFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        var text = new StringBuilder();
        foreach (var frame in frames)
        {
            if (frame.IsNative)
            {
                text.Append("Called from ").Append(frame.FunctionName).Append('\n');
                continue;
            }
            text.Append("Called from ").Append(frame.FunctionName).Append(" at ").Append(frame.SourceFileName)
                .Append(", line ").Append(frame.SourceLine.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
        return text.ToString();
    }
}
