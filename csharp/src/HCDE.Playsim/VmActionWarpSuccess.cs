namespace HCDE.Playsim;

/// <summary><c>A_Warp</c> success state / chain result from <c>p_actionfunctions.cpp</c> (3732–3747).</summary>
public static class VmActionWarpSuccess
{
    public static bool ShouldSetSuccessStatePointer(bool warpOk, object? successState, bool hasStateSlot) =>
        warpOk && successState is not null && hasStateSlot;

    public static bool ShouldSetChainSuccess(bool warpOk, object? successState, bool hasChainSlot) =>
        warpOk && successState is null && hasChainSlot;
}
