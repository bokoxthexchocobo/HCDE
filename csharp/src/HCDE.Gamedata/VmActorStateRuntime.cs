namespace HCDE.Gamedata;

public sealed class VmStateActionBinding
{
    public int ImplicitArgs { get; set; }
    public string? RequiredCallerType { get; set; }
    public bool DehackedState { get; set; }
}

/// <summary><c>FState::MatchesCallerType</c> / <c>CallAction</c> guard logic from <c>info.cpp</c>.</summary>
public static class VmActorStateRuntime
{
    public static bool MatchesCallerType(
        VmStateActionBinding? action,
        string? selfClass,
        string? stateOwnerClass)
    {
        if (action is null || action.ImplicitArgs < 1) return true;
        if (string.IsNullOrEmpty(action.RequiredCallerType)) return true;
        if (action.DehackedState) return true;
        if (!IsKindOf(selfClass, action.RequiredCallerType)) return false;
        if (action.ImplicitArgs >= 2)
            return IsKindOf(stateOwnerClass, action.RequiredCallerType);
        return true;
    }

    private static bool IsKindOf(string? instance, string required) =>
        !string.IsNullOrEmpty(instance) &&
        (instance.Equals(required, StringComparison.OrdinalIgnoreCase) ||
         instance.EndsWith(':' + required, StringComparison.OrdinalIgnoreCase));
}
