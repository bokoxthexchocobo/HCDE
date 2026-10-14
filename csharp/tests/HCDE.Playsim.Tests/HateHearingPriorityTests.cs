using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HateHearingPriorityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void HateSearchTakesPriorityOverDirectAndRememberedNoise(bool remembered, bool huntPlayers)
    {
        var (sim, actor, noise, hatee) = Room(); actor.TidToHate = 9; actor.HuntPlayers = huntPlayers;
        actor.LastHeardTargetId = noise.Id;
        if (remembered) actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        else
        {
            actor.Brain!.Hear(sim, actor, noise); Assert.Null(actor.TargetId);
            actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        }
        Assert.Equal(hatee.Id, actor.TargetId); Assert.Equal(noise.Id, actor.LastHeardTargetId);
    }

    [Fact]
    public void ClearingHateRestoresStoredNoiseEligibility()
    {
        var (sim, actor, noise, _) = Room(); actor.TidToHate = 999; actor.LastHeardTargetId = noise.Id;
        actor.Brain!.Tick(sim, actor); Assert.Null(actor.TargetId);
        actor.TidToHate = 0; actor.Brain.Tick(sim, actor); Assert.Equal(noise.Id, actor.TargetId);
    }

    [Fact]
    public void SavedHateAndNoisePreservePriority()
    {
        var (sim, actor, noise, hatee) = Room(); actor.TidToHate = 9; actor.LastHeardTargetId = noise.Id;
        var saved = SimSavegame.Write(sim, true); actor.TidToHate = 0; SimSavegame.Apply(sim, saved);
        actor.Brain!.Tick(sim, actor); Assert.Equal(hatee.Id, actor.TargetId); Assert.Equal(noise.Id, actor.LastHeardTargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Noise, Actor Hatee) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 },
                new LevelThing { Type = 3002, X = 200, Id = 8 }, new LevelThing { Type = 3001, Y = 300, Id = 9 }] });
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
