namespace HCDE.Client;

/// <summary><c>FLatchedValue</c> / <c>UnlatchCVars</c> from <c>c_cvars.cpp</c>.</summary>
public sealed class VmCvarLatchBuffer
{
    private readonly List<Entry> entries = [];

    public void Push(VmTypedCvar variable, VmCvarValue value, VmCvarValueType type, bool unsafeContext)
    {
        entries.Add(new Entry(variable, value, type, unsafeContext));
    }

    public void Unlatch()
    {
        foreach (var entry in entries)
        {
            var flags = entry.Variable.Flags;
            if (entry.UnsafeContext) flags |= VmCvarFlags.UnsafeContext;
            else flags &= ~VmCvarFlags.UnsafeContext;
            entry.Variable.Flags = flags & ~(VmCvarFlags.Latch | VmCvarFlags.ServerInfo);
            entry.Variable.ForceSet(entry.Value, entry.Type);
            entry.Variable.Flags = flags;
        }

        entries.Clear();
    }

    public int Count => entries.Count;

    private readonly record struct Entry(
        VmTypedCvar Variable,
        VmCvarValue Value,
        VmCvarValueType Type,
        bool UnsafeContext);
}
