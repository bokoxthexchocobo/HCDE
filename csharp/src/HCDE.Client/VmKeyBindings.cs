namespace HCDE.Client;

/// <summary>Managed <c>FKeyBindings</c> from <c>c_bind.cpp</c>.</summary>
public sealed class VmKeyBindings
{
    private readonly Dictionary<int, string> binds = new();

    public void DoBind(string key, string command)
    {
        var keynum = VmKeyNames.GetConfigKeyFromName(key);
        if (keynum != 0) binds[keynum] = command;
    }

    public void UnbindAll(IReadOnlyList<int>? filter = null)
    {
        if (filter is null)
        {
            binds.Clear();
            return;
        }

        foreach (var key in filter) binds.Remove(key);
    }

    public void UnbindACommand(string command)
    {
        foreach (var key in binds.Keys.ToList())
        {
            if (string.Equals(binds[key], command, StringComparison.OrdinalIgnoreCase))
                binds.Remove(key);
        }
    }

    public void DefaultBind(string keyName, string command)
    {
        var key = VmKeyNames.GetKeyFromName(keyName);
        if (key == 0) return;
        if (binds.ContainsKey(key)) return;
        if (binds.Values.Any(v => string.Equals(v, command, StringComparison.OrdinalIgnoreCase))) return;
        binds[key] = command;
    }

    public string? GetBind(int key) => binds.TryGetValue(key, out var cmd) ? cmd : null;

    public void LoadFromConfig(VmConfigFile config)
    {
        binds.Clear();
        while (config.NextInSection(out var key, out var value))
            DoBind(key, value);
    }

    public void ArchiveBindings(VmConfigFile config, string? matchCommand = null)
    {
        for (var i = 0; i < VmKeyNames.NumKeys; i++)
        {
            if (!binds.TryGetValue(i, out var cmd) || cmd.Length == 0)
            {
                if (matchCommand is null) config.ClearKey(VmKeyNames.ConfigKeyName(i));
                continue;
            }

            if (matchCommand is null || string.Equals(cmd, matchCommand, StringComparison.OrdinalIgnoreCase))
            {
                if (cmd[0] == '\u0001')
                {
                    binds.Remove(i);
                    continue;
                }

                config.SetValueForKey(VmKeyNames.ConfigKeyName(i), cmd);
                if (matchCommand is not null) binds[i] = "\u0001";
            }
        }
    }
}
