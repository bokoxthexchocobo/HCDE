namespace HCDE.Playsim;

/// <summary>Native <c>p_mobj.cpp</c> <c>P_CheckMissileSpawn</c> subset for spawned projectiles.</summary>
public static class MissileSpawnCheck
{
    public static bool Check(AuthoritySimulation sim, ProjectileActor missile, double maxDistance)
    {
        if (missile.Destroyed) return false;
        var advanceX = missile.VelocityX.ToDouble();
        var advanceY = missile.VelocityY.ToDouble();
        var advanceZ = missile.VelocityZ.ToDouble();
        if (maxDistance > 0)
        {
            var maxSquared = maxDistance * maxDistance;
            while (advanceX * advanceX + advanceY * advanceY >= maxSquared)
            {
                advanceX *= 0.5;
                advanceY *= 0.5;
                advanceZ *= 0.5;
            }
        }
        missile.X = Fixed.FromDouble(missile.X.ToDouble() + advanceX);
        missile.Y = Fixed.FromDouble(missile.Y.ToDouble() + advanceY);
        missile.Z = Fixed.FromDouble(missile.Z.ToDouble() + advanceZ);
        missile.SectorIndex = ActorPhysics.SectorAt(sim.Level, missile.X.ToDouble(), missile.Y.ToDouble());
        if (ActorPhysics.TryMove(sim, missile, missile.X.ToDouble(), missile.Y.ToDouble(), out var wall))
            return true;
        if (wall?.Special == 9)
        {
            missile.Destroy();
            return false;
        }
        missile.ExplodeForAction();
        return false;
    }
}
