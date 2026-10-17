namespace HCDE.Playsim;

internal static class TeleportActorCollision
{
    internal static bool TryClear(AuthoritySimulation sim, Actor mover, double x, double y, double z)
    {
        foreach (var target in sim.Actors.ToArray())
        {
            if (target == mover || target.Destroyed || !target.IsBlockmapActor || !target.Shootable
                || target.DoomEdNum == LineSpecials.TeleportDestType || target.DoomEdNum == InvasionDirector.SpawnSpotType
                || mover.ThruActors || target.ThruActors || mover.SharesEnabledThruBits(target)
                || mover.ThruSpecies && mover.SharesContactSpecies(target)) continue;
            var reach = mover.Radius.ToDouble() + target.Radius.ToDouble();
            if (Math.Abs(x - target.X.ToDouble()) >= reach || Math.Abs(y - target.Y.ToDouble()) >= reach) continue;
            if ((mover.CanPass || target.ActsLikeBridge) && !sim.Compat.HasFlag(CompatSurface.NoPassMobj)
                && !(mover.DontOverlap && target.DontOverlap)
                && (z > target.Z.ToDouble() + target.Height.ToDouble() || z + mover.Height.ToDouble() < target.Z.ToDouble())) continue;
            if (!mover.Solid && (mover.TeleStomp || PickupCatalog.IsPickup(mover.ClassDoomEdNum, sim.RavenHealthPickups))
                && (target is PlayerPawn || target.IsMonster)) continue;
            if ((mover.TeleStomp || sim.Level.AllowMonsterTelefrags) && !mover.NoTeleStomp && !target.NoTelefrag || target.AlwaysTelefrag)
            {
                ActorDamage.Apply(target, ActorDamage.TelefragDamage, mover, damageType: "Telefrag", inflictor: mover);
                continue;
            }
            return false;
        }
        return true;
    }
}
