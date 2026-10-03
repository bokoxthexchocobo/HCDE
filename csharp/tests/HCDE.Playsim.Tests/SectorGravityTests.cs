using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class SectorGravityTests
{
    [Theory]
    [InlineData(2, 150, 2.99)]
    [InlineData(0, -25, -0.25)]
    [InlineData(0, 0, 0)]
    public void MapDecodesFractionAndChangesOnlyTaggedSectors(int integer, int fraction, double expected)
    {
        var sim = Room();
        var line = new LevelLine { Special = 216, Arg0 = 7, Arg1 = integer, Arg2 = fraction, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, Assert.Single(sim.Players), line, true));
        Assert.Equal(expected, sim.Level.Sectors[0].Gravity, 8); Assert.Equal(1, sim.Level.Sectors[1].Gravity);
        Assert.Equal(0, line.Special);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsSelectsUntaggedSectors(bool stack)
    {
        var sim = Room(); int[] words = stack ? [3, 0, 3, 0, 3, 25, 6, 216, 1] : [11, 216, 0, 0, 25, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.Level.Sectors[0].Gravity); Assert.Equal(0.25, sim.Level.Sectors[1].Gravity);
    }
    [Fact]
    public void SectorGravityMultipliesActorGravityInFallingPhysicsAndChecksum()
    {
        var first = PhysicsRoom(); var second = PhysicsRoom();
        foreach (var sector in first.Level.Sectors) sector.Gravity = 0.25;
        foreach (var sim in new[] { first, second })
        {
            var player = Assert.Single(sim.Players); player.SectorIndex = 0;
            player.Z = Fixed.FromInt(64); player.OnGround = false; player.Gravity = Fixed.FromDouble(0.5);
            ActorPhysics.Step(sim, player);
        }
        Assert.Equal(-0.125, Assert.Single(first.Players).VelocityZ.ToDouble());
        Assert.Equal(-0.5, Assert.Single(second.Players).VelocityZ.ToDouble());
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
        Assert.True(SectorGravity.ExecuteSpecial(first, 216, 99, 2, 0));
    }
    private static AuthoritySimulation PhysicsRoom()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40 }],
        });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}