using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGetPlayerInputNativeParityTests
{
    [Fact]
    public void Query_ReadsOriginalForwardMoveAfterTick()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 42 });
        sim.Tick();
        Assert.Equal(42, AcsPlayerInput.Query(sim, sim.Players.Single(), 0, AcsPlayerInputIds.InputForwardMove));
    }

    [Fact]
    public void Query_ReadsModifiedUseButtonMask()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.Equal(2, AcsPlayerInput.Query(sim, sim.Players.Single(), 0, AcsPlayerInputIds.ModInputButtons));
    }

    [Fact]
    public void Query_ReadsOriginalPitchDelta()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { PitchDelta = 9 });
        sim.Tick();
        Assert.Equal(9, AcsPlayerInput.Query(sim, sim.Players.Single(), 0, AcsPlayerInputIds.InputPitch));
    }

    [Fact]
    public void Query_ReturnsZeroForUnknownInputNumber()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Assert.Equal(0, AcsPlayerInput.Query(sim, player, 0, 99));
    }

    [Fact]
    public void Query_ReturnsZeroWhenExplicitPlayerNotInGame()
    {
        var sim = Room();
        Assert.Equal(0, AcsPlayerInput.Query(sim, null, 3, AcsPlayerInputIds.InputButtons));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
