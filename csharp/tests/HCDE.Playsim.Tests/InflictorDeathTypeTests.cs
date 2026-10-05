namespace HCDE.Playsim.Tests;

public class InflictorDeathTypeTests
{
    [Fact]
    public void OverrideChecksumUsesNativeNameCaseAndPreservesNoneDefault()
    {
        var level = new HCDE.MapLoader.PlayLevel
        {
            Sectors = [new HCDE.MapLoader.LevelSector { CeilingHeight = 128 }],
            Things = [new HCDE.MapLoader.LevelThing { Type = 1 }],
        };
        var left = AuthoritySimulation.Start(level);
        var right = AuthoritySimulation.Start(level);
        right.Players.Single().DeathType = "none";
        left.Tick(); right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
        left.Players.Single().DeathType = "Fire";
        left.Tick(); right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().DeathType = "FIRE";
        left.Tick(); right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    [Theory]
    [InlineData("fire", true)]
    [InlineData("Ice", false)]
    [InlineData("None", false)]
    [InlineData("none", false)]
    public void OverrideControlsTypedOnlyDamageGate(string deathType, bool accepted)
    {
        var actor = new Actor { Health = 20, DeathState = -1 };
        actor.SetTypedDeath("Fire", 3);
        var result = ActorDamage.Apply(actor, 20, damageType: "Normal", inflictor: new Actor { DeathType = deathType });
        Assert.Equal(accepted, result.Killed);
        Assert.Equal(accepted ? 0 : 20, actor.Health);
        if (accepted) Assert.Equal(3, actor.States.Current);
    }

    [Fact]
    public void DeathOverrideLeavesSurvivingPainDamageTypeUnchanged()
    {
        var actor = new Actor { Health = 100, PainChance = 0 };
        actor.SetTypedPain("Ice", 3, 256);
        actor.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(actor, 10, damageType: "Ice", inflictor: new Actor { DeathType = "Fire" });
        Assert.Equal(3, actor.States.Current);
        Assert.Equal(90, actor.Health);
    }

    [Fact]
    public void MassacreDamageStillBypassesGateBeforeOverride()
    {
        var actor = new Actor { Health = 20, DeathState = -1 };
        actor.SetTypedDeath("Fire", 3);
        Assert.True(ActorDamage.Apply(actor, 20, damageType: "Massacre", inflictor: new Actor { DeathType = "Ice" }).Killed);
    }
}
