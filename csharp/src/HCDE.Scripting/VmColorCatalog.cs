using System.Globalization;

namespace HCDE.Scripting;

/// <summary>Color names supplied by an archive's native X11R6RGB resource.</summary>
public sealed class VmColorCatalog
{
    private readonly Dictionary<string, int> colors = new(StringComparer.OrdinalIgnoreCase);

    public static VmColorCatalog Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var catalog = new VmColorCatalog();
        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('!')) continue;
            var position = 0;
            var color = 0;
            for (var component = 0; component < 3; component++)
            {
                while (position < line.Length && line[position] <= ' ') position++;
                var start = position;
                while (position < line.Length && char.IsAsciiDigit(line[position])) position++;
                if (!int.TryParse(line.AsSpan(start, position - start), NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                    || value is < 0 or > 255)
                    throw new FormatException("Invalid X11R6RGB component.");
                color = (color << 8) | value;
            }
            var name = line[position..].Trim();
            if (name.Length == 0) throw new FormatException("Missing X11R6RGB color name.");
            // Native lookup selects the first matching entry.
            catalog.colors.TryAdd(name, color);
        }
        return catalog;
    }

    public bool TryGetColor(string name, out int color)
    {
        var terminator = name.IndexOf('\0');
        return colors.TryGetValue(terminator >= 0 ? name[..terminator] : name, out color);
    }
}
