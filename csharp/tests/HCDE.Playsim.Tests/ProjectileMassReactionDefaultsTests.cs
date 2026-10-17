using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileMassReactionDefaultsTests
{
    [Theory]
    [InlineData(50, 8)]
    [InlineData(200, 0)]
    [InlineData(1000, -4)]
    [InlineData(0, 12)]
    [InlineData(-100, 3)]
    public void PatchedDefaultsSurviveFreshReconstructionAndRuntimeOverrides(int mass, int reaction)
    {
        var text = $"Thing 35\nMass = {mass}\nReaction time = {reaction}\n";
        var (sim, missile) = Room(text);
        Assert.Equal(mass, missile.Mass); Assert.Equal(mass, missile.SpawnMass);
        Assert.Equal(reaction, missile.ReactionTime); Assert.Equal(reaction, missile.SpawnReactionTime);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(text);
        SimSavegame.Apply(fresh, saved); Assert.Equal(mass, loaded.Mass); Assert.Equal(reaction, loaded.ReactionTime);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        ActorPropertyActions.SetMass(missile, 321); missile.ReactionTime = 17;
        var changed = SimSavegame.Write(sim, true); SimSavegame.Apply(fresh, changed);
        Assert.Equal(321, loaded.Mass); Assert.Equal(17, loaded.ReactionTime);
        SimSavegame.Apply(fresh, saved); Assert.Equal(mass, loaded.Mass); Assert.Equal(reaction, loaded.ReactionTime);
        Assert.Equal(mass, loaded.SpawnMass); Assert.Equal(reaction, loaded.SpawnReactionTime);
    }

    [Theory]
    [InlineData(50, 40)]
    [InlineData(200, 10)]
    [InlineData(1000, 2)]
    public void PatchedMassControlsActualArchvileLaunchAfterSave(int mass, int launch)
    {
        var (sim, missile) = Room($"Thing 35\nBits = SHOOTABLE+NOGRAVITY\nMass = {mass}\n");
        missile.SetXYZ(100, 0, 0); missile.Health = 1000; missile.NoPain = true;
        var attacker = sim.AddBot(-200, 0, 64); attacker.Brain = null;
        var saved = SimSavegame.Write(sim, true); missile.Mass = 100;
        SimSavegame.Apply(sim, saved); ArchvileActions.Attack(sim, attacker, missile, false, thrust: 2);
        Assert.Equal(launch, missile.VelocityZ.ToDouble()); Assert.Equal(980, missile.Health);
    }

    [Fact]
    public void InheritedFieldsPersistThroughUnrelatedPatch()
    {
        var original = DehackedPatch.Apply("Thing 35\nMass = 200\nReaction time = 9\n");
        var inherited = DehackedPatch.Apply("Thing 35\nSpeed = 12\n", original); Assert.Empty(inherited.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: inherited);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(200, missile.Mass); Assert.Equal(9, missile.ReactionTime); Assert.Equal(12, missile.MovementSpeed.ToDouble());
    }

    [Fact]
    public void UnpatchedFieldsKeepStockProjectileDefaults()
    {
        var (sim, missile) = Room("Thing 35\nSpeed = 12\n");
        var control = new ProjectileActor(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(control.Mass, missile.Mass); Assert.Equal(control.ReactionTime, missile.ReactionTime);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string text)
    {
        var patch = DehackedPatch.Apply(text); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 600 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
