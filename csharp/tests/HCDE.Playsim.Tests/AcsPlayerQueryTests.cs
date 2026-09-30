using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPlayerQueryTests
{
    [Fact]
    public void IsNetworkGame_IsFalseForLocalAuthority()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.IsNetworkGame,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SinglePlayer_IsTrueInSinglePlayerMode()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.SinglePlayer,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void SinglePlayer_IsFalseInCooperativeMode()
    {
        var sim = AuthoritySimulation.Start(
            new PlayLevel
            {
                Format = MapDataFormat.HexenBinary,
                MapName = "MAP01",
                Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1, X = 32, Y = 64, Single = false, Coop = true }],
            },
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        Run(sim,
            (int)AcsPcode.SinglePlayer,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PlayerInGame_ReportsTheSpawnedPlayerSlot()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PlayerInGame,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PlayerInGame_ReturnsFalseForAnEmptySlot()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.PlayerInGame,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void GameType_ReportsSingleCoopAndDeathmatch()
    {
        var single = Room();
        Run(single,
            (int)AcsPcode.GameType,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, single.LightOf(0));

        var coop = AuthoritySimulation.Start(
            new PlayLevel
            {
                Format = MapDataFormat.HexenBinary,
                MapName = "MAP01",
                Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1, X = 32, Y = 64, Single = false, Coop = true }],
            },
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        Run(coop,
            (int)AcsPcode.GameType,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, coop.LightOf(0));

        var deathmatch = AuthoritySimulation.Start(
            new PlayLevel
            {
                Format = MapDataFormat.HexenBinary,
                MapName = "MAP01",
                Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1, X = 32, Y = 64, Single = false, Deathmatch = true }],
            },
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch));
        Run(deathmatch,
            (int)AcsPcode.GameType,
            (int)AcsPcode.PushNumber, 2,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, deathmatch.LightOf(0));
    }

    [Fact]
    public void PlayerCount_ReportsSpawnedHumans()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.PlayerCount,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void GameSkill_ReportsBootSkill()
    {
        var sim = AuthoritySimulation.Start(
            new PlayLevel
            {
                Format = MapDataFormat.HexenBinary,
                MapName = "MAP01",
                Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
            },
            spawnOptions: new SpawnOptions(Skill: 3));
        Run(sim,
            (int)AcsPcode.GameSkill,
            (int)AcsPcode.PushNumber, 3,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PlayerIsBot_IsFalseForHumanPlayersAndMissingSlots()
    {
        var sim = Room();
        Run(sim,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PlayerIsBot,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));

        Run(sim,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.PlayerIsBot,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });
}
