namespace HCDE.Scripting;

/// <summary>A managed VM callback with integer parameters and results.</summary>
public sealed class VmIntegerFunction
{
    private readonly Func<IReadOnlyList<int>, int, IReadOnlyList<int>> _callback;
    public string PrintableName { get; }
    internal int[] DefaultArguments { get; }

    public VmIntegerFunction(Func<IReadOnlyList<int>, int, IReadOnlyList<int>> callback, string printableName = "",
        IReadOnlyList<int>? defaultArguments = null)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ArgumentNullException.ThrowIfNull(printableName);
        PrintableName = printableName;
        _callback = callback;
        DefaultArguments = defaultArguments?.ToArray() ?? Array.Empty<int>();
        if (DefaultArguments.Length > 256) throw new ArgumentException("Too many default argument slots.", nameof(defaultArguments));
    }

    internal IReadOnlyList<int> Invoke(IReadOnlyList<int> parameters, int requestedReturns)
        => _callback(parameters, requestedReturns);
}
