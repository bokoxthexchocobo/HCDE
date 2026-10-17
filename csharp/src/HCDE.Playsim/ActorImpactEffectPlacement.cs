namespace HCDE.Playsim;

internal static class ActorImpactEffectPlacement
{
    internal static void AtVictim(AuthoritySimulation sim, Actor effect, Actor victim)
    {
        effect.SetXYZ(victim.PosPlusZ(victim.Height.ToDouble() / 4));
        effect.SectorIndex = ActorPhysics.SectorAt(sim.Level, effect.X.ToDouble(), effect.Y.ToDouble());
        effect.OnGround = effect.Z.ToDouble() <= sim.FloorOf(effect.SectorIndex);
        if (effect.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, effect);
        effect.RememberPosition();
    }
}
