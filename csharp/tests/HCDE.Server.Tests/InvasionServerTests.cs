using System.Buffers.Binary;
using System.Net;
using HCDE.MapLoader;
using HCDE.MapLoader.Tests;
using HCDE.Net.Core;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class InvasionServerTests
{
    [Fact]
    public void SchedulerProduces35TicsPerSecondWithoutRoundingDrift()
    {
        var scheduler = new ServerTicScheduler(0);
        var count = 0;
        for (ulong milliseconds = 0; milliseconds <= 60_000; milliseconds += 10)
            count += scheduler.TakeDueTics(milliseconds);
        Assert.Equal(2100, count);
        Assert.Equal(0, scheduler.TakeDueTics(60_000));
        Assert.Equal(0, scheduler.TakeDueTics(59_000));
    }

    [Fact]
    public void SchedulerRetainsCatchupDebtAcrossBoundedBatches()
    {
        var scheduler = new ServerTicScheduler(100);
        var count = scheduler.TakeDueTics(1100);
        Assert.Equal(8, count);
        for (var i = 0; i < 4; i++) count += scheduler.TakeDueTics(1100);
        Assert.Equal(35, count);
        Assert.Equal(0, scheduler.TakeDueTics(1100));
    }

    [Fact]
    public void ConfiguredDirectorCountsDownAndStopsAfterFinalWave()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        sim.Invasion.Configure(2, 3, 2);
        sim.Invasion.Enabled = true;
        sim.Tick();
        Assert.Equal(InvasionPhase.Countdown, sim.Invasion.Phase);
        Assert.Equal(3, sim.Invasion.Cooldown);
        sim.Tick(); sim.Tick();
        Assert.Equal(0, sim.Invasion.Wave);
        sim.Tick();
        Assert.Equal(1, sim.Invasion.Wave);
        sim.Actors.OfType<BotPawn>().Single().Health = 0;
        sim.Tick();
        Assert.Equal(2, sim.Invasion.Cooldown);
        sim.Tick(); sim.Tick();
        Assert.Equal(2, sim.Invasion.Wave);
        sim.Actors.OfType<BotPawn>().Single(b => !b.IsDead).Health = 0;
        sim.Tick();
        Assert.Equal(InvasionPhase.Victory, sim.Invasion.Phase);
        for (var i = 0; i < 100; i++) sim.Tick();
        Assert.Equal(2, sim.Invasion.Wave);
        Assert.Equal(0, sim.Invasion.ActiveMonsters);
    }

    [Fact]
    public void QueryAndSnapshotUseTheSameDirectorValuesAndWireStateIds()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        sim.Invasion.Configure(1, 2, 1);
        sim.Invasion.Enabled = true;
        for (var i = 0; i < 5; i++)
        {
            sim.Tick();
            var header = InvasionSnapshotPublisher.Capture(sim.Invasion);
            var query = new ServerQuerySnapshot();
            InvasionSnapshotPublisher.ApplyToQuery(sim.Invasion, query);
            Assert.Equal(header.State, query.InvasionState);
            Assert.Equal(header.StateTics, (uint)query.InvasionStateTics);
            Assert.Equal(header.Wave, (uint)query.InvasionWave);
            Assert.Equal(header.ActiveMonsters, (uint)query.InvasionActiveMonsters);
            if (sim.Invasion.Phase == InvasionPhase.Wave)
            {
                Assert.Equal(LiveConstants.InvasionStateCleanup, header.State);
                sim.Actors.OfType<BotPawn>().Single().Health = 0;
            }
        }
        Assert.Equal(LiveConstants.InvasionStateVictory, InvasionSnapshotPublisher.Capture(sim.Invasion).State);
    }

    [Fact]
    public async Task HostRoutesNetworkAttacksAndPublishesWaveCompletion()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var map, out _));
        Assert.True(map.TryGetLump(MapLumpKind.Things, out var things));
        var offset = (int)things.Entry.FilePosition;
        BinaryPrimitives.WriteInt16LittleEndian(wad.AsSpan(offset), -100);
        BinaryPrimitives.WriteInt16LittleEndian(wad.AsSpan(offset + 2), 64);
        BinaryPrimitives.WriteInt16LittleEndian(wad.AsSpan(offset + 4), 0);
        BinaryPrimitives.WriteInt16LittleEndian(wad.AsSpan(offset + 6), 2); // guest slot 1
        var gameId = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18 };
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0, IwadBytes = wad, GameMode = 4, InvasionWaves = 1, InvasionCountdownTics = 2,
            Pregame = new PregameHostOptions
            {
                GameId = gameId, ExpectedEngineInfo = new EngineInfoSnapshot(),
                Session = new PregameSessionSnapshot
                {
                    MapLoad = new MapLoadInfo { MapName = "MAP01", RngSeed = 3 },
                    GameInfo = new GameInfoPayload { GameId = gameId },
                },
            },
        });
        using var transport = new UdpTransport();
        transport.Bind(0); transport.SetNonBlocking(true);
        var endpoint = new NetworkEndpoint(IPAddress.Loopback, host.BoundPort);
        var guest = new PregameGuest(transport, new PregameGuestOptions
        {
            ServerAddress = endpoint, EngineInfo = new EngineInfoSnapshot(),
        });
        for (var i = 0; i < 5; i++) host.Pump((ulong)Environment.TickCount64);
        Assert.Equal(0, host.Simulation!.Invasion.Wave); // do not start an empty lobby
        var deadline = Environment.TickCount64 + 5000;
        while (host.LiveSession == null && Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            guest.Pump(now);
            host.Pump(now);
            if (!host.PregameHost.StartGameSent && guest.Phase == PregameGuestPhase.Ready
                && host.PregameHost.Clients.Any(c => c.ClientSlot == guest.AssignedClientSlot && c.Status == ConnectionStatus.Ready))
                host.PregameHost.StartGame(now);
            await Task.Delay(1);
        }
        Assert.NotNull(host.LiveSession);
        Assert.Equal(1, guest.AssignedClientSlot);

        // Clear setup/control packets before switching to the live endpoint.
        var drain = new byte[NetConstants.MaxTransmitSize];
        while (transport.TryReceive(drain, out _, out _, TimeSpan.Zero)) { }
        var world = new GuestWorldStateStore();
        var live = new LiveGuestSession(transport, gameId, endpoint, 1, 0, 8);
        live.SetGuestWorldState(world, new SnapshotChecksumSession(), rngSeed: 3);
        var input = new LiveGameplayEndpoint(transport, gameId);

        void TickAndReceive()
        {
            host.Pump((ulong)Environment.TickCount64);
            Assert.True(live.TryReceiveAuthorityControl(out _));
            Assert.True(live.TryReceiveServerSnapshot(out _, out _, out _));
            Assert.Equal(host.Simulation.Invasion.Wave, live.InvasionState!.MirrorState.Wave);
            Assert.Equal((uint)host.Simulation.Invasion.Cooldown, live.InvasionState.StateTics);
            Assert.Equal((uint)host.Simulation.Invasion.ActiveMonsters, live.InvasionState.ActiveMonsters);
        }
        while (host.Simulation.Invasion.Wave == 0) TickAndReceive();
        var enemy = host.Simulation.Actors.OfType<BotPawn>().Single();
        Assert.Equal(30, world.Actors[enemy.Id].Health);
        Assert.Equal(enemy.X.ToDouble(), world.Actors[enemy.Id].PosX, 2);
        for (uint shot = 1; shot <= 3; shot++)
        {
            while (host.Simulation.Players.Single().WeaponCooldown > 1) TickAndReceive();
            Assert.True(input.TrySendClientInput(endpoint, 0, shot, 1, new UserCmd(1, 0, 0, 0, 0, 0, 0)));
            TickAndReceive();
            Assert.Equal(30 - 10 * (int)shot, enemy.Health);
            Assert.Equal(enemy.Health, world.Actors[enemy.Id].Health);
        }
        Assert.Equal(47, host.Simulation.Players.Single().Inventory.Bullets);
        Assert.Equal(LiveConstants.InvasionStateVictory, live.InvasionState!.MirrorState.State);
        Assert.Equal(1u, live.InvasionState.MirrorState.WaveCleared);
        Assert.Equal(0u, live.InvasionState.ActiveMonsters);
        Assert.Equal(0, world.Actors[enemy.Id].Flags & LiveConstants.ActorDeltaFlagLive);
        Assert.True(input.TrySendClientInput(endpoint, 0, 4, 1, new UserCmd(4, 0, 0, 0, 0, 0, 0)));
        TickAndReceive();
        var jumping = host.Simulation.Players.Single();
        Assert.False(world.Players[1].OnGround);
        Assert.True(world.Players[1].PosZ > 0);
        Assert.Equal(jumping.Z.ToDouble(), world.Players[1].PosZ, 2);
        Assert.Equal(jumping.VelocityZ.ToDouble(), world.Players[1].VelZ, 2);
        Assert.Equal(jumping.Z.ToDouble(), world.Actors[jumping.Id].PosZ, 2);
        jumping.Inventory.Weapons |= WeaponKind.Plasma;
        jumping.Inventory.Selected = WeaponKind.Plasma;
        jumping.Inventory.Cells = 1;
        while (jumping.WeaponCooldown > 1) TickAndReceive();
        Assert.True(input.TrySendClientInput(endpoint, 0, 5, 1, new UserCmd(1, 0, 0, 0, 0, 0, 0)));
        TickAndReceive();
        var projectile = Assert.Single(host.Simulation.Actors.OfType<ProjectileActor>());
        Assert.Equal((byte)ReplicatedActorCategory.Projectile, world.Actors[projectile.Id].Category);
        var startX = world.Actors[projectile.Id].PosX;
        TickAndReceive();
        Assert.True(world.Actors[projectile.Id].PosX > startX);
        projectile.Destroy(); TickAndReceive();
        Assert.Equal(0, world.Actors[projectile.Id].Flags & LiveConstants.ActorDeltaFlagLive);
    }

    [Fact]
    public void RemovedActorsRemainNonLiveInSubsequentSnapshots()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        var bot = sim.AddBot(0, 0);
        var store = new GuestWorldStateStore();
        SimSnapshotPublisher.Publish(sim, store);
        Assert.Equal(LiveConstants.ActorDeltaFlagLive, store.Actors[bot.Id].Flags);
        bot.Destroy(); sim.Tick();
        SimSnapshotPublisher.Publish(sim, store);
        Assert.Equal(0, store.Actors[bot.Id].Flags & LiveConstants.ActorDeltaFlagLive);
        Assert.Equal(0, store.Actors[bot.Id].Health);
        var next = sim.AddBot(0, 0);
        SimSnapshotPublisher.Publish(sim, store);
        Assert.NotEqual(bot.Id, next.Id);
        Assert.Equal(0, store.Actors[bot.Id].Health);
        Assert.Equal(30, store.Actors[next.Id].Health);
    }
}
