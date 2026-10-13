using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorDropActorsTests
{
    [Theory]
    [InlineData(false, 0, 64)]
    [InlineData(true, 0, 48)]
    [InlineData(true, 8, 72)]
    public void FastCarryRequiresSectorFlagAndFloorContact(bool dropActors, int offset, int expected)
    {
        var sim = Room(dropActors); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(64 + offset);
        Drop(sim); LineSpecials.TickMotions(sim);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.Equal(48, sim.FloorOf(0));
    }

    [Fact]
    public void NoSectorStillExcludesCarry()
    {
        var sim = Room(true); var actor = sim.Players.Single(); actor.NoSector = true;
        Drop(sim); sim.Tick(); Assert.Equal(64, actor.Z.ToDouble()); Assert.False(actor.OnGround);
    }

    [Fact]
    public void FastCarryAndSaveContinuationUseLoadedMapFlag()
    {
        var sim = Room(true); var actor = sim.Players.Single(); Drop(sim); sim.Tick();
        Assert.Equal(48, actor.Z.ToDouble());
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(32, actor.Z.ToDouble());
    }

    private static void Drop(AuthoritySimulation sim) => sim.Motions.Add(new SectorMotion
    { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 0, FloorDirection = -1, Speed = 16 });

    private static AuthoritySimulation Room(bool dropActors)
    {
        var text = $$"""namespace = "ZDoom"; sector { heightfloor = 64; heightceiling = 256; dropactors = {{dropActors.ToString().ToLowerInvariant()}}; } thing { type = 1; skill1 = true; skill2 = true; skill3 = true; skill4 = true; skill5 = true; single = true; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        return AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
    }
}
