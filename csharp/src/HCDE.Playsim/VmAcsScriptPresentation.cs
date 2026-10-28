namespace HCDE.Playsim;

/// <summary><c>ScriptPresentation</c> from <c>p_acs.cpp</c> (1508–1523).</summary>
public static class VmAcsScriptPresentation
{
    public static string FormatScriptNumber(int script) => $"script {script}";

    public static string FormatNamedScript(string name) => $"script \"{name}\"";

    public static string PresentNative(int script, Func<int, string?> resolveNamedScript)
    {
        if (script < 0)
        {
            var name = resolveNamedScript(script);
            if (!string.IsNullOrEmpty(name))
                return FormatNamedScript(name);
        }

        return FormatScriptNumber(script);
    }
}
