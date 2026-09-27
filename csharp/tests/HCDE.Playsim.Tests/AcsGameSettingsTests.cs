using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGameSettingsTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, 0)]
    [InlineData(SpawnGameMode.Cooperative, 1)]
    [InlineData(SpawnGameMode.Deathmatch, 2)]
    public void ModeQueryUsesConfiguredModeEvenWithoutPlayers(SpawnGameMode mode, int expected)
    {
        Query(Room(new SpawnOptions(Mode: mode)), 91, expected);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    public void SkillQueryUsesSameClampingAsSpawnFilter(int configured, int expected)
    {
        var sim = Room(new SpawnOptions(configured));
        Assert.Equal(expected, sim.Skill); Query(sim, 92, expected);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(92, 2)]
    public void OmittedOptionsUseSinglePlayerAndNormalSkill(int opcode, int expected)
    {
        Query(Room(), opcode, expected);
    }

    [Theory]
    [InlineData(SpawnGameMode.Cooperative, 2)]
    [InlineData(SpawnGameMode.Deathmatch, 2)]
    [InlineData(SpawnGameMode.Single, 3)]
    public void ObservableSettingsParticipateInChecksum(SpawnGameMode mode, int skill)
    {
        Assert.NotEqual(Room().Checksum, Room(new SpawnOptions(skill, mode)).Checksum);
    }

    [Fact]
    public void EquivalentClampedSkillsHaveSameChecksum()
    {
        Assert.Equal(Room(new SpawnOptions(4)).Checksum, Room(new SpawnOptions(99)).Checksum);
    }

    private static void Query(AuthoritySimulation sim, int opcode, int expected)
    {
        int[] words = [3, 7, opcode, 3, expected, 19, 5, 112, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static AuthoritySimulation Room(SpawnOptions? options = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    }, spawnOptions: options);
}
