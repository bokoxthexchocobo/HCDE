using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetPlayerInputTests
{
    [Fact]
    public void OriginalAndModifiedInputSurviveTheNextTicAndFreshSave()
    {
        var sim = Room();
        var player = sim.Players.Single(p => p.PlayerNum == 0);
        sim.QueueCommand(0, new PlayerCommand { Attack = true, Crouch = true, Jump = true, ForwardMove = 100, YawDelta = 7, PitchDelta = -3, SideMove = 4, UpMove = 2 });
        sim.Tick();
        Assert.Equal(1 | 4 | 8, player.GetPlayerInput(1));
        Assert.Equal(1 | 4, player.GetPlayerInput(9));
        Assert.Equal(100, player.GetPlayerInput(5));
        Assert.Equal(7, player.GetPlayerInput(3));
        Assert.Equal(-3, player.GetPlayerInput(2));
        Assert.Equal(4, player.GetPlayerInput(6));
        Assert.Equal(2, player.GetPlayerInput(7));
        Assert.Equal(0, player.GetPlayerInput(0));
        Assert.Equal(0, player.GetPlayerInput(16));
        Assert.Equal(0, player.GetPlayerInput(-1));
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        sim.QueueCommand(0, new PlayerCommand { Use = true, ForwardMove = 20 });
        sim.Tick();
        Assert.Equal(1 | 4 | 8, player.GetPlayerInput(0));
        Assert.Equal(1 | 4, player.GetPlayerInput(8));
        Assert.Equal(2, player.GetPlayerInput(1));
        Assert.Equal(20, player.GetPlayerInput(5));
        var continued = SimSavegame.Write(sim, true);
        var fresh = Room();
        SimSavegame.Apply(fresh, continued);
        var restored = fresh.Players.Single(p => p.PlayerNum == 0);
        Assert.Equal(1 | 4 | 8, restored.GetPlayerInput(0));
        Assert.Equal(1 | 4, restored.GetPlayerInput(8));
        Assert.Equal(2, restored.GetPlayerInput(1));
        Assert.Equal(20, restored.GetPlayerInput(5));
        Assert.Equal(continued, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void FrozenChainsawAndPointersFollowTheRepresentedCommand()
    {
        var sim = Room();
        var first = sim.Players.Single(p => p.PlayerNum == 0);
        var second = sim.Players.Single(p => p.PlayerNum == 1);
        var imp = sim.Actors.Single(a => a.DoomEdNum == 2007);
        first.JustAttacked = true;
        first.Frozen = true;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 50, YawDelta = 9, Attack = true });
        sim.Tick();
        Assert.Equal(50, first.GetPlayerInput(5));
        Assert.Equal(9, first.GetPlayerInput(3));
        Assert.Equal(0, first.GetPlayerInput(13));
        Assert.Equal(0, first.GetPlayerInput(11));
        Assert.Equal(1, first.GetPlayerInput(9));
        first.Frozen = false;
        first.TotallyFrozen = true;
        sim.QueueCommand(0, new PlayerCommand { Use = true, Attack = true, ForwardMove = 80, YawDelta = 1 });
        sim.Tick();
        Assert.Equal(1 | 2, first.GetPlayerInput(1));
        Assert.Equal(2, first.GetPlayerInput(9));
        Assert.Equal(80, first.GetPlayerInput(5));
        Assert.Equal(0, first.GetPlayerInput(13));
        Assert.Equal(0, second.GetPlayerInput(1));
        Assert.Equal(1 | 2, second.GetPlayerInput(1, AcsActorPointer.Player1));
        Assert.Equal(0, imp.GetPlayerInput(1));
        imp.SetTargetPointer(first);
        Assert.Equal(2, imp.GetPlayerInput(9, AcsActorPointer.Target));
        Assert.Equal(0, first.GetPlayerInput(1, AcsActorPointer.Null));
        Assert.Throws<InvalidOperationException>(() => new Actor().GetPlayerInput(1, AcsActorPointer.Player1));
        Assert.Equal(0, new PlayerPawn().GetPlayerInput(1));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 256 }, new LevelThing { Type = 2007, X = -256 }]
    });
}
