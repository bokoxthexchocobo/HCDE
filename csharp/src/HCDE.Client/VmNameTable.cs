namespace HCDE.Client;

/// <summary><c>FName::NameManager::FindName</c> subset from <c>name.cpp</c>.</summary>
public sealed class VmNameTable
{
    private readonly Dictionary<string, int> stringToIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> indexToString = new();

    public VmNameTable(IEnumerable<string>? predefined = null)
    {
        Register("None");
        if (predefined is null) return;
        foreach (var name in predefined) Register(name);
    }

    public int FindName(string? text, bool noCreate = false)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (stringToIndex.TryGetValue(text, out var index)) return index;
        if (noCreate) return 0;
        return Register(text);
    }

    public int FindName(ReadOnlySpan<char> text, bool noCreate = false) =>
        FindName(text.ToString(), noCreate);

    public string GetString(int index) => index >= 0 && index < indexToString.Count ? indexToString[index] : "";

    private int Register(string text)
    {
        var index = indexToString.Count;
        indexToString.Add(text);
        stringToIndex[text] = index;
        return index;
    }
}
