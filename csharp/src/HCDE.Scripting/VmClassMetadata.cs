namespace HCDE.Scripting;

public enum VmClassScope { PlainData, UI, Play }

/// <summary>Metadata allocations and virtual method slots for converted runtime classes.</summary>
public sealed class VmClassMetadata
{
    private readonly Dictionary<Type, object?> _allocations;
    private readonly Dictionary<Type, object?[]> _virtualMethods = new();
    private readonly Dictionary<Type, VmClassScope> _scopes = new();

    public VmClassMetadata(IReadOnlyDictionary<Type, object?> allocations,
        IReadOnlyDictionary<Type, IReadOnlyList<object?>>? virtualMethods = null,
        IReadOnlyDictionary<Type, VmClassScope>? scopes = null,
        IReadOnlyDictionary<Type, IReadOnlyDictionary<int, object?>>? virtualOverrides = null)
    {
        ArgumentNullException.ThrowIfNull(allocations);
        _allocations = new Dictionary<Type, object?>();
        foreach (var entry in allocations)
        {
            if (!entry.Key.IsClass) throw new ArgumentException("Metadata requires a class descriptor.", nameof(allocations));
            if (entry.Value is not null && entry.Value is not byte[] && entry.Value is not VmStringMemory && entry.Value is not VmObjectMemory)
                throw new ArgumentException("Unsupported metadata allocation.", nameof(allocations));
            _allocations.Add(entry.Key, entry.Value);
        }
        if (virtualMethods is not null)
            foreach (var entry in virtualMethods)
            {
                if (!entry.Key.IsClass) throw new ArgumentException("Virtual methods require a class descriptor.", nameof(virtualMethods));
                ArgumentNullException.ThrowIfNull(entry.Value);
                if (entry.Value.Any(method => method is not null && method is not VmFunction && method is not VmIntegerFunction && method is not VmScriptFunction))
                    throw new ArgumentException("Unsupported virtual method.", nameof(virtualMethods));
                _virtualMethods.Add(entry.Key, entry.Value.ToArray());
            }
        if (scopes is not null)
            foreach (var entry in scopes)
            {
                if (!entry.Key.IsClass || !Enum.IsDefined(entry.Value)) throw new ArgumentException("Invalid class scope.", nameof(scopes));
                _scopes.Add(entry.Key, entry.Value);
            }
        var overrides = new Dictionary<Type, Dictionary<int, object?>>();
        if (virtualOverrides is not null)
            foreach (var entry in virtualOverrides)
            {
                if (!entry.Key.IsClass) throw new ArgumentException("Virtual overrides require a class descriptor.", nameof(virtualOverrides));
                ArgumentNullException.ThrowIfNull(entry.Value);
                var slots = new Dictionary<int, object?>();
                foreach (var slot in entry.Value)
                {
                    if (slot.Key < 0 || slot.Key > 255 || slot.Value is not null && slot.Value is not VmFunction && slot.Value is not VmIntegerFunction && slot.Value is not VmScriptFunction)
                        throw new ArgumentException("Invalid virtual override slot.", nameof(virtualOverrides));
                    slots.Add(slot.Key, slot.Value);
                }
                overrides.Add(entry.Key, slots);
            }
        foreach (var runtimeClass in _allocations.Keys.Concat(_scopes.Keys).Concat(_virtualMethods.Keys).Concat(overrides.Keys)
            .Distinct().OrderBy(InheritanceDepth).ToArray())
        {
            if (!_virtualMethods.ContainsKey(runtimeClass))
                for (var parent = runtimeClass.BaseType; parent is not null; parent = parent.BaseType)
                {
                    if (!_virtualMethods.TryGetValue(parent, out var inheritedMethods)) continue;
                    _virtualMethods.Add(runtimeClass, inheritedMethods.ToArray());
                    break;
                }
            if (overrides.TryGetValue(runtimeClass, out var classOverrides))
            {
                var methods = _virtualMethods.TryGetValue(runtimeClass, out var existing) ? existing.ToList() : new List<object?>();
                foreach (var slot in classOverrides.OrderBy(slot => slot.Key))
                {
                    if (slot.Key > methods.Count) throw new ArgumentException("New virtual slots must be contiguous.", nameof(virtualOverrides));
                    if (slot.Key == methods.Count) methods.Add(slot.Value);
                    else methods[slot.Key] = slot.Value;
                }
                _virtualMethods[runtimeClass] = methods.ToArray();
            }
        }
    }

    private static int InheritanceDepth(Type type)
    {
        var depth = 0;
        for (var parent = type.BaseType; parent is not null; parent = parent.BaseType) depth++;
        return depth;
    }

    internal bool TryGet(Type runtimeClass, out object? allocation) => _allocations.TryGetValue(runtimeClass, out allocation);
    internal bool TryGetScope(Type runtimeClass, out VmClassScope scope) => _scopes.TryGetValue(runtimeClass, out scope);

    internal bool TryGetVirtual(Type runtimeClass, int slot, out object? method)
    {
        method = null;
        if (!_virtualMethods.TryGetValue(runtimeClass, out var methods) || slot >= methods.Length) return false;
        method = methods[slot];
        return true;
    }
}
