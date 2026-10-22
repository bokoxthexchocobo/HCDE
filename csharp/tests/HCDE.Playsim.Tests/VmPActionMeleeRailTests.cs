namespace HCDE.Playsim.Tests;

public class VmPActionMeleeRailTests
{
    [Fact]
    public void CustomMeleeAttack_MissPlaysMissSound()
    {
        var miss = false;
        var ctx = new VmPActionMeleeRail.MeleeContext
        {
            Target = new(),
            InMeleeRange = () => false,
            PlayMissSound = () => miss = true,
        };
        Assert.True(VmPActionMeleeRail.CustomMeleeAttack(ctx, 5));
        Assert.True(miss);
    }

    [Fact]
    public void CustomComboAttack_SpawnsMissileWhenOutOfRange()
    {
        var spawned = false;
        var ctx = new VmPActionMeleeRail.ComboContext
        {
            Target = new(),
            InMeleeRange = () => false,
            HasMissileType = true,
            SpawnMissile = () => { spawned = true; return new(); },
        };
        Assert.True(VmPActionMeleeRail.CustomComboAttack(ctx, 3));
        Assert.True(spawned);
    }

    [Fact]
    public void CustomRailgun_RequiresTargetWhenAiming()
    {
        Assert.False(VmPActionMeleeRail.CustomRailgun(new() { Aim = 1, Target = null }));
    }
}
