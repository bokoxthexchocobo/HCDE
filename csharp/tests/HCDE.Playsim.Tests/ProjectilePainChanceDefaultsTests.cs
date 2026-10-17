using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectilePainChanceDefaultsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(128, 128)]
    [InlineData(256, 256)]
    [InlineData(1000, 1000)]
    [InlineData(-1, -1)]
    [InlineData(32768, -32768)]
    [InlineData(65536, 0)]
    public void ExplicitPatchUsesNativeSignedShortAndReconstructs(int input, int chance)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nPain chance = {input}\n"); Assert.Empty(patch.Errors);
        Assert.True(patch.Actors.Single(a => a.Index == 35).PainChancePatched);
        var (sim, missile) = Room(patch); Assert.Equal(chance, missile.PainChance);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(patch);
        SimSavegame.Apply(fresh, saved); Assert.Equal(chance, loaded.PainChance);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(-1)]
    public void PatchedChanceControlsPainActionAndSavedRandomContinuation(int chance)
    {
        var patch = DehackedPatch.Apply($"Thing 35\nPain chance = {chance}\n");
        var (sim, missile) = Room(patch); missile.PainState = 0;
        var expected = NativeStateRandom.Seed(42, 0xdae49b5au);
        var reacts = (NativeStateRandom.Next(ref expected) & 255) < chance;
        Assert.Equal(reacts, missile.TriggerPainChance()); Assert.Equal(expected, sim.CaptureState().PainChanceRandomState);
        var saved = SimSavegame.Write(sim, true); var next = missile.TriggerPainChance();
        var random = sim.CaptureState().PainChanceRandomState;
        SimSavegame.Apply(sim, saved); Assert.Equal(next, missile.TriggerPainChance());
        Assert.Equal(random, sim.CaptureState().PainChanceRandomState);
    }

    [Fact]
    public void InheritanceRetainsExplicitnessUntilFieldReplacement()
    {
        var original = DehackedPatch.Apply("Thing 35\nPain chance = 0\n");
        var inherited = DehackedPatch.Apply("Thing 35\nMass = 200\n", original);
        var replaced = DehackedPatch.Apply("Thing 35\nPain chance = 256\n", inherited);
        Assert.Equal(0, Room(inherited).Missile.PainChance); Assert.Equal(256, Room(replaced).Missile.PainChance);
        Assert.Equal(0, original.Actors.Single(a => a.Index == 35).PainChance);
    }

    [Fact]
    public void UnrelatedPatchPreservesStockProjectilePainChance()
    {
        var patch = DehackedPatch.Apply("Thing 35\nMass = 200\n"); var (sim, missile) = Room(patch);
        Assert.False(patch.Actors.Single(a => a.Index == 35).PainChancePatched);
        Assert.Equal(new ProjectileActor(sim.Players.Single(), ProjectileKind.Plasma).PainChance, missile.PainChance);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }] }, rngSeed: 42, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
