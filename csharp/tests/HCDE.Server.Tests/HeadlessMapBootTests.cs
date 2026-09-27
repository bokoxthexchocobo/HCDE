using HCDE.MapLoader.Tests;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class HeadlessMapBootTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 4)]
    public void ServerRetainsModeAndSkillForAcs(bool deathmatch, byte skill)
    {
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        { Port = 0, IwadBytes = TestWadBuilder.BuildMinimalMapWad("MAP01"), Deathmatch = deathmatch, Skill = skill });
        var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
        Assert.Equal(deathmatch ? SpawnGameMode.Deathmatch : SpawnGameMode.Cooperative, sim.GameMode);
        Assert.Equal(skill, sim.Skill);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void ServerPropagatesDamageExitPolicy(bool deathmatch, bool noExit, bool allowed)
    {
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        { Port = 0, IwadBytes = TestWadBuilder.BuildMinimalMapWad("MAP01"), Deathmatch = deathmatch, NoExit = noExit });
        Assert.Equal(allowed, Assert.IsType<AuthoritySimulation>(host.Simulation).DamageExitAllowed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CommandLineCrushDefaultsReachBootedSimulation(int flagCount)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, TestWadBuilder.BuildMinimalMapWad("MAP01"));
            var args = new List<string> { "--iwad", path };
            for (var i = 0; i < flagCount; i++) args.Add("--hexen-crush-defaults");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
            Assert.Equal(flagCount == 0 ? CompatSurface.None : CompatSurface.HexenCrushDefaults, sim.Compat);
            var reference = AuthoritySimulation.Start(sim.Level, options.Pregame.Session.MapLoad.RngSeed,
                compat: sim.Compat, spawnOptions: new SpawnOptions(options.Skill, SpawnGameMode.Cooperative));
            Assert.Equal(reference.Checksum, sim.Checksum);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ServerPassesCombinedCompatibilityFlagsWithoutDiscardingExistingRules()
    {
        const CompatSurface flags = CompatSurface.HexenCrushDefaults | CompatSurface.SharedLightMaximum;
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        { Port = 0, IwadBytes = TestWadBuilder.BuildMinimalMapWad("MAP01"), Compatibility = flags });
        Assert.Equal(flags, Assert.IsType<AuthoritySimulation>(host.Simulation).Compat);
    }

    [Fact]
    public void ConstructorLoadsTheMapButLobbyPumpDoesNotAdvanceGameplay()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0,
            IwadBytes = wad,
            Pregame = new PregameHostOptions
            {
                Session = new PregameSessionSnapshot
                {
                    MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 3 },
                },
            },
        });

        Assert.NotNull(host.Simulation);
        var player = Assert.Single(host.Simulation!.Players);
        Assert.Equal(100, player.X.ToDouble());
        Assert.Equal(200, player.Y.ToDouble());
        var checksum = host.Simulation.Checksum;
        host.Pump(1);
        Assert.Equal(0, host.Simulation.Thinkers.Clock.Tic);
        Assert.Equal(checksum, host.Simulation.Checksum);
    }

    [Fact]
    public void Publish_TailCarriesTheTickedPlayerPosition()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var simulation, out var error, rngSeed: 3), error);
        Assert.NotNull(simulation);
        var store = new GuestWorldStateStore();
        SimSnapshotPublisher.Publish(simulation!, store);

        var before = new byte[512];
        var first = WorldStateTailBuilder.TryBuildCoopTailFromStore(before, store, gameTic: 1);
        Assert.True(first.HasTail);
        Assert.Equal(100, store.Players[0].PosX);
        Assert.Equal(200, store.Players[0].PosY);

        simulation.QueueCommand(0, new PlayerCommand { ForwardMove = 2048 });
        var checksum = simulation.Checksum;
        simulation.Tick();
        Assert.NotEqual(checksum, simulation.Checksum);
        SimSnapshotPublisher.Publish(simulation, store);

        var after = new byte[512];
        var second = WorldStateTailBuilder.TryBuildCoopTailFromStore(after, store, gameTic: 1);
        Assert.True(second.HasTail);
        Assert.NotEqual(before.AsSpan(0, first.BytesWritten).ToArray(), after.AsSpan(0, second.BytesWritten).ToArray());
        Assert.True(store.Players[0].PosY > 200);
    }

    [Fact]
    public void Acceptance_MinimalMapLoadsSpawnsAndTicks()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(HeadlessMapBoot.TryBoot(wad, "MAP01", out var simulation, out var error), error);
        Assert.Contains(simulation!.Actors, actor => actor.DoomEdNum == 1);
        var checksum = simulation.Checksum;
        simulation.Tick();
        Assert.NotEqual(checksum, simulation.Checksum);
        Assert.True(simulation.Thinkers.Clock.Tic > 1);
    }
}
