using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>Supported V_GetColor syntax using the bundled native X11R6RGB names.</summary>
public static class NativeColorParser
{
    private static readonly Lazy<IReadOnlyDictionary<string, uint>> Names = new(ReadNames);

    public static bool TryParse(string text, out uint color, IReadOnlyDictionary<string, uint>? colorNames = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        if ((colorNames ?? Names.Value).TryGetValue(text, out color)) return true;
        if (text.StartsWith('#'))
        {
            if (text.Length == 7) return uint.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color);
            if (text.Length == 4)
            {
                if (!uint.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb)) return false;
                color = ((rgb & 0xf00) << 8 | (rgb & 0x0f0) << 4 | (rgb & 0x00f)) * 17;
                return true;
            }
            color = 0; // Native explicitly interprets other HTML color lengths as black.
            return true;
        }
        var parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1 && parts[0].Length == 6)
            return uint.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color);
        if (parts.Length == 3
            && byte.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red)
            && byte.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green)
            && byte.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue))
        {
            color = (uint)(red << 16 | green << 8 | blue);
            return true;
        }
        color = 0;
        return false;
    }

    private static IReadOnlyDictionary<string, uint> ReadNames()
    {
        using var stream = typeof(NativeColorParser).Assembly.GetManifestResourceStream("HCDE.Gamedata.X11R6RGB")
            ?? throw new InvalidOperationException("Bundled native color table is missing.");
        using var reader = new StreamReader(stream);
        if (!TryReadNames(reader.ReadToEnd(), out var names, out var error))
            throw new InvalidDataException(error);
        return names;
    }

    public static bool TryReadNames(string table, out IReadOnlyDictionary<string, uint> colors, out string? error)
    {
        ArgumentNullException.ThrowIfNull(table);
        var names = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        colors = names; error = null;
        // The bundled native text asset includes a trailing C-string terminator.
        foreach (var line in table.Split('\0')[0].Split('\n'))
        {
            var text = line.Trim();
            if (text.Length == 0 || text.StartsWith('!')) continue;
            var parts = text.Split((char[]?)null, 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4
                || !byte.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var red)
                || !byte.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var green)
                || !byte.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var blue))
            {
                colors = new Dictionary<string, uint>(); error = "Invalid X11R6RGB color table row.";
                return false;
            }
            names.TryAdd(parts[3], (uint)(red << 16 | green << 8 | blue));
        }
        return true;
    }
}
