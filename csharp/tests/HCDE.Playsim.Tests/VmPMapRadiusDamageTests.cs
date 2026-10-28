namespace HCDE.Playsim.Tests;

public class VmPMapRadiusDamageTests
{
    static VmPMapRadiusDamage.ActorRef Spot => new("spot", 0, 0, 0, 32, 16, 100, 0, 0, 0, 0, 0, 0, 0, false, false, false, true, "", "", 1);
    static VmPMapRadiusDamage.ActorRef Victim => new("victim", 64, 0, 0, 32, 16, 100, VmPMapRadiusDamage.MfShootable, 0, 0, 0, 0, 0, 0, false, false, false, true, "", "", 1);

    [Fact]
    public void GetRadiusDamageAction_ReturnsFullDamageOnSelf()
    {
        var d = VmPMapRadiusDamage.GetRadiusDamageAction(Spot, Spot, 128, 128, 0, false, false);
        Assert.Equal(128, d);
    }

    [Fact]
    public void GetRadiusDamage_ModernSquareDistanceUsesZWhenOutsideBand()
    {
        var spot = Spot with { Z = 100 };
        var victim = Victim with { Z = 0, Top = 32, Radius = 16 };
        var pts = VmPMapRadiusDamage.GetRadiusDamage(false, spot, victim, 100, 128, 0, false, false);
        Assert.True(pts > 0);
    }

    [Fact]
    public void GetOldRadiusDamage_ReturnsNegativeWhenOutOfRange()
    {
        var far = Victim with { X = 10000 };
        Assert.Equal(-1, VmPMapRadiusDamage.GetOldRadiusDamage(false, Spot, far, 128, 128, 0));
    }

    [Fact]
    public void RadiusAttack_SkipsNonShootableWithoutVulnerable()
    {
        var s = new VmPMapRadiusDamage.RadiusAttackState
        {
            BombSpot = Spot,
            BombDamage = 64,
            BombDistance = 128,
        };
        s.Candidates.Add(Victim with { Flags = 0 });
        Assert.Equal(0, VmPMapRadiusDamage.RadiusAttack(s));
        Assert.Empty(s.Targets);
    }

    [Fact]
    public void RadiusAttack_DamagesWhenInSight()
    {
        var s = new VmPMapRadiusDamage.RadiusAttackState
        {
            BombSpot = Spot,
            BombSource = Spot,
            BombDamage = 100,
            BombDistance = 128,
            CompatExplode2 = 0,
            CheckSight = _ => true,
            DamageMobj = (_, _, dmg) => dmg,
        };
        s.Candidates.Add(Victim);
        Assert.Equal(1, VmPMapRadiusDamage.RadiusAttack(s));
    }
}
