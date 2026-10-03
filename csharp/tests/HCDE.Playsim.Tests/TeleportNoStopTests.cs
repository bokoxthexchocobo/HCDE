using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportNoStopTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MapAndAcsPreserveVelocityAndReactionCounter(int path)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        player.VelocityX = Fixed.FromInt(3); player.VelocityY = Fixed.FromInt(4);
        player.VelocityZ = Fixed.FromInt(2); player.ReactionTime = 7;
        if (path == 0)
        {
            var line = new LevelLine { Special = 154, Arg1 = 7, PlayerCross = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, player, line, false, false));
            Assert.Equal(0, line.Special);
        }
        else
        {
            int[] words = path == 1 ? [10, 154, 0, 7, 1] : [3, 0, 3, 7, 5, 154, 1];
            var bytes = new byte[words.Length * 4];
            for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
            sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
            Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        }
        Assert.Equal(200, player.X.ToDouble()); Assert.Equal(90, player.Angle.ToDegrees());
        Assert.Equal(3, player.VelocityX.ToDouble()); Assert.Equal(4, player.VelocityY.ToDouble());
        Assert.Equal(2, player.VelocityZ.ToDouble()); Assert.Equal(7, player.ReactionTime);
    }

    [Fact]
    public void MissileStillRedirectsHorizontalSpeed()
    {
        var sim = Room();
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            NoTeleport = false, VelocityX = Fixed.FromInt(3), VelocityY = Fixed.FromInt(4),
            VelocityZ = Fixed.FromInt(2), SectorIndex = 0,
        };
        Assert.True(LineSpecials.ExecuteTeleportSpecial(sim, 154, 0, 7, missile, false));
        Assert.Equal(0, missile.VelocityX.ToDouble()); Assert.Equal(5, missile.VelocityY.ToDouble());
        Assert.Equal(2, missile.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void BackSideAndNoTeleportRejectWithoutConsumingLine(bool back, bool noTeleport)
    {
        var sim = Room(); var player = Assert.Single(sim.Players); player.NoTeleport = noTeleport;
        var line = new LevelLine { Special = 154, Arg1 = 7, PlayerCross = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, false, back));
        Assert.Equal(0, player.X.ToDouble()); Assert.Equal(154, line.Special);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200, Angle = 90 }],
    });
}