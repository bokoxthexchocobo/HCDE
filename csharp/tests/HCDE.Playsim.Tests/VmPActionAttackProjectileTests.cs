namespace HCDE.Playsim.Tests;

public class VmPActionAttackProjectileTests
{
    [Fact]
    public void BulletAttack_SkipsWithoutTarget()
    {
        Assert.False(VmPActionAttackProjectile.BulletAttack(new() { Target = null }));
    }

    [Fact]
    public void Jump_RollsAgainstMaxChance()
    {
        Assert.Null(VmPActionAttackProjectile.Jump(50, 60, new(), null));
        Assert.NotNull(VmPActionAttackProjectile.Jump(256, 200, new(), null));
    }

    [Fact]
    public void RadiusDamageSelf_AppliesWhenInRange()
    {
        var hit = false;
        var ctx = new VmPActionAttackProjectile.RadiusSelfContext
        {
            Target = new(),
            Distance = 100,
            Damage = 50,
            ActualDistance = 50,
            DamageTarget = (_, _, _) => { hit = true; return 10; },
        };
        Assert.True(VmPActionAttackProjectile.RadiusDamageSelf(ctx));
        Assert.True(hit);
    }

    [Fact]
    public void SpawnProjectile_CheckTargetDead_Aborts()
    {
        var aborted = false;
        VmPActionAttackProjectile.SpawnProjectile(new()
        {
            CheckTargetDead = true,
            AbortToSeeState = () => aborted = true,
        });
        Assert.True(aborted);
    }
}
