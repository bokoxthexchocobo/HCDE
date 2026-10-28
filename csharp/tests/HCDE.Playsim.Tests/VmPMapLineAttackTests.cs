namespace HCDE.Playsim.Tests;

public class VmPMapLineAttackTests
{
    [Fact]
    public void LineAttack_SuppressesLocalPlayerHitscan()
    {
        var st = new VmPMapLineAttack.LineAttackState { SuppressLocalHitscan = true };
        Assert.Null(VmPMapLineAttack.LineAttack(st));
        Assert.Equal(0, st.ActualDamage);
    }

    [Fact]
    public void LineAttack_SpawnsPuffOnMissWhenAlwaysPuff()
    {
        object? spawned = new object();
        var st = new VmPMapLineAttack.LineAttackState
        {
            PuffDefaults = new VmPMapLineAttack.PuffDefaults { Flags3 = VmPMapLineAttack.Mf3Alwayspuff },
            BuildStartPos = _ => (0, 0, 32),
            Trace = _ => null,
            SpawnPuff = (_, _, _f) => spawned,
        };
        Assert.Null(VmPMapLineAttack.LineAttack(st));
    }

    [Fact]
    public void LineAttack_NoInteractReturnsPuffDirectly()
    {
        var puff = new object();
        var st = new VmPMapLineAttack.LineAttackState
        {
            Flags = VmPMapLineAttack.LafNointeract,
            BuildStartPos = _ => (0, 0, 32),
            Trace = _ => null,
            SpawnPuff = (_, _, _f) => puff,
        };
        Assert.Same(puff, VmPMapLineAttack.LineAttack(st));
    }

    [Fact]
    public void UsePuffDamageType_OverridesHitscan()
    {
        Assert.True(VmPMapLineAttack.UsePuffDamageType("Hitscan", new VmPMapLineAttack.PuffDefaults { DamageType = "Fire" }));
    }
}
