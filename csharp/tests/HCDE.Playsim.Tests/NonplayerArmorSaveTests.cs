using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonplayerArmorSaveTests
{
    [Theory]
    [InlineData(33, 0, 0, 0)]
    [InlineData(50, 20, 5, 7)]
    public void RestoredNonplayerArmorProducesSameDamage(int percent, int cap, int full, int count)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        actor.Armor = 80; actor.ArmorSavePercent = percent; actor.MaxAbsorb = cap;
        actor.MaxFullAbsorb = full; actor.AbsorbCount = count;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state); var other = restored.Actors.Single();
        Assert.Equal(SimActorArmor.Capture(actor), SimActorArmor.Capture(other));
        Assert.Equal(bytes, SimSavegame.Write(restored));
        ActorDamage.Apply(actor, 30); ActorDamage.Apply(other, 30);
        Assert.Equal(actor.Health, other.Health); Assert.Equal(actor.Armor, other.Armor);
        Assert.Equal(actor.AbsorbCount, other.AbsorbCount);
    }

    [Fact]
    public void LegacyRestoreClearsNonplayerArmorOverrides()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim); var actor = sim.Actors.Single();
        actor.Armor = 80; actor.ArmorSavePercent = 50; actor.AbsorbCount = 7;
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        sim.RestoreState(state);
        Assert.Null(SimActorArmor.Capture(actor));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
    });
}
