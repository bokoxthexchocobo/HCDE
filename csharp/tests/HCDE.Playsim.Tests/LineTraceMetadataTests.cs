using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineTraceMetadataTests
{
    [Theory]
    [InlineData(0, 128, 1, "MID")]
    [InlineData(32, 128, 2, "LOW")]
    [InlineData(0, 20, 0, "TOP")]
    public void WallTierSelectsNearSideTexture(short floor, short ceiling, int part, string texture)
    {
        var sim = WallRoom(floor, ceiling); var source = sim.Players.Single();
        var hit = CombatTrace.LineTrace(sim, source, default, 256, offsetZ: 28);
        Assert.Equal(ActorLineTraceHitType.Wall, hit.HitType); Assert.Equal(64, hit.Distance, 8);
        Assert.Equal(part, hit.LinePart); Assert.Equal(texture, hit.HitTexture); Assert.Equal(0, hit.HitSector);
        Assert.Equal((1.0, 0.0, 0.0), hit.Direction);
    }

    [Fact]
    public void BackSideHitUsesBackTextureAndSector()
    {
        var sim = WallRoom(0, 128); var source = sim.Players.Single(); source.X = Fixed.FromInt(128);
        var hit = CombatTrace.LineTrace(sim, source, BamAngle.FromDegrees(180), 256, offsetZ: 28);
        Assert.Equal(1, hit.Side); Assert.Equal(1, hit.HitSector); Assert.Equal("BACKMID", hit.HitTexture);
        Assert.Equal(64, hit.Distance, 8); Assert.Equal(-1, hit.Direction.X, 8);
    }

    [Theory]
    [InlineData(90, ActorLineTraceHitType.Floor, 28, "FLOOR", -1)]
    [InlineData(270, ActorLineTraceHitType.Ceiling, 100, "CEIL", 1)]
    public void PlaneHitsDescribeTextureDistanceAndDirection(int pitch, ActorLineTraceHitType type, int distance, string texture, int vertical)
    {
        var sim = Room(); var hit = CombatTrace.LineTrace(sim, sim.Players.Single(), default, 256,
            BamAngle.FromDegrees(pitch), offsetZ: 28);
        Assert.Equal(type, hit.HitType); Assert.Equal(distance, hit.Distance, 8); Assert.Equal(texture, hit.HitTexture);
        Assert.Equal(0, hit.HitSector); Assert.Equal(-1, hit.LinePart); Assert.Equal(vertical, hit.Direction.Z, 8);
    }

    [Fact]
    public void ActorHitReportsActorTypeAndEntryDistanceWithoutTexture()
    {
        var sim = Room(); var target = sim.AddBot(64, 0); var hit = CombatTrace.LineTrace(sim, sim.Players.Single(), default, 256, offsetZ: 28);
        Assert.Same(target, hit.Victim); Assert.Equal(ActorLineTraceHitType.Actor, hit.HitType);
        Assert.Equal(64 - target.Radius.ToDouble(), hit.Distance, 8); Assert.Null(hit.HitTexture); Assert.Equal(-1, hit.LinePart);
    }

    [Fact]
    public void MissDistanceStartsAtExplicitOriginAndDoesNotMutateState()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        var hit = CombatTrace.LineTrace(sim, sim.Players.Single(), BamAngle.FromDegrees(90), 10,
            flags: ActorLineTraceFlags.AbsolutePosition, offsetZ: 30, offsetForward: 10, offsetSide: 20);
        Assert.Equal(ActorLineTraceHitType.None, hit.HitType); Assert.Equal(10, hit.Distance, 8);
        Assert.Equal(10, hit.X, 8); Assert.Equal(30, hit.Y, 8); Assert.Equal(30, hit.Z, 8); Assert.Null(hit.HitTexture);
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "FLOOR", CeilingPic = "CEIL" }],
        Things = [new LevelThing { Type = 1 }]
    });

    private static AuthoritySimulation WallRoom(short floor, short ceiling) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { FloorHeight = floor, CeilingHeight = ceiling }],
        Sides = [new LevelSide { Sector = 0, TopTexture = "TOP", MidTexture = "MID", BottomTexture = "LOW" },
            new LevelSide { Sector = 1, TopTexture = "BACKTOP", MidTexture = "BACKMID", BottomTexture = "BACKLOW" }],
        Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = 128, Y2 = -128, SideFront = 0, SideBack = 1, Flags = LevelLine.BlockHitscanFlag }],
        Things = [new LevelThing { Type = 1 }]
    });
}
