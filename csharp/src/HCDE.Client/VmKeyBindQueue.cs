namespace HCDE.Client;

/// <summary><c>C_TickQueuedInputs</c> / <c>C_DoKey</c> double-click queue subset.</summary>
public sealed class VmKeyBindQueue
{
    private readonly int numKeys;
    private readonly Func<uint> getTimeMs;
    private readonly bool[] queuedPresses;
    private readonly bool[] queuedPressReleases;
    private readonly bool[] doubleClickedKeys;
    private readonly uint[] doubleClickDeadline;
    private VmKeyBindings?[] queuedBindings = [];

    public VmKeyBindQueue(int numKeys, Func<uint> getTimeMs, int doubleClickThresholdMs = 250)
    {
        this.numKeys = numKeys;
        this.getTimeMs = getTimeMs;
        DoubleClickThresholdMs = doubleClickThresholdMs;
        queuedPresses = new bool[numKeys];
        queuedPressReleases = new bool[numKeys];
        doubleClickedKeys = new bool[numKeys];
        doubleClickDeadline = new uint[numKeys];
        queuedBindings = new VmKeyBindings?[numKeys];
    }

    public int DoubleClickThresholdMs { get; set; }

    public void Tick()
    {
        var now = getTimeMs();
        for (var key = 0; key < numKeys; key++)
        {
            if (!queuedPresses[key] || doubleClickDeadline[key] > now) continue;
            var binds = queuedBindings[key] ?? throw new InvalidOperationException("Queued bind without source.");
            var binding = binds.GetBinding(key);
            ClearQueuedPress(key);
            if (binding.Length > 0)
                PendingExecutions.Add(new VmKeyExec(key, binding, false, false));
        }
    }

    public List<VmKeyExec> PendingExecutions { get; } = [];

    public bool TryHandleKey(
        int key,
        bool isKeyUp,
        VmKeyBindings binds,
        VmKeyBindings? doubleBinds,
        out VmKeyExec? exec)
    {
        exec = null;
        if (key < 0 || key >= numKeys) return false;
        Tick();

        var hasDouble = doubleBinds is not null && doubleBinds.GetBinding(key).Length > 0;
        var hasQueued = queuedPresses[key] && queuedBindings[key] == binds;
        var now = getTimeMs();

        if (!isKeyUp)
        {
            if (!hasDouble)
            {
                var binding = binds.GetBinding(key);
                if (binding.Length == 0) return false;
                exec = new VmKeyExec(key, binding, false, false);
                return true;
            }

            if (hasQueued && doubleClickDeadline[key] > now)
            {
                var binding = doubleBinds!.GetBinding(key);
                ClearQueuedPress(key);
                doubleClickedKeys[key] = true;
                exec = new VmKeyExec(key, binding, false, true);
                return true;
            }

            queuedPresses[key] = true;
            queuedPressReleases[key] = false;
            queuedBindings[key] = binds;
            doubleClickDeadline[key] = now + (uint)DoubleClickThresholdMs;
            return true;
        }

        if (hasDouble && doubleClickedKeys[key])
        {
            doubleClickedKeys[key] = false;
            var binding = doubleBinds!.GetBinding(key);
            if (binding.Length == 0) return false;
            exec = new VmKeyExec(key, binding, true, true);
            return true;
        }

        if (hasQueued)
        {
            queuedPressReleases[key] = true;
            return true;
        }

        var normal = binds.GetBinding(key);
        if (normal.Length == 0 || normal[0] != '+') return false;
        exec = new VmKeyExec(key, normal, true, false);
        return true;
    }

    private void ClearQueuedPress(int key)
    {
        queuedPresses[key] = false;
        queuedPressReleases[key] = false;
        queuedBindings[key] = null;
        doubleClickDeadline[key] = 0;
    }
}

public readonly record struct VmKeyExec(int Key, string Binding, bool IsKeyUp, bool IsDoubleClick);
