using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedAmbushTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void SpawnCombinesClassAndMapAmbush(bool patchedAmbush, bool mapAmbush, bool expected)
    {
        var sim = Room(patchedAmbush, mapAmbush);
        Assert.Equal(expected, sim.Actors.Single(actor => actor.DoomEdNum == 3004).Ambush);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void PatchedAmbushControlsHiddenNoiseAcquisition(bool patchedAmbush, bool acquires)
    {
        var sim = Room(patchedAmbush, false);
        var monster = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        var player = sim.Players.Single();
        SoundPropagation.Alert(sim, player);
        Assert.Equal(player.Id, monster.LastHeardTargetId);
        Assert.Equal(acquires ? (uint?)player.Id : null, monster.Brain!.TargetId);
        monster.ReactionTime = 0;
        monster.Brain.Tick(sim, monster);
        Assert.Equal(acquires ? (uint?)player.Id : null, monster.Brain.TargetId);
    }

    private static AuthoritySimulation Room(bool patchedAmbush, bool mapAmbush) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = -128, Y2 = 128, SideBack = -1 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 128, Ambush = mapAmbush }],
    }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {(patchedAmbush ? 38 : 6)}\n"));
}
