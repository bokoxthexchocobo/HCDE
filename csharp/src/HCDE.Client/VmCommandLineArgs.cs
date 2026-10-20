namespace HCDE.Client;

/// <summary>Native <c>m_argv.cpp</c> <c>FArgs</c> lookup helpers.</summary>
public sealed class VmCommandLineArgs
{
    private readonly List<string> argv;

    public VmCommandLineArgs(IEnumerable<string> arguments)
    {
        argv = arguments?.ToList() ?? throw new ArgumentNullException(nameof(arguments));
    }

    public int NumArgs => argv.Count;

    public string? GetArg(int index) => (uint)index < (uint)argv.Count ? argv[index] : null;

    public int CheckParm(string name, int start = 1)
    {
        for (var index = start; index < argv.Count; index++)
        {
            if (string.Equals(argv[index], name, StringComparison.OrdinalIgnoreCase))
                return index;
        }

        return 0;
    }

    public int CheckParmList(string name, out IReadOnlyList<string> values, int start = 1)
    {
        values = Array.Empty<string>();
        var at = CheckParm(name, start);
        if (at == 0)
            return 0;
        var end = at + 1;
        while (end < argv.Count && argv[end][0] is not '-' and not '+')
            end++;
        values = argv.GetRange(at + 1, end - at - 1);
        return values.Count;
    }

    public string? CheckValue(string name)
    {
        var index = CheckParm(name);
        if (index <= 0 || index >= argv.Count - 1)
            return null;
        var value = argv[index + 1];
        return value.Length > 0 && value[0] is not '+' and not '-' ? value : null;
    }

    public string TakeValue(string name)
    {
        var index = CheckParm(name);
        if (index <= 0 || index >= argv.Count)
            return "";
        if (index < argv.Count - 1 && argv[index + 1][0] is not '+' and not '-')
        {
            var value = argv[index + 1];
            argv.RemoveRange(index, 2);
            return value;
        }

        argv.RemoveAt(index);
        return "";
    }
}
