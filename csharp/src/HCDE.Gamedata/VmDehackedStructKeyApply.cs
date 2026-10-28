namespace HCDE.Gamedata;

public sealed class VmDehackedStructKey
{
    public required string Name { get; init; }
    public required Action<int> Setter { get; init; }
}

/// <summary><c>HandleKey</c> offset writes from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedStructKeyApply
{
    public static bool TryApply(IEnumerable<VmDehackedStructKey> keys, string key, int value)
    {
        foreach (var entry in keys)
        {
            if (!entry.Name.Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
            entry.Setter(value);
            return true;
        }

        return false;
    }
}
