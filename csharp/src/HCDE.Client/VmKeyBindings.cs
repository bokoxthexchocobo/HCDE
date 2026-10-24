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

    public string GetBinding(int key) => GetBind(key) ?? "";

    public void SetBind(int key, string command, bool overwrite = true)
    {
        if (key == 0) return;
        if (!overwrite && binds.ContainsKey(key)) return;
        binds[key] = command;
    }

    public string? GetBind(string keyName)
    {
        var key = VmKeyNames.GetKeyFromName(keyName);
        return key == 0 ? null : GetBind(key);
    }

    public void UnbindKey(string key)
    {
        var i = VmKeyNames.GetKeyFromName(key);
        if (i != 0) binds.Remove(i);
    }

    public int GetKeysForCommand(string cmd, out int first, out int second)
    {
        first = second = 0;
        if (string.IsNullOrEmpty(cmd)) return 0;
        var count = 0;
        for (var i = 0; i < VmKeyNames.NumKeys && count < 2; i++)
        {
            if (!binds.TryGetValue(i, out var bound) || !string.Equals(bound, cmd, StringComparison.OrdinalIgnoreCase))
                continue;
            if (count++ == 0) first = i;
            else second = i;
        }

        return count;
    }

    public IReadOnlyList<int> GetKeysForCommand(string cmd)
    {
        var result = new List<int>();
        if (string.IsNullOrEmpty(cmd)) return result;
        for (var i = 0; i < VmKeyNames.NumKeys; i++)
        {
            if (binds.TryGetValue(i, out var bound) && string.Equals(bound, cmd, StringComparison.OrdinalIgnoreCase))
                result.Add(i);
        }

        return result;
    }

    public void LoadFromConfig(VmConfigFile config)
    {
        binds.Clear();
        while (config.NextInSection(out var key, out var value))
            DoBind(key, value);
    }

    public void PerformBind(IReadOnlyList<string> argv, Action<string>? writeLine = null)
    {
        writeLine ??= _ => { };
        if (argv.Count > 1)
        {
            var key = VmKeyNames.GetKeyFromName(argv[1]);
            if (key == 0)
            {
                writeLine($"Unknown key \"{argv[1]}\"");
                return;
            }

            if (argv.Count == 2)
                writeLine($"\"{argv[1]}\" = \"{GetBind(key) ?? ""}\"");
            else
                binds[key] = argv[2];
            return;
        }

        writeLine("Current key bindings:");
        for (var i = 0; i < VmKeyNames.NumKeys; i++)
        {
            if (binds.TryGetValue(i, out var cmd) && cmd.Length > 0)
                writeLine($"{VmKeyNames.KeyName(i)} \"{cmd}\"");
        }
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
