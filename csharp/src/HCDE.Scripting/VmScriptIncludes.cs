namespace HCDE.Scripting;

/// <summary>Native archive-relative ZScript include path rules.</summary>
public static class VmScriptIncludes
{
    public static IReadOnlyList<VmScriptInclude> Extract(string source, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceName);
        source = VmScriptText.Prepare(source);
        var includes = new List<VmScriptInclude>();
        var position = 0;
        var line = 1;
        var depth = 0;
        while (position < source.Length)
        {
            SkipTrivia(source, ref position, ref line);
            if (position == source.Length) break;
            if (source[position] is '"' or '\'') { ReadString(source, ref position, ref line); continue; }
            if (source[position] == '{') depth++;
            else if (source[position] == '}') depth--;
            if (source.AsSpan(position).StartsWith("#include", StringComparison.OrdinalIgnoreCase))
            {
                if (depth != 0) throw new FormatException("Script includes require global scope.");
                position += 8;
                SkipTrivia(source, ref position, ref line);
                var sourceLine = line;
                if (position == source.Length || source[position] != '"') throw new FormatException("Include requires a string constant.");
                var path = ReadString(source, ref position, ref line);
                SkipTrivia(source, ref position, ref line);
                while (position < source.Length && source[position] == '"')
                {
                    path += ReadString(source, ref position, ref line);
                    SkipTrivia(source, ref position, ref line);
                }
                includes.Add(new VmScriptInclude(ResolvePath(path, sourceName), sourceName, sourceLine));
                continue;
            }
            position++;
        }
        return includes.AsReadOnly();
    }

    private static string ReadString(string source, ref int position, ref int line)
    {
        var quote = source[position++];
        var start = position;
        while (position < source.Length)
        {
            if (quote == '"' && source[position] == '\\' && position + 1 < source.Length && source[position + 1] == quote) { position += 2; continue; }
            if (quote == '\'' && source[position] == '\n') throw new FormatException("Name constants cannot span lines.");
            if (source[position] == quote)
            {
                var value = source[start..position];
                position++;
                return value;
            }
            if (source[position++] == '\n') line++;
        }
        throw new FormatException("Unterminated script string constant.");
    }

    private static void SkipTrivia(string source, ref int position, ref int line)
    {
        while (position < source.Length)
        {
            if (source[position] <= ' ') { if (source[position++] == '\n') line++; continue; }
            if (source.AsSpan(position).StartsWith("//", StringComparison.Ordinal)
                || source.AsSpan(position).StartsWith("#region", StringComparison.Ordinal)
                || source.AsSpan(position).StartsWith("#endregion", StringComparison.Ordinal))
            {
                while (position < source.Length && source[position] != '\n') position++;
                continue;
            }
            if (!source.AsSpan(position).StartsWith("/*", StringComparison.Ordinal)) break;
            var end = source.IndexOf("*/", position + 2, StringComparison.Ordinal);
            if (end < 0) throw new FormatException("Unterminated script comment.");
            while (position < end + 2) if (source[position++] == '\n') line++;
        }
    }

    public static string ResolvePath(string path, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(sourceName);
        if (!path.StartsWith("./", StringComparison.Ordinal) && !path.StartsWith("../", StringComparison.Ordinal)) return path;
        var separator = sourceName.LastIndexOf(':');
        if (separator < 0) return path;
        var end = sourceName.LastIndexOf('/');
        var topLevel = separator > end;
        var folder = topLevel ? "" : sourceName[(separator + 1)..end];
        var relative = path.StartsWith("./", StringComparison.Ordinal) ? path[2..] : path;
        while (relative.StartsWith("../", StringComparison.Ordinal))
        {
            relative = relative[3..];
            var slash = folder.LastIndexOf('/');
            if (slash >= 0) folder = folder[..slash];
            else if (folder.Length != 0) folder = "";
            else return path;
        }
        // Preserve the native leading slash when a nested include ascends to root.
        return topLevel ? relative : folder + "/" + relative;
    }
}
