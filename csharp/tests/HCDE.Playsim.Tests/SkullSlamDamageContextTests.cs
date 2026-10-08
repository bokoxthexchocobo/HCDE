using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullSlamDamageContextTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(2)]
    public void ChargeImpactUsesMeleeFactorAfterSave(double factor)
    {
        var sim = Room(); var prediction = Room();
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        var target = sim.Players.Single(); target.Health = 1000;
        target.SetDamageFactor("Melee", factor); target.SetDamageFactor("Fire", 0);
        soul.DamageType = "Fire"; soul.Damage = 8;
        soul.Brain!.StartCharge(soul, target);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        var baseDamage = 8 * (1 + (int)(prediction.NextCombatRandom() & 7));
        if ((int)(baseDamage * factor) > 0) prediction.NextCombatRandom();
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(1000 - (int)(baseDamage * factor), target.Health);
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
        Assert.Equal(default, soul.VelocityX); Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DormantChargeStopsWithoutDamageRollAfterSave(bool save)
    {
        var sim = Room(); var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        var target = sim.Players.Single(); soul.Brain!.StartCharge(soul, target);
        soul.Dormant = true;
        if (save) SimSavegame.Apply(sim, SimSavegame.Write(sim));
        var random = sim.CombatRandomState;
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging); Assert.Equal(100, target.Health);
        Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(default, soul.VelocityX); Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }],
    }, rngSeed: 42);
}
