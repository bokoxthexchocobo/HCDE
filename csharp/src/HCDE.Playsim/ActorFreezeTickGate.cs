namespace HCDE.Playsim;

/// <summary>Native <c>AActor::Tick</c> time-freezer gate (<c>freezetics</c>).</summary>
public static class ActorFreezeTickGate
{
    public static bool TryConsumeFreezeTick(Actor actor)
    {
        if (actor.FreezeTics <= 0)
            return false;
        actor.FreezeTics--;
        return true;
    }
}
