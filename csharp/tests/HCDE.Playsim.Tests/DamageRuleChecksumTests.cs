using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageRuleChecksumTests
{
    [Fact]
    public void EquivalentRulesIgnoreCaseInsertionOrderAndSignedZero()
    {
        var left = Room(); var right = Room();
        left.DamageTypes.Define("Fire", 0); left.DamageTypes.Define("Ice", 2, noArmor: true);
        right.DamageTypes.Define("iCe", 2, noArmor: true); right.DamageTypes.Define("fIrE", -0.0);
        left.Players.Single().SetDamageFactor("Fire", 0.5); left.Players.Single().SetDamageFactor("Ice", 0);
        right.Players.Single().SetDamageFactor("iCe", -0.0); right.Players.Single().SetDamageFactor("fIrE", 0.5);
        left.Tick(); right.Tick(); Assert.Equal(left.Checksum, right.Checksum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void DamageRuleDifferencesChangeSimulationChecksum(int kind)
    {
        var left = Room(); var right = Room();
        if (kind == 0) left.DamageTypes.Define("Fire", 2);
        if (kind == 1) left.DamageTypes.Define("Fire", replaceFactor: true);
        if (kind == 2) left.DamageTypes.Define("Fire", noArmor: true);
        if (kind == 3) left.Players.Single().SetDamageFactor("Fire", 0.5);
        left.Tick(); right.Tick(); Assert.NotEqual(left.Checksum, right.Checksum);
    }

    [Fact]
    public void RedefiningBuiltInToSameValuePreservesChecksum()
    {
        var left = Room(); var right = Room();
        left.DamageTypes.Define("drowning", noArmor: true);
        left.Tick(); right.Tick(); Assert.Equal(left.Checksum, right.Checksum);
        left.DamageTypes.Define("Drowning", noArmor: false);
        left.Tick(); right.Tick(); Assert.NotEqual(left.Checksum, right.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
