namespace HCDE.Playsim.Tests;

public class VmPThingsProjectileDamageTests
{
    [Fact]
    public void EvThingProjectile_RejectsUnknownKind()
    {
        var ctx = new VmPThingsProjectileDamage.EvThingProjectileContext
        {
            Tid = 0,
            Source = new object(),
            Type = 1,
            ResolveKind = (_, _) => null,
        };
        Assert.False(VmPThingsProjectileDamage.EvThingProjectile(ctx));
    }

    [Fact]
    public void EvThingDamage_HealsWhenAmountNegative()
    {
        var actor = new VmPThingsProjectileDamage.DamageActor
        {
            Flags = VmPThingsProjectileDamage.MfShootable,
            Health = 50,
            SpawnHealth = 100,
            HasPlayer = true,
            PlayerHealth = 50,
        };
        var ctx = new VmPThingsProjectileDamage.EvThingDamageContext
        {
            Tid = 0,
            WhoFor = new object(),
            Amount = -10,
            AsDamageActor = _ => actor,
        };
        Assert.Equal(1, VmPThingsProjectileDamage.EvThingDamage(ctx));
        Assert.Equal(60, actor.Health);
        Assert.Equal(60, actor.PlayerHealth);
    }

    [Fact]
    public void EvThingProjectile_SpawnsMissileWithGravity()
    {
        VmPThingsProjectileDamage.ProjectileMobj? spawned = null;
        var ctx = new VmPThingsProjectileDamage.EvThingProjectileContext
        {
            Tid = 0,
            Source = "spot",
            Type = 1,
            DestTid = 0,
            Gravity = 1,
            ResolveKind = (_, _) => "Rocket",
            AsSpot = _ => new VmPThingsProjectileDamage.ProjectileSpot { PosZ = 64 },
            SpawnAtZ = (_, _, _) => spawned = new VmPThingsProjectileDamage.ProjectileMobj { Flags3 = 0 },
            VelFromAngle = (m, _) => m.VelX = 1,
            TestMobjLocation = _ => true,
        };
        Assert.True(VmPThingsProjectileDamage.EvThingProjectile(ctx));
        Assert.NotNull(spawned);
        Assert.Equal(0.125, spawned!.Gravity, 3);
    }
}
