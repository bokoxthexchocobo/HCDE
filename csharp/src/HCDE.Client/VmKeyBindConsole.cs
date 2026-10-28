namespace HCDE.Client;

/// <summary>Global bind stacks from <c>c_bind.cpp</c> (Bindings, DoubleBindings, AutomapBindings).</summary>
public sealed class VmKeyBindConsole
{
    public VmKeyBindings Bindings { get; } = new();
    public VmKeyBindings DoubleBindings { get; } = new();
    public VmKeyBindings AutomapBindings { get; } = new();

    public void UnbindAllStacks(IReadOnlyList<int>? filter = null)
    {
        Bindings.UnbindAll(filter);
        DoubleBindings.UnbindAll(filter);
        AutomapBindings.UnbindAll(filter);
    }

    public void PerformBind(IReadOnlyList<string> argv, Action<string>? writeLine = null) =>
        Bindings.PerformBind(argv, writeLine);

    public void PerformDoubleBind(IReadOnlyList<string> argv, Action<string>? writeLine = null) =>
        DoubleBindings.PerformBind(argv, writeLine);

    public void PerformMapBind(IReadOnlyList<string> argv, Action<string>? writeLine = null) =>
        AutomapBindings.PerformBind(argv, writeLine);
}
