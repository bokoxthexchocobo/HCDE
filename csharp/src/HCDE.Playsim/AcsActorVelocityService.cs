namespace HCDE.Playsim;

/// <summary>Native <c>ACSF_GetActorVelX/Y/Z</c> from <c>p_acs.cpp</c> <c>CallFunction</c>.</summary>
public static class AcsActorVelocityService
{
    public static int Get(AuthoritySimulation sim, int tid, Actor? activator, int axis)
    {
        Actor? actor;
        if (tid == 0)
            actor = activator is { Destroyed: false } ? activator : null;
        else
            actor = AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed)
            return 0;
        var velocity = axis switch
        {
            0 => actor.VelocityX.ToDouble(),
            1 => actor.VelocityY.ToDouble(),
            _ => actor.VelocityZ.ToDouble(),
        };
        return AcsFixedPoint.FromDouble(velocity);
    }
}
