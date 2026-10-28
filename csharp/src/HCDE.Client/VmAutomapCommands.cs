namespace HCDE.Client;

/// <summary>Automap CCMD helpers from <c>am_map.cpp</c> (200–260).</summary>
public static class VmAutomapCommands
{
    public const string DefaultMarkFont = "AMMNUMx";

    public static bool ToggleBool(ref bool value)
    {
        value = !value;
        return value;
    }

    public static bool ShouldRunToggleMap(int gameAction) => gameAction == 0;

    public static float ParseZoomDir(string? arg, float current) =>
        float.TryParse(arg, out var parsed) ? parsed : current;
}
