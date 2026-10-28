namespace HCDE.Scripting;

public sealed class VmScriptException : Exception
{
    public VmScriptException(string scriptName, int line, string message)
        : base($"Script error, \"{scriptName}\" line {line}:\n{message}")
    {
        ScriptName = scriptName;
        Line = line;
    }

    public string ScriptName { get; }
    public int Line { get; }
}
