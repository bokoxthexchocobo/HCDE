namespace HCDE.Playsim;

public static class NoiseAlertSpecial
{
    internal static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int targetTid, int emitterTid)
        => special == 173 ? Execute(sim, activator, targetTid, emitterTid) : null;

    public static bool Execute(AuthoritySimulation sim, Actor? activator, int targetTid, int emitterTid)
    {
        var target = targetTid == 0 ? activator : AcsActorTid.AllFromTid(sim, targetTid).FirstOrDefault();
        var emitter = emitterTid == 0 ? activator : emitterTid == targetTid ? target
            : AcsActorTid.AllFromTid(sim, emitterTid).FirstOrDefault();
        if (emitter is not null) SoundPropagation.Alert(sim, emitter, target);
        return true;
    }
}
