namespace HCDE.Playsim;

/// <summary>
/// Console command tokens from <c>c_commandline.cpp</c> (<b>1–195</b>).
/// Chunk <b>1193</b>. <c>FindCVarSub</c> is a host lookup. Character codes 128–255
/// compare as signed <c>char</c> values, so outside quotes they are whitespace.
/// </summary>
public static class VmCommandLine1193Slice
{
    public const char TextColorEscape = '\u001c';

    public static Func<string, string?> FindCVar = _ => null;

    public static void Clear()
    {
        FindCVar = _ => null;
    }

    public sealed class CommandLine
    {
        public readonly string Command;
        public readonly bool NoEscapes;
        public int ArgcValue = -1;
        public string[]? Argv;
        public int ArgSize;

        public CommandLine(string commandLine, bool noEscapes)
        {
            Command = commandLine;
            NoEscapes = noEscapes;
            ArgcValue = -1;
            Argv = null;
        }

        public string Args()
        {
            return Command;
        }

        public int Argc()
        {
            if (ArgcValue == -1)
            {
                var counted = Parse(Command, NoEscapes);
                ArgcValue = counted.Count;
                ArgSize = counted.Size;
            }

            return ArgcValue;
        }

        public string this[int index]
        {
            get
            {
                if (Argv == null)
                {
                    Argc();
                    Argv = Parse(Command, NoEscapes).Tokens.ToArray();
                }

                return Argv[index];
            }
        }

        public void Shift()
        {
            if (Argv == null)
                return;
            for (var i = 1; i < ArgcValue; i++)
                Argv[i - 1] = Argv[i];
        }

        public void Destroy()
        {
            Argv = null;
        }
    }

    public readonly struct Parsed
    {
        public Parsed(List<string> tokens, int size)
        {
            Tokens = tokens;
            Size = size;
            Count = tokens.Count;
        }

        public List<string> Tokens { get; }
        public int Size { get; }
        public int Count { get; }
    }

    public static CommandLine Create(string commandLine, bool noEscapes)
    {
        return new CommandLine(commandLine, noEscapes);
    }

    public static Parsed Parse(string args, bool noEscapes)
    {
        var tokens = new List<string>();
        var size = 0;
        var i = 0;
        while (true)
        {
            while (i < args.Length && IsSpace(args[i]))
                i++;
            if (i >= args.Length)
                break;
            if (args[i] == '"')
            {
                i++;
                var built = new System.Text.StringBuilder();
                char stuff;
                do
                {
                    if (i >= args.Length)
                        stuff = '\0';
                    else
                        stuff = args[i++];
                    if (!noEscapes && stuff == '\\' && i < args.Length && args[i] == '"')
                    {
                        stuff = '"';
                        i++;
                    }
                    else if (!noEscapes && stuff == '\\' && i < args.Length && args[i] == '\\')
                    {
                        i++;
                    }
                    else if (!noEscapes && stuff == '\\' && i < args.Length && args[i] == 'c')
                    {
                        stuff = TextColorEscape;
                        i++;
                    }
                    else if (stuff == '"')
                    {
                        stuff = '\0';
                    }

                    size++;
                    if (stuff != 0)
                        built.Append(stuff);
                }
                while (stuff != 0);
                tokens.Add(built.ToString());
            }
            else
            {
                var start = i;
                i++;
                while (i < args.Length && args[i] > ' ' && args[i] != '"' && args[i] < 128)
                    i++;
                string text;
                if (args[start] == '$')
                {
                    var name = args.Substring(start + 1, i - (start + 1));
                    var value = FindCVar(name);
                    text = value ?? args.Substring(start, i - start);
                }
                else
                {
                    text = args.Substring(start, i - start);
                }

                tokens.Add(text);
                size += text.Length + 1;
            }
        }

        return new Parsed(tokens, size);
    }

    static bool IsSpace(char value)
    {
        if (value == 0)
            return false;
        var signed = value < 128 ? value : value - 256;
        return signed <= ' ';
    }
}
