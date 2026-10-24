namespace HCDE.Playsim;

/// <summary>Native <c>GetVarAddrType</c>, <c>SetUserVariable</c>, and <c>GetUserVariable</c> (int slots) from <c>p_acs.cpp</c>.</summary>
internal static class AcsScriptUserVariableService
{
    public static bool IsValidIndex(Actor actor, string name, int index)
    {
        if (string.IsNullOrEmpty(name) || index < 0)
            return false;
        if (actor.ScriptUserVariables is not { } table || !table.TryGetValue(name, out var slots))
            return index == 0;
        return (uint)index < (uint)slots.Length;
    }

    public static void Write(Actor actor, string name, int index, int value) =>
        actor.SetScriptUserVar(name, index, value);

    public static int Read(Actor actor, string name, int index) =>
        actor.GetScriptUserVar(name, index);
}
