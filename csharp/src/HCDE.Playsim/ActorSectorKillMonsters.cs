namespace HCDE.Playsim;

/// <summary>Native <c>SECF_KILLMONSTERS</c> floor contact instant kill (<c>p_mobj.cpp</c>).</summary>
internal static class ActorSectorKillMonsters
{
    public static void Tick(AuthoritySimulation simulation, Actor actor)
    {
        if (actor.Destroyed || actor.IsDead || actor is PlayerPawn || !actor.Shootable || actor.Floating)
            return;
        if (actor.SectorIndex < 0 || actor.SectorIndex >= simulation.Level.Sectors.Count)
            return;
        var sector = simulation.Level.Sectors[actor.SectorIndex];
        if ((sector.RuntimeFlags & SectorEnvironmentLineSpecials.KillMonstersFlag) == 0)
            return;
        var floor = simulation.FloorOf(actor.SectorIndex);
        if (Math.Abs(actor.Z.ToDouble() - floor) > 1.0 / Fixed.Unit)
            return;
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage, damageType: "InstantDeath");
    }
}
