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

    public int CheckParm(IReadOnlyList<string> names, int start = 1)
    {
        for (var index = start; index < argv.Count; index++)
        {
            foreach (var name in names)
            {
                if (string.Equals(argv[index], name, StringComparison.OrdinalIgnoreCase))
                    return index;
            }
        }

        return 0;
    }

    public void AppendRawArg(string arg) => argv.Add(arg);

    public void RemoveArg(int argIndex)
    {
        if ((uint)argIndex < (uint)argv.Count) argv.RemoveAt(argIndex);
    }

    public void RemoveArgs(string name)
    {
        var index = CheckParm(name);
        if (index <= 0 || index >= argv.Count - 1) return;
        do RemoveArg(index);
        while (index < argv.Count && argv[index][0] is not '+' and not '-');
    }

    public IReadOnlyList<string> Arguments => argv;

    public void AppendRawArgsString(string argsLine)
    {
        var fileIndex = argv.FindIndex(a => string.Equals(a, "-file", StringComparison.OrdinalIgnoreCase));
        if (fileIndex < 0)
        {
            argv.Add("-file");
            fileIndex = argv.Count - 1;
        }

        var filesEnd = fileIndex + 1;
        while (filesEnd < argv.Count && argv[filesEnd][0] is not '-' and not '+') filesEnd++;
        var insertingFile = true;
        argsLine = argsLine.Trim();
        var index = 0;
        while (index < argsLine.Length)
        {
            while (index < argsLine.Length && argsLine[index] == ' ') index++;
            if (index >= argsLine.Length) break;
            var start = index;
            if (argsLine[index] is '"' or '\'')
            {
                var quote = argsLine[index++];
                start = index;
                while (index < argsLine.Length && argsLine[index] != quote) index++;
                var quoted = argsLine[start..index];
                if (index < argsLine.Length) index++;
                InsertParsedArg(quoted, ref insertingFile, ref fileIndex, ref filesEnd);
                continue;
            }

            while (index < argsLine.Length && argsLine[index] != ' ') index++;
            var arg = argsLine[start..index];
            InsertParsedArg(arg, ref insertingFile, ref fileIndex, ref filesEnd);
        }
    }

    private void InsertParsedArg(string arg, ref bool insertingFile, ref int fileIndex, ref int filesEnd)
    {
        if (arg.Length == 0) return;
        if (arg[0] is '-' or '+') insertingFile = false;
        if (insertingFile)
        {
            argv.Insert(filesEnd++, arg);
        }
        else if (!string.Equals(arg, "-file", StringComparison.OrdinalIgnoreCase))
        {
            argv.Insert(fileIndex++, arg);
            filesEnd++;
        }
        else insertingFile = true;
    }
}
