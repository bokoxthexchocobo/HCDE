namespace HCDE.Scripting;

/// <summary>An explicit VM abort raised by a converted native callback.</summary>
public sealed class VmAbortException : Exception
{
    public ushort Reason { get; }
    public string? Context { get; }

    public VmAbortException(ushort reason, string? context = null)
        : base(VmAbortDiagnostics.FormatMessage(reason, context))
        => (Reason, Context) = (reason, context);
}
