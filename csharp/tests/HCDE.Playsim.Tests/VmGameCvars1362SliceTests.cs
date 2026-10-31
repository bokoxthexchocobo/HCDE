using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmGameCvars1362SliceTests
{
    [Fact]
    public void DefaultsSyncAndCorpseQueues()
    {
        var slice = new VmGameCvars1362Slice();
        Assert.False(slice.UseCallback);
        Assert.True(slice.CallbacksInstalled);
        Assert.Equal(VmGameCvars1362Slice.GsStartup, slice.GameState);
        Assert.True(slice.SpreadDecals);
        Assert.True(slice.Pushers);
        Assert.True(slice.CacheNodes);
        Assert.Equal(0.6f, slice.CacheTime);
        Assert.False(slice.AlwaysApplyDmFlags);
        Assert.True(slice.Friction);
        Assert.Equal(1f, slice.ScaleWeaponFov);
        Assert.Equal(640, slice.TurnWalkFast);
        Assert.Equal(1280, slice.TurnSprintFast);
        Assert.Equal(320, slice.TurnWalkSlow);
        Assert.Equal(320, slice.TurnSprintSlow);
        Assert.True(slice.GlLights);
        Assert.True(slice.RDynLights);
        Assert.False(slice.SyncingDynamicLights);
        Assert.Equal(1, slice.CorpseFilter);
        Assert.Equal(64, slice.CorpseQueueSize);
        Assert.Equal(1024, slice.MaxDecals);
        Assert.Equal(4000, slice.MaxParticles);
        Assert.Equal(0f, slice.TeamDamage);
        Assert.Equal(1, VmGameCvars1362Slice.CvarArchive);
        Assert.Equal(4, VmGameCvars1362Slice.CvarServerInfo);
        Assert.Equal(512, VmGameCvars1362Slice.CvarNoInitCall);
        Assert.Equal(1024, VmGameCvars1362Slice.CvarGlobalConfig);
        Assert.Equal(1, VmGameCvars1362Slice.SpreadDecalsFlags);
        Assert.Equal(4, VmGameCvars1362Slice.PushersFlags);
        Assert.Equal(1025, VmGameCvars1362Slice.CacheNodesFlags);
        Assert.Equal(1025, VmGameCvars1362Slice.CacheTimeFlags);
        Assert.Equal(4, VmGameCvars1362Slice.AlwaysApplyDmFlagsFlags);
        Assert.Equal(4, VmGameCvars1362Slice.FrictionFlags);
        Assert.Equal(1025, VmGameCvars1362Slice.TurnSpeedFlags);
        Assert.Equal(1537, VmGameCvars1362Slice.GlLightsFlags);
        Assert.Equal(517, VmGameCvars1362Slice.CorpseFlags);
        Assert.Equal(513, VmGameCvars1362Slice.MaxDecalsFlags);
        Assert.Equal(513, VmGameCvars1362Slice.MaxParticlesFlags);
        Assert.Equal(516, VmGameCvars1362Slice.TeamDamageFlags);
        Assert.Equal(1025, VmGameCvars1362Slice.ScaleWeaponFovFlags);
        Assert.Equal(0, VmGameCvars1362Slice.GsLevel);
        Assert.Equal(6, VmGameCvars1362Slice.GsStartup);
        Assert.Equal(7, VmGameCvars1362Slice.GsTitleLevel);
        Assert.Equal(2, VmGameCvars1362Slice.StatAutoDecal);
        Assert.Equal(32, VmGameCvars1362Slice.BodyQueSize);

        var lights = 0;
        slice.RecreateAllAttachedLights = _ => lights++;
        slice.DeleteAllAttachedLights = _ => lights++;
        slice.EnableCallbacks();
        Assert.True(slice.UseCallback);
        Assert.Equal(0, lights);
        Assert.Equal(640, slice.TurnWalkFast);
        Assert.Equal(1280, slice.TurnSprintFast);
        Assert.Equal(1, slice.CorpseFilter);
        Assert.Equal(64, slice.CorpseQueueSize);
        Assert.True(slice.GlLights);

        slice.SetSpreadDecals(false);
        Assert.False(slice.SpreadDecals);
        slice.SetCacheNodes(false);
        Assert.False(slice.CacheNodes);
        slice.SetCacheTime(1.25f);
        Assert.Equal(1.25f, slice.CacheTime);
        slice.SetScaleWeaponFov(0.5f);
        Assert.Equal(0.5f, slice.ScaleWeaponFov);

        slice.GameState = VmGameCvars1362Slice.GsLevel;
        slice.SetPushers(false);
        Assert.True(slice.Pushers);
        Assert.Equal("var_pushers", Assert.Single(slice.SwallowedSets));
        slice.SetAlwaysApplyDmFlags(true);
        Assert.False(slice.AlwaysApplyDmFlags);
        slice.SetFriction(false);
        Assert.True(slice.Friction);

        slice.Messages.Clear();
        slice.SwallowedSets.Clear();
        slice.NetGame = true;
        slice.LocalCanControlSettings = false;
        slice.SetPushers(false);
        Assert.True(slice.Pushers);
        Assert.Equal(
            "Only setting controllers or server/RCON console can change server CVAR var_pushers\n",
            Assert.Single(slice.Messages));
        Assert.Equal("var_pushers", Assert.Single(slice.SwallowedSets));

        slice.NetGame = false;
        slice.LocalCanControlSettings = true;
        slice.DemoPlayback = true;
        slice.SwallowedSets.Clear();
        slice.SetFriction(false);
        Assert.False(slice.Friction);
        Assert.Empty(slice.SwallowedSets);
        slice.DemoPlayback = false;
        slice.SaveGameRestore = true;
        slice.SetAlwaysApplyDmFlags(true);
        Assert.True(slice.AlwaysApplyDmFlags);
        slice.SaveGameRestore = false;
        slice.CallbacksInstalled = false;
        slice.SetPushers(false);
        Assert.False(slice.Pushers);

        var missing = new VmGameCvars1362Slice { UseCallback = true };
        var room = new VmGameCvars1362Slice.Level();
        missing.Levels.Add(room);
        var deleteMissing = Assert.Throws<InvalidOperationException>(() => missing.SetGlLights(false));
        Assert.Equal("DeleteAllAttachedLights", deleteMissing.Message);
        Assert.False(missing.GlLights);
        Assert.False(missing.RDynLights);
        Assert.False(missing.SyncingDynamicLights);
        var recreateMissing = Assert.Throws<InvalidOperationException>(() => missing.SetGlLights(true));
        Assert.Equal("RecreateAllAttachedLights", recreateMissing.Message);
        Assert.True(missing.GlLights);
        Assert.False(missing.SyncingDynamicLights);

        var sync = new VmGameCvars1362Slice { UseCallback = true };
        var synced = new VmGameCvars1362Slice.Level();
        var deleted = new List<VmGameCvars1362Slice.Level>();
        var recreated = new List<VmGameCvars1362Slice.Level>();
        var sawSyncing = new List<bool>();
        sync.DeleteAllAttachedLights = level =>
        {
            sawSyncing.Add(sync.SyncingDynamicLights);
            deleted.Add(level);
        };
        sync.RecreateAllAttachedLights = level =>
        {
            sawSyncing.Add(sync.SyncingDynamicLights);
            recreated.Add(level);
        };
        sync.Levels.Add(synced);
        sync.SyncDynamicLights(true);
        Assert.True(sync.GlLights);
        Assert.True(sync.RDynLights);
        Assert.False(sync.SyncingDynamicLights);
        Assert.Empty(deleted);
        Assert.Empty(recreated);

        sync.SyncDynamicLights(false);
        Assert.False(sync.GlLights);
        Assert.False(sync.RDynLights);
        Assert.False(sync.SyncingDynamicLights);
        Assert.Same(synced, Assert.Single(deleted));
        Assert.True(Assert.Single(sawSyncing));

        sawSyncing.Clear();
        sync.SyncDynamicLights(true);
        Assert.True(sync.GlLights);
        Assert.True(sync.RDynLights);
        Assert.False(sync.SyncingDynamicLights);
        Assert.Same(synced, Assert.Single(recreated));
        Assert.True(Assert.Single(sawSyncing));

        sync.RDynLights = false;
        recreated.Clear();
        sync.SetGlLights(true);
        Assert.True(sync.RDynLights);
        Assert.Same(synced, Assert.Single(recreated));
        Assert.False(sync.SyncingDynamicLights);

        sync.Levels.Add(null);
        Assert.Throws<NullReferenceException>(() => sync.SyncDynamicLights(false));
        Assert.False(sync.GlLights);
        Assert.True(sync.SyncingDynamicLights);

        var (corpses, destroyed) = Ready();
        var pile = new VmGameCvars1362Slice.Level();
        var first = new object();
        var third = new object();
        pile.CorpseQueue.Add(first);
        pile.CorpseQueue.Add(null);
        pile.CorpseQueue.Add(third);
        var bare = new VmGameCvars1362Slice();
        bare.TrimMonsterCorpseQueue(pile, -1);
        Assert.Equal(3, pile.CorpseQueue.Count);
        Assert.Throws<InvalidOperationException>(() => bare.TrimMonsterCorpseQueue(pile, 1));
        corpses.TrimMonsterCorpseQueue(pile, 1);
        Assert.Same(third, Assert.Single(pile.CorpseQueue));
        Assert.Same(first, Assert.Single(destroyed));

        var bodies = new VmGameCvars1362Slice.Level();
        var keep = new object();
        var drop = new object();
        var tail = new object();
        bodies.BodyQue[4] = keep;
        bodies.BodyQue[5] = drop;
        bodies.BodyQue[31] = tail;
        bodies.BodyQueSlot = 5;
        destroyed.Clear();
        corpses.TrimPlayerBodyQueue(bodies, 5);
        Assert.Same(keep, bodies.BodyQue[4]);
        Assert.Null(bodies.BodyQue[5]);
        Assert.Null(bodies.BodyQue[31]);
        Assert.Equal(0, bodies.BodyQueSlot);
        Assert.Equal(2, destroyed.Count);
        Assert.Same(drop, destroyed[0]);
        Assert.Same(tail, destroyed[1]);
        bodies.BodyQueSlot = 4;
        corpses.TrimPlayerBodyQueue(bodies, 5);
        Assert.Equal(4, bodies.BodyQueSlot);
        bodies.BodyQue[31] = tail;
        bodies.BodyQueSlot = 40;
        destroyed.Clear();
        corpses.TrimPlayerBodyQueue(bodies, 40);
        Assert.Same(tail, bodies.BodyQue[31]);
        Assert.Equal(8, bodies.BodyQueSlot);
        Assert.Empty(destroyed);
        bodies.BodyQue[4] = null;
        bodies.BodyQue[31] = null;
        bodies.BodyQue[0] = keep;
        bodies.BodyQueSlot = 9;
        destroyed.Clear();
        corpses.TrimPlayerBodyQueue(bodies, 0);
        Assert.Null(bodies.BodyQue[0]);
        Assert.Equal(0, bodies.BodyQueSlot);
        Assert.Same(keep, Assert.Single(destroyed));

        var live = Ready().Slice;
        live.GameState = VmGameCvars1362Slice.GsLevel;
        var primary = new VmGameCvars1362Slice.Level();
        var monsters = new VmGameCvars1362Slice.Level();
        var players = new VmGameCvars1362Slice.Level();
        var monsterA = new object();
        var monsterB = new object();
        var body = new object();
        monsters.CorpseQueue.Add(monsterA);
        monsters.CorpseQueue.Add(monsterB);
        players.BodyQue[0] = body;
        players.BodyQueSlot = 3;
        live.PrimaryLevel = primary;
        live.Levels.Add(monsters);
        live.Levels.Add(null);
        live.Levels.Add(players);
        live.ForceCorpseQueueSize(0);
        Assert.Equal(0, live.CorpseQueueSize);
        Assert.Empty(monsters.CorpseQueue);
        Assert.Same(body, players.BodyQue[0]);
        Assert.Equal(3, players.BodyQueSlot);
        Assert.Equal(1, live.ApplyEntered);

        monsters.CorpseQueue.Add(monsterA);
        live.ForceCorpseQueueSize(-1);
        Assert.Equal(-1, live.CorpseQueueSize);
        Assert.Empty(monsters.CorpseQueue);
        Assert.Equal(2, live.ApplyEntered);
        Assert.Contains("sv_corpsequeuesize", live.SwallowedSets);

        var titled = Ready().Slice;
        titled.GameState = VmGameCvars1362Slice.GsTitleLevel;
        titled.PrimaryLevel = primary;
        var titledMonsters = new VmGameCvars1362Slice.Level();
        titledMonsters.CorpseQueue.Add(monsterA);
        titledMonsters.CorpseQueue.Add(monsterB);
        titled.Levels.Add(titledMonsters);
        titled.ForceCorpseQueueSize(1);
        Assert.Same(monsterB, Assert.Single(titledMonsters.CorpseQueue));

        var both = Ready().Slice;
        both.GameState = VmGameCvars1362Slice.GsLevel;
        both.PrimaryLevel = primary;
        var bothMonsters = new VmGameCvars1362Slice.Level();
        var bothPlayers = new VmGameCvars1362Slice.Level();
        bothMonsters.CorpseQueue.Add(monsterA);
        bothPlayers.BodyQue[2] = body;
        bothPlayers.BodyQueSlot = 4;
        both.Levels.Add(bothMonsters);
        both.Levels.Add(bothPlayers);
        both.ForceCorpseQueueSize(0);
        Assert.Empty(bothMonsters.CorpseQueue);
        Assert.Same(body, bothPlayers.BodyQue[2]);
        bothMonsters.CorpseQueue.Add(monsterA);
        both.ForceCorpseFilter(5);
        Assert.Equal(5, both.CorpseFilter);
        Assert.Empty(bothMonsters.CorpseQueue);
        Assert.Null(bothPlayers.BodyQue[2]);
        Assert.Equal(0, bothPlayers.BodyQueSlot);
        Assert.Equal(2, both.ApplyEntered);

        both.ForceCorpseFilter(2);
        Assert.Equal(2, both.CorpseFilter);
        bothMonsters.CorpseQueue.Add(monsterA);
        both.ForceCorpseQueueSize(0);
        Assert.Same(monsterA, Assert.Single(bothMonsters.CorpseQueue));

        var startup = Ready().Slice;
        startup.PrimaryLevel = primary;
        var startupMonsters = new VmGameCvars1362Slice.Level();
        startupMonsters.CorpseQueue.Add(monsterA);
        startupMonsters.CorpseQueue.Add(monsterB);
        startupMonsters.CorpseQueue.Add(third);
        startup.Levels.Add(startupMonsters);
        startup.ForceCorpseQueueSize(-1);
        Assert.Equal(0, startup.CorpseQueueSize);
        Assert.Equal(3, startupMonsters.CorpseQueue.Count);
        Assert.Equal(2, startup.ApplyEntered);

        var overflow = Ready().Slice;
        var overflowed = Assert.Throws<InvalidOperationException>(() => overflow.ForceCorpseFilter(4));
        Assert.Equal("sv_corpsefilter", overflowed.Message);
        Assert.Equal(3, overflow.CorpseFilter);
        Assert.Equal(0, overflow.ApplyEntered);
        var same = Assert.Throws<InvalidOperationException>(() => overflow.ForceCorpseFilter(2));
        Assert.Equal("sv_corpsefilter", same.Message);
        Assert.Equal(2, overflow.CorpseFilter);

        var demo = Ready().Slice;
        demo.GameState = VmGameCvars1362Slice.GsLevel;
        demo.DemoPlayback = true;
        var demoFail = Assert.Throws<InvalidOperationException>(() => demo.ForceCorpseFilter(2));
        Assert.Equal("sv_corpsefilter", demoFail.Message);
        Assert.Equal(2, demo.CorpseFilter);

        var closed = Ready().Slice;
        closed.GameState = VmGameCvars1362Slice.GsLevel;
        closed.CallbacksInstalled = false;
        var closedFail = Assert.Throws<InvalidOperationException>(() => closed.SetCorpseFilter(2));
        Assert.Equal("sv_corpsefilter", closedFail.Message);

        var pregame = Ready().Slice;
        pregame.GameState = VmGameCvars1362Slice.GsLevel;
        pregame.NetGame = true;
        pregame.GameTic = 0;
        pregame.PrimaryLevel = primary;
        var pregameMonsters = new VmGameCvars1362Slice.Level();
        pregameMonsters.CorpseQueue.Add(monsterA);
        pregame.Levels.Add(pregameMonsters);
        pregame.ForceCorpseQueueSize(3);
        Assert.Equal(3, pregame.CorpseQueueSize);
        Assert.Equal(0, pregame.ApplyEntered);
        Assert.Same(monsterA, Assert.Single(pregameMonsters.CorpseQueue));
        pregame.ApplyCorpseFilter();
        Assert.Equal(1, pregame.ApplyEntered);
        Assert.Same(monsterA, Assert.Single(pregameMonsters.CorpseQueue));

        var idle = Ready().Slice;
        idle.GameState = VmGameCvars1362Slice.GsLevel;
        var idleMonsters = new VmGameCvars1362Slice.Level();
        idleMonsters.CorpseQueue.Add(monsterA);
        idle.Levels.Add(idleMonsters);
        idle.ApplyCorpseFilter();
        Assert.Same(monsterA, Assert.Single(idleMonsters.CorpseQueue));
        idle.PrimaryLevel = primary;
        idle.GameState = 1;
        idle.ApplyCorpseFilter();
        Assert.Same(monsterA, Assert.Single(idleMonsters.CorpseQueue));
        Assert.Equal(2, idle.ApplyEntered);
    }

    [Fact]
    public void DecalsParticlesTeamAndTurnSpeeds()
    {
        var (slice, destroyed) = Ready();
        slice.GameState = VmGameCvars1362Slice.GsLevel;
        var level = new VmGameCvars1362Slice.Level();
        var d1 = new object();
        var d2 = new object();
        var d3 = new object();
        level.AutoDecals.Add(d1);
        level.AutoDecals.Add(d2);
        level.AutoDecals.Add(d3);
        level.ImpactDecalCount = 3;
        slice.Levels.Add(level);
        slice.SetMaxDecals(-5);
        Assert.Equal(0, slice.MaxDecals);
        Assert.Equal(0, level.ImpactDecalCount);
        Assert.Empty(level.AutoDecals);
        Assert.Equal(3, destroyed.Count);
        Assert.Same(d1, destroyed[0]);
        Assert.Same(d3, destroyed[2]);

        level.AutoDecals.Add(d1);
        level.AutoDecals.Add(d2);
        level.ImpactDecalCount = 2;
        destroyed.Clear();
        slice.SetMaxDecals(0);
        Assert.Equal(0, slice.MaxDecals);
        Assert.Empty(level.AutoDecals);
        Assert.Equal(2, destroyed.Count);

        level.AutoDecals.Add(d1);
        level.AutoDecals.Add(d2);
        level.AutoDecals.Add(d3);
        level.ImpactDecalCount = 3;
        slice.SetMaxDecals(1);
        Assert.Equal(1, slice.MaxDecals);
        Assert.Equal(1, level.ImpactDecalCount);
        Assert.Same(d3, Assert.Single(level.AutoDecals));

        level.AutoDecals.Clear();
        level.AutoDecals.Add(d1);
        level.ImpactDecalCount = 3;
        var stalled = Assert.Throws<InvalidOperationException>(() => slice.SetMaxDecals(-1));
        Assert.Equal("STAT_AUTODECAL", stalled.Message);
        Assert.Equal(0, slice.MaxDecals);
        Assert.Equal(2, level.ImpactDecalCount);
        Assert.Empty(level.AutoDecals);

        var seenStat = -1;
        slice.FirstThinker = (_, stat) =>
        {
            seenStat = stat;
            return null;
        };
        level.ImpactDecalCount = 1;
        var missingThinker = Assert.Throws<InvalidOperationException>(() => slice.SetMaxDecals(0));
        Assert.Equal("STAT_AUTODECAL", missingThinker.Message);
        Assert.Equal(VmGameCvars1362Slice.StatAutoDecal, seenStat);

        var nullLevel = Ready().Slice;
        nullLevel.GameState = VmGameCvars1362Slice.GsLevel;
        nullLevel.Levels.Add(null);
        Assert.Throws<NullReferenceException>(() => nullLevel.SetMaxDecals(-1));
        Assert.Equal(0, nullLevel.MaxDecals);

        var particles = new VmGameCvars1362Slice { UseCallback = true, GameState = VmGameCvars1362Slice.GsLevel };
        var inits = 0;
        var particleLevel = new VmGameCvars1362Slice.Level();
        particles.Levels.Add(particleLevel);
        particles.InitParticles = _ => inits++;
        particles.SetMaxParticles(0);
        Assert.Equal(4000, particles.MaxParticles);
        Assert.Equal(2, inits);
        particles.SetMaxParticles(99);
        Assert.Equal(100, particles.MaxParticles);
        Assert.Equal(4, inits);
        particles.SetMaxParticles(100);
        Assert.Equal(100, particles.MaxParticles);
        Assert.Equal(5, inits);
        particles.SetMaxParticles(65535);
        Assert.Equal(65535, particles.MaxParticles);
        Assert.Equal(6, inits);
        particles.SetMaxParticles(65536);
        Assert.Equal(65535, particles.MaxParticles);
        Assert.Equal(8, inits);
        particles.SetMaxParticles(-1);
        Assert.Equal(100, particles.MaxParticles);
        Assert.Equal(10, inits);

        var startup = new VmGameCvars1362Slice { UseCallback = true };
        startup.Levels.Add(particleLevel);
        startup.SetMaxParticles(0);
        Assert.Equal(4000, startup.MaxParticles);
        startup.GameState = VmGameCvars1362Slice.GsLevel;
        startup.Levels.Clear();
        startup.SetMaxParticles(200);
        Assert.Equal(200, startup.MaxParticles);
        startup.Levels.Add(particleLevel);
        var initMissing = Assert.Throws<InvalidOperationException>(() => startup.SetMaxParticles(150));
        Assert.Equal("P_InitParticles", initMissing.Message);
        Assert.Equal(150, startup.MaxParticles);

        var team = new VmGameCvars1362Slice { UseCallback = true, GameState = VmGameCvars1362Slice.GsLevel };
        var alpha = new VmGameCvars1362Slice.Level();
        var beta = new VmGameCvars1362Slice.Level();
        team.Levels.Add(alpha);
        team.Levels.Add(beta);
        team.SetTeamDamage(-1.25f);
        Assert.Equal(0f, team.TeamDamage);
        Assert.Equal(0f, alpha.TeamDamage);
        Assert.Contains("teamdamage", team.SwallowedSets);
        team.ForceTeamDamage(-1.25f);
        Assert.Equal(-1.25f, team.TeamDamage);
        Assert.Equal(-1.25f, alpha.TeamDamage);
        Assert.Equal(-1.25f, beta.TeamDamage);
        team.GameState = VmGameCvars1362Slice.GsStartup;
        team.SetTeamDamage(0.5f);
        Assert.Equal(0.5f, alpha.TeamDamage);
        Assert.Equal(0.5f, beta.TeamDamage);
        team.Levels.Add(null);
        Assert.Throws<NullReferenceException>(() => team.ForceTeamDamage(2f));
        Assert.Equal(2f, team.TeamDamage);

        var turns = new VmGameCvars1362Slice { UseCallback = true };
        turns.SetTurnWalkFast(0);
        Assert.Equal(1, turns.TurnWalkFast);
        turns.SetTurnWalkFast(-4);
        Assert.Equal(1, turns.TurnWalkFast);
        turns.SetTurnSprintFast(12);
        Assert.Equal(12, turns.TurnSprintFast);
        turns.SetTurnWalkSlow(1);
        Assert.Equal(1, turns.TurnWalkSlow);
        turns.SetTurnSprintSlow(0);
        Assert.Equal(1, turns.TurnSprintSlow);
        turns.SetTurnWalkFast(8);
        Assert.Equal(8, turns.TurnWalkFast);
    }

    static (VmGameCvars1362Slice Slice, List<object> Destroyed) Ready()
    {
        var slice = new VmGameCvars1362Slice { UseCallback = true };
        var destroyed = new List<object>();
        slice.DestroyActor = destroyed.Add;
        return (slice, destroyed);
    }
}
