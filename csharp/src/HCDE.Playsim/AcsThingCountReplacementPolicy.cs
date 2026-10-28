namespace HCDE.Playsim;

/// <summary>DECORATE replacement pass for ACS thing counting (native <c>GetReplacement</c> second pass).</summary>
public static class AcsThingCountReplacementPolicy
{
    private static readonly Dictionary<int, int> EditorReplacements = new()
    {
        // Example: Doom 2 plasma rifle pickup replacing the beta version in some mods is handled at spawn time;
        // keep empty until decorate replacement tables are ported. Hook exists for parity tests.
    };

    public static int? ResolveEditorNumber(int editorNumber)
    {
        if (EditorReplacements.TryGetValue(editorNumber, out var replacement) && replacement != editorNumber)
            return replacement;
        return null;
    }

    public static void RegisterReplacement(int fromEditorNumber, int toEditorNumber) =>
        EditorReplacements[fromEditorNumber] = toEditorNumber;
}
