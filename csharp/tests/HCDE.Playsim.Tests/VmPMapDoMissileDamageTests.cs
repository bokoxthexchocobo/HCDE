namespace HCDE.Playsim.Tests;

public class VmPMapDoMissileDamageTests
{
    [Fact]
    public void CheckRipLevel_RespectsBounds() =>
        Assert.False(VmPMapDoMissileDamage.CheckRipLevel(5, 0, 2));

    [Fact]
    public void ProjectileImmune_BlocksSameSpeciesDefault() =>
        Assert.True(VmPMapDoMissileDamage.ProjectileImmune(0, 0, false, true, false));

    [Fact]
    public void CanAttackHurt_AllowsPlayers() =>
        Assert.True(VmPMapDoMissileDamage.CanAttackHurt(new VmPMapDoMissileDamage.CanAttackHurtState { VictimPlayer = true }));

    [Fact]
    public void DoMissileDamage_AppliesDamageAndBleed()
    {
        var dealt = 0;
        var bled = 0;
        var self = new VmPMapDoMissileDamage.MissileDamageSelf
        {
            GetMissileDamage = (_, _) => 10,
            DamageMobj = (_, _, _, d, _) => { dealt = d; return d; },
            TraceBleed = (_, _, _) => bled++,
        };
        VmPMapDoMissileDamage.DoMissileDamage(self, new VmPMapDoMissileDamage.MissileDamageVictim());
        Assert.Equal(10, dealt);
        Assert.Equal(1, bled);
    }

    [Fact]
    public void DoMissileDamage_HealsOnNegativeDamage()
    {
        var healed = 0;
        var self = new VmPMapDoMissileDamage.MissileDamageSelf
        {
            GetMissileDamage = (_, _) => -5,
            GiveBody = (_, amt) => healed = amt,
        };
        VmPMapDoMissileDamage.DoMissileDamage(self, new VmPMapDoMissileDamage.MissileDamageVictim());
        Assert.Equal(5, healed);
    }
}
