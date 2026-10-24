using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

/// <summary>
/// Sector line traversal with pitched traces; expectations follow native <c>FTraceInfo::LineCheck</c>
/// opening tests (<c>p_trace.cpp</c> 464–466, <c>hit.Z</c> vs far floor/ceiling).
/// </summary>
public class TraceSectorEntryTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SectorEntryUsesNativeLineCheckOpening(bool reverse, bool ceiling)
    {
        var sign = reverse ? -1 : 1;
        var firstX = sign * (ceiling ? 36 : 28);
        var secondX = sign * 64;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { FloorHeight = -256, CeilingHeight = 256 },
                new LevelSector { FloorHeight = ceiling ? -256 : 0, CeilingHeight = ceiling ? 64 : 256 },
                new LevelSector { FloorHeight = -256, CeilingHeight = 256 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }, new LevelSide { Sector = 2 }],
            Lines = [new LevelLine { X1 = secondX, X2 = secondX, Y1 = sign * 128, Y2 = -sign * 128,
                SideFront = 1, SideBack = 2, Flags = LevelLine.BlockHitscanFlag, Health = 100 },
                new LevelLine { X1 = firstX, X2 = firstX, Y1 = sign * 128, Y2 = -sign * 128,
                    SideFront = 0, SideBack = 1 }],
            Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
        });
        var source = CenterOrigin(sim); source.Z = Fixed.FromInt(0);
        var target = sim.AddBot(sign * 128, 0); target.Z = Fixed.FromInt(ceiling ? 120 : -112);
        target.Radius = Fixed.FromInt(16); target.Height = Fixed.FromInt(56);
        var pitch = BamAngle.FromDegrees(ceiling ? -45 : 45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 256);
        // Native LineCheck passes the first portal when hit.Z stays inside the far opening, then
        // ML_BLOCKHITSCAN on the inner line stops the trace (wall index 0) before the bot.
        Assert.Null(hit.Victim);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(-1, hit.PlaneSector);
        Assert.Null(CombatTrace.PickActor(sim, source, source.Angle, pitch, 256));
        var health = target.Health;
        AcsLineAttack.Attack(sim, source, unchecked((int)source.Angle.Raw), unchecked((int)pitch.Raw),
            7, "None", 256, absoluteAngles: true);
        Assert.Equal(health, target.Health);
        Assert.Equal(93, sim.Level.Lines[0].Health);
    }

    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
