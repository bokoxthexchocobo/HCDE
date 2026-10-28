namespace HCDE.Playsim;

/// <summary>Native <c>ACSF_GetActorViewHeight</c> (<c>p_acs.cpp</c>).</summary>
public static class AcsActorViewHeightService
{
    public static int Get(AuthoritySimulation sim, int tid, Actor? activator)
    {
        Actor? actor;
        if (tid == 0)
            actor = activator is { Destroyed: false } ? activator : null;
        else
            actor = AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed)
            return 0;
        var height = actor is PlayerPawn player
            ? player.DefaultViewHeight.ToDouble() * player.CrouchFactor
            : actor.GetCameraHeight();
        return AcsFixedPoint.FromDouble(height);
    }
}
