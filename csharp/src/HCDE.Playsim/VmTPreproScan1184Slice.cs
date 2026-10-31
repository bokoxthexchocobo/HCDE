namespace HCDE.Playsim;

/// <summary>
/// FraggleScript preprocess scan from <c>t_prepro.cpp</c> (<b>213–425</b>). Chunk <b>1184</b>.
/// Section hash lookup stays in chunk 1183.
/// </summary>
public static class VmTPreproScan1184Slice
{
    public const int StEmpty = 0;
    public const int StIf = 1;
    public const int StElseIf = 2;
    public const int StElse = 3;
    public const int StLoop = 4;
    public const int TokenFunction = 5;
    public const int SvtLabel = 4;

    public sealed class ScriptFault : Exception
    {
        public ScriptFault(string message) : base(message)
        {
            Detail = message;
        }

        public string Detail { get; }
    }

    public sealed class Label
    {
        public string Name = "";
        public int Type;
        public int IntValue;
    }

    public sealed class Script
    {
        public char[] Data = Array.Empty<char>();
        public int Len;
        public int ScriptNum;
        public VmTPreproSections1183Slice.Script Sections = new();
        public List<Label> Labels { get; } = new();
        public List<string> Bold { get; } = new();
        public List<string> Errors { get; } = new();
        public List<string> Fatals { get; } = new();
    }

    public sealed class TokenFrame
    {
        public int NumTokens;
        public int TokenType0;
        public string Token0 = "";
        public VmTPreproSections1183Slice.Section? Section;
        public int LineStart;
    }

    public delegate int TokenSource(Script script, int rover, TokenFrame frame);

    public sealed class Files
    {
        public Func<string, int> CheckNumForName = _ => -1;
        public Func<int, int> FileLength = _ => 0;
        public Action<int, byte[]> ReadFile = (_, _) => { };
        public List<(char[] Data, int Start, int End)> Runs { get; } = new();
    }

    public static int? ProcessFindChar(Script script, int index, char find)
    {
        var data = script.Data;
        while (index < data.Length && data[index] != '\0')
        {
            if (data[index] == find)
                return index;
            if (data[index] == '"')
            {
                index++;
                while (index < data.Length && data[index] != '\0' && data[index] != '"')
                {
                    if (data[index] == '\\' && index + 1 < data.Length)
                        index++;
                    index++;
                }

                if (index >= data.Length || data[index] == '\0')
                    return null;
            }

            if (index + 1 < data.Length && data[index] == '/' && data[index + 1] == '*')
            {
                while (index < data.Length && data[index] != '\0'
                    && !(data[index] == '*' && index + 1 < data.Length && data[index + 1] == '/'))
                {
                    data[index] = ' ';
                    index++;
                }

                if (index < data.Length && data[index] != '\0')
                {
                    data[index] = ' ';
                    data[index + 1] = ' ';
                }
                else
                    throw new ScriptFault("script terminated inside comment\n");
            }

            if (index + 1 < data.Length && data[index] == '/' && data[index + 1] == '/')
            {
                while (index < data.Length && data[index] != '\n')
                {
                    data[index] = ' ';
                    index++;
                }
            }

            if (index < data.Length && data[index] == ':' && script.ScriptNum != -1)
                NoteLabel(script, index);

            if (index < data.Length && data[index] == '{')
            {
                var section = VmTPreproSections1183Slice.NewSection(script.Sections, index);
                section.Type = StEmpty;
                var theEnd = ProcessFindChar(script, index + 1, '}');
                if (theEnd == null)
                    throw new ScriptFault("Script " + script.ScriptNum + ": section error: no ending brace\n");
                section.EndIndex = theEnd.Value;
                index = theEnd.Value;
            }

            index++;
        }

        return null;
    }

    public static void DryRun(Script script, TokenSource tokens)
    {
        var rover = 0;
        while (rover < script.Len && rover < script.Data.Length && script.Data[rover] != '\0')
        {
            var frame = new TokenFrame();
            try
            {
                rover = tokens(script, rover, frame);
            }
            catch (ScriptFault fault)
            {
                script.Errors.Add(ErrorMessage(script, rover, frame.LineStart, fault.Message));
                break;
            }

            if (frame.NumTokens == 0)
                continue;
            if (frame.Section == null || frame.TokenType0 != TokenFunction)
                continue;
            if (frame.Token0 == "if")
                frame.Section.Type = StIf;
            else if (frame.Token0 == "elseif")
                frame.Section.Type = StElseIf;
            else if (frame.Token0 == "else")
                frame.Section.Type = StElse;
            else if (frame.Token0 == "while" || frame.Token0 == "for")
            {
                frame.Section.Type = StLoop;
                frame.Section.LoopIndex = frame.LineStart;
            }
        }
    }

    public static void Preprocess(Script script, TokenSource tokens)
    {
        script.Len = script.Data.Length - 1;
        ProcessFindChar(script, 0, '\0');
        DryRun(script, tokens);
    }

    public static void ParseInclude(Script script, Files files, string lumpName)
    {
        var lump = files.CheckNumForName(lumpName);
        if (lump == -1)
        {
            script.Fatals.Add("include lump '" + lumpName + "' not found!\n");
            return;
        }

        var length = files.FileLength(lump);
        var bytes = new byte[length + 10];
        files.ReadFile(lump, bytes);
        var chars = new char[length + 10];
        for (var i = 0; i < length; i++)
            chars[i] = (char)(bytes[i] & 0xFF);
        chars[length] = '\0';
        var saved = script.Data;
        try
        {
            script.Data = chars;
            ProcessFindChar(script, 0, '\0');
        }
        finally
        {
            script.Data = saved;
        }

        files.Runs.Add((chars, 0, length));
    }

    public static string ErrorMessage(Script script, int rover, int lineStart, string message)
    {
        var line = 0;
        if (rover >= 0 && rover <= script.Len)
        {
            var limit = lineStart;
            if (limit > script.Data.Length)
                limit = script.Data.Length;
            for (var i = 0; i < limit; i++)
            {
                if (script.Data[i] == '\n')
                    line++;
            }
        }

        return "Script " + script.ScriptNum + ", line " + line + ": " + message;
    }

    static void NoteLabel(Script script, int colon)
    {
        var data = script.Data;
        var labelPtr = colon - 1;
        while (labelPtr >= 0 && !IsOp(data[labelPtr]))
            labelPtr--;
        var start = labelPtr + 1;
        var name = new string(data, start, colon - start);
        if (name.Length == 0)
            script.Bold.Add("Script " + script.ScriptNum + ": ':' encountrered in incorrect position!\n");
        script.Labels.Add(new Label { Name = name, Type = SvtLabel, IntValue = labelPtr });
    }

    static bool IsOp(char c)
    {
        var letter = (c <= 'Z' && c >= 'A') || (c <= 'z' && c >= 'a');
        var digit = c <= '9' && c >= '0';
        if (letter || digit || c == '_')
            return false;
        return true;
    }
}
