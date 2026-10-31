using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAiDirector1348SliceTests
{
    [Fact]
    public void DriveSweepAndClusters()
    {
        var clamp = new VmAiDirector1348();
        clamp.SetSweepTics(0);
        Assert.Equal(1, clamp.SweepTics);
        clamp.SetSweepTics(-4);
        Assert.Equal(1, clamp.SweepTics);
        clamp.SetSweepTics(1);
        Assert.Equal(1, clamp.SweepTics);
        clamp.SetSweepTics(70);
        Assert.Equal(70, clamp.SweepTics);
        clamp.SetSweepTics(71);
        Assert.Equal(70, clamp.SweepTics);
        clamp.SetSweepTics(7);
        Assert.Equal(7, clamp.SweepTics);

        var gate = new VmAiDirector1348 { LocalAuthority = true };
        Assert.False(gate.ShouldDriveAuthority());
        Assert.Equal(0, gate.AuthorityQueries);
        gate.Enable = true;
        gate.LocalAuthority = false;
        Assert.False(gate.ShouldDriveAuthority());
        Assert.Equal(1, gate.AuthorityQueries);
        gate.LocalAuthority = true;
        Assert.True(gate.ShouldDriveAuthority());
        Assert.Equal(2, gate.AuthorityQueries);
        Assert.Same(gate.State, gate.ReadState());
        Assert.Equal("AIDirector", VmAiDirector1348.RngStream);

        var cadence = Armed();
        cadence.Clock = 1000;
        cadence.ClockStep = 10;
        cadence.Level = new Level1348 { Time = 5 };
        for (int i = 0; i < 8; i++)
            cadence.Tick();
        Assert.Equal(8, cadence.State.TicksSinceStart);
        Assert.True(cadence.State.AuthorityActive);
        Assert.Equal(2, cadence.State.SweepCount);
        Assert.Equal(5, cadence.State.LastSweepTic);
        Assert.Equal(10, cadence.State.LastSweepWallclockMs);
        Assert.Equal(10, cadence.State.MaxSweepWallclockMs);
        Assert.Equal(1110, cadence.State.LastTickWallclockMs);
        Assert.Equal(1120ul, cadence.Clock);
        Assert.Equal(0, cadence.State.DeterministicRngDraws);

        ulong clockBeforeNull = cadence.Clock;
        long sweepMs = cadence.State.LastSweepWallclockMs;
        long maxMs = cadence.State.MaxSweepWallclockMs;
        cadence.Level = null;
        cadence.SetSweepTics(1);
        cadence.Tick();
        Assert.Equal(9, cadence.State.TicksSinceStart);
        Assert.Equal(3, cadence.State.SweepCount);
        Assert.Equal(0, cadence.State.LastSweepTic);
        Assert.Equal(0, cadence.State.MonsterCountObserved);
        Assert.Equal(sweepMs, cadence.State.LastSweepWallclockMs);
        Assert.Equal(maxMs, cadence.State.MaxSweepWallclockMs);
        Assert.Equal(clockBeforeNull + 20, cadence.Clock);
        Assert.Empty(cadence.Observed);
        Assert.Empty(cadence.Clusters);
        Assert.Empty(cadence.Hints);

        cadence.Enable = false;
        ulong frozen = cadence.Clock;
        cadence.Tick();
        Assert.Equal(9, cadence.State.TicksSinceStart);
        Assert.False(cadence.State.AuthorityActive);
        Assert.Equal(0, cadence.State.HintsIssuedThisTic);
        Assert.Equal(frozen, cadence.Clock);

        var every = Armed();
        every.SetSweepTics(1);
        every.Level = new Level1348 { Time = 1 };
        every.Tick();
        every.Tick();
        every.Tick();
        Assert.Equal(3, every.State.SweepCount);
        Assert.Equal(3, every.State.TicksSinceStart);

        var map = Armed();
        map.Level = new Level1348 { Time = 11 };
        var origin = Monster(0, 0);
        var north = Monster(0, 1000);
        var mid = Monster(0, 400);
        var upper = Monster(0, 800);
        var back = Monster(0, 200);
        var decoy = new Actor1348 { X = 0, Y = 0, Health = 100, Flags3 = 1 };
        var corpse = Monster(100000, 0, health: 50, flags: VmAiDirector1348.MfCorpse);
        var dead = Monster(100000, 50, health: 0);
        var sleeper = Monster(100000, 80, flags2: VmAiDirector1348.Mf2Dormant);
        var dormantDead = Monster(100000, 90, health: -2, flags2: VmAiDirector1348.Mf2Dormant);
        map.Level.Actors.AddRange(new[] { decoy, origin, north, mid, upper, back, corpse, dead, sleeper, dormantDead });
        map.Tick();

        Assert.Equal(9, map.State.MonsterCountObserved);
        Assert.Equal(6, map.State.AliveMonsterCountObserved);
        Assert.Equal(3, map.State.CorpseMonsterCountObserved);
        Assert.Equal(2, map.State.DormantMonsterCountObserved);
        Assert.Equal(3, map.State.GroupCountObserved);
        Assert.Equal(3, map.State.LargestGroupSizeObserved);
        Assert.Equal(6, map.Observed.Count);
        Assert.Equal(11, map.State.LastSweepTic);
        Assert.Empty(map.Hints);
        Assert.Equal(0, map.State.RegroupHintsStored);
        Assert.Equal(0, map.State.HintsIssuedTotal);

        Assert.Equal(3, map.Clusters[0].Count);
        Assert.Equal(0, map.Clusters[0].X);
        double recip = 1.0 / 3.0;
        Assert.Equal(600.0 * recip, map.Clusters[0].Y);
        Assert.Equal(2, map.Clusters[1].Count);
        Assert.Equal(0, map.Clusters[1].X);
        Assert.Equal(900, map.Clusters[1].Y);
        Assert.Equal(1, map.Clusters[2].Count);
        Assert.Equal(100000, map.Clusters[2].X);
        Assert.Equal(80, map.Clusters[2].Y);
        Assert.Same(sleeper, map.Observed[^1].Actor);
        Assert.Equal(2, map.Observed[^1].GroupId);
        Assert.Same(origin, map.Observed[0].Actor);
        Assert.Equal(0, map.Observed[0].GroupId);
        Assert.Same(north, map.Observed[1].Actor);
        Assert.Equal(1, map.Observed[1].GroupId);
        Assert.Same(upper, map.Observed[3].Actor);
        Assert.Equal(1, map.Observed[3].GroupId);

        var edge = Armed();
        edge.Level = new Level1348();
        edge.Level.Actors.Add(Monster(1, 1));
        edge.Level.Actors.Add(Monster(2, 4));
        edge.Tick();
        Assert.Equal(1, edge.State.GroupCountObserved);
        Assert.Equal(1.5, edge.Clusters[0].X);
        Assert.Equal(2.5, edge.Clusters[0].Y);

        var bound = Armed();
        bound.Level = new Level1348();
        bound.Level.Actors.Add(Monster(0, 0));
        bound.Level.Actors.Add(Monster(512, 0));
        bound.Tick();
        Assert.Equal(1, bound.State.GroupCountObserved);
        Assert.Equal(256, bound.Clusters[0].X);
        Assert.Equal(0, bound.Clusters[0].Y);
        Assert.Equal(2, bound.Clusters[0].Count);

        var outside = Armed();
        outside.Level = new Level1348();
        outside.Level.Actors.Add(Monster(0, 0));
        outside.Level.Actors.Add(Monster(513, 0));
        outside.Tick();
        Assert.Equal(2, outside.State.GroupCountObserved);
        Assert.Equal(1, outside.State.LargestGroupSizeObserved);
        Assert.Equal(513, outside.Clusters[1].X);
        Assert.Equal(1, outside.Clusters[0].Count);
    }

    [Fact]
    public void HintsStatusAndConsume()
    {
        var idle = new VmAiDirector1348();
        Assert.Equal(DefaultStatus, idle.AiStatus());
        Assert.Equal(0, idle.AuthorityQueries);

        var dir = Armed();
        dir.RegroupHint = true;
        dir.Clock = 1000;
        dir.ClockStep = 10;
        dir.Level = new Level1348 { Time = 42 };
        var a = Monster(0, 0);
        var b = Monster(0, 0);
        var c = Monster(0, 0);
        var far = Monster(1000, 0);
        var corpse = Monster(0, 0, flags: VmAiDirector1348.MfCorpse, health: 40);
        dir.Level.Actors.AddRange(new[] { a, b, c, far, corpse });
        dir.Tick();

        Assert.Equal(3, dir.State.HintsIssuedThisTic);
        Assert.Equal(3, dir.State.HintsIssuedTotal);
        Assert.Equal(3, dir.State.RegroupHintsStored);
        Assert.Equal(3, dir.Hints.Count);
        Assert.Equal(new[] { a, b, c }, dir.Hints.Select(hint => hint.Actor).ToArray());
        Assert.All(dir.Hints, hint => Assert.Equal(0, hint.GroupId));
        Assert.All(dir.Hints, hint => Assert.Equal(42, hint.StoredTic));
        Assert.Equal(3, dir.State.LargestGroupSizeObserved);
        Assert.Equal(3, dir.Clusters[0].Count);
        Assert.Equal(1, dir.Clusters[1].Count);
        Assert.DoesNotContain(dir.Hints, hint => ReferenceEquals(hint.Actor, far));
        Assert.DoesNotContain(dir.Hints, hint => ReferenceEquals(hint.Actor, corpse));
        Assert.Equal(0, dir.State.DeterministicRngDraws);
        Assert.Equal(PopulatedStatus, dir.AiStatus());

        dir.Tick();
        Assert.Equal(2, dir.State.TicksSinceStart);
        Assert.Equal(0, dir.State.HintsIssuedThisTic);
        Assert.Equal(3, dir.State.HintsIssuedTotal);
        Assert.Equal(3, dir.State.RegroupHintsStored);
        Assert.Equal(3, dir.Hints.Count);
        Assert.Equal(1, dir.State.SweepCount);

        dir.Enable = false;
        dir.Tick();
        Assert.Equal(2, dir.State.TicksSinceStart);
        Assert.False(dir.State.AuthorityActive);
        Assert.Equal(3, dir.Hints.Count);
        dir.Enable = true;
        dir.Level!.Time = 50;
        dir.Level.Actors.Clear();
        dir.Level.Actors.Add(a);
        dir.Level.Actors.Add(b);
        for (int i = 0; i < 4; i++)
            dir.Tick();
        Assert.Equal(6, dir.State.TicksSinceStart);
        Assert.Equal(1, dir.State.SweepCount);
        dir.Tick();
        Assert.Equal(7, dir.State.TicksSinceStart);
        Assert.Equal(2, dir.State.SweepCount);
        Assert.Equal(50, dir.State.LastSweepTic);
        Assert.Empty(dir.Hints);
        Assert.Equal(0, dir.State.HintsIssuedThisTic);
        Assert.Equal(3, dir.State.HintsIssuedTotal);
        Assert.Equal(0, dir.State.RegroupHintsStored);
        Assert.Equal(2, dir.State.LargestGroupSizeObserved);

        var use = Armed();
        use.RegroupHint = true;
        use.Level = new Level1348 { Time = 7 };
        var one = Monster(3, 6);
        var two = Monster(3, 6);
        var three = Monster(3, 6);
        use.Level.Actors.AddRange(new[] { one, two, three });
        use.Tick();
        Assert.Equal(3, use.Clusters[0].X);
        Assert.Equal(6, use.Clusters[0].Y);
        Assert.Equal(3, use.Clusters[0].Count);

        var group = new OutInt1348(-9);
        var tic = new OutInt1348(-8);
        Assert.False(use.ReadRegroupHint(null, group, tic));
        Assert.Equal(-9, group.Value);
        Assert.Equal(-8, tic.Value);
        Assert.False(use.ReadRegroupHint(Monster(3, 6), group, tic));
        Assert.Equal(-9, group.Value);
        Assert.True(use.ReadRegroupHint(one, null, tic));
        Assert.Equal(7, tic.Value);
        Assert.Equal(-9, group.Value);
        Assert.True(use.ReadRegroupHint(one, group, null));
        Assert.Equal(0, group.Value);

        Assert.True(use.ConsumeRegroupHint(one, group, tic));
        Assert.True(use.ConsumeRegroupHint(one, group, tic));
        Assert.Equal(2, use.State.HintsConsumedTotal);
        Assert.Equal(3, use.Hints.Count);
        Assert.False(use.ConsumeRegroupHint(null, group, tic));
        Assert.Equal(2, use.State.HintsConsumedTotal);

        var x = new OutDouble1348(-1);
        var y = new OutDouble1348(-1);
        use.Clusters[0].X = 4;
        use.Clusters[0].Y = 5;
        Assert.True(use.ConsumeRegroupTarget(two, null, y, null, tic));
        Assert.Equal(5, y.Value);
        Assert.Equal(-1, x.Value);
        Assert.Equal(7, tic.Value);
        Assert.Equal(3, use.State.HintsConsumedTotal);
        Assert.True(use.ConsumeRegroupTarget(three, x, y, group, tic));
        Assert.Equal(4, x.Value);
        Assert.Equal(5, y.Value);
        Assert.Equal(0, group.Value);
        Assert.Equal(4, use.State.HintsConsumedTotal);

        int consumed = use.State.HintsConsumedTotal;
        use.Clusters.Clear();
        var keptX = new OutDouble1348(11);
        var keptY = new OutDouble1348(12);
        var keptGroup = new OutInt1348(13);
        var keptTic = new OutInt1348(14);
        Assert.False(use.ConsumeRegroupTarget(one, keptX, keptY, keptGroup, keptTic));
        Assert.Equal(11, keptX.Value);
        Assert.Equal(12, keptY.Value);
        Assert.Equal(13, keptGroup.Value);
        Assert.Equal(14, keptTic.Value);
        Assert.Equal(consumed, use.State.HintsConsumedTotal);
        Assert.True(use.ConsumeRegroupHint(one, keptGroup, keptTic));
        Assert.Equal(0, keptGroup.Value);
        Assert.Equal(consumed + 1, use.State.HintsConsumedTotal);

        var negative = new Actor1348();
        use.Hints.Insert(0, new RegroupHint1348 { Actor = negative, GroupId = -1, StoredTic = 4 });
        var negGroup = new OutInt1348(8);
        var negTic = new OutInt1348(9);
        Assert.False(use.ConsumeRegroupTarget(negative, keptX, keptY, negGroup, negTic));
        Assert.Equal(8, negGroup.Value);
        Assert.Equal(9, negTic.Value);
        Assert.Equal(11, keptX.Value);
        Assert.Equal(consumed + 1, use.State.HintsConsumedTotal);
        Assert.True(use.ConsumeRegroupHint(negative, negGroup, negTic));
        Assert.Equal(-1, negGroup.Value);
        Assert.Equal(4, negTic.Value);
        Assert.Equal(consumed + 2, use.State.HintsConsumedTotal);

        use.Hints.Insert(0, new RegroupHint1348 { Actor = null, GroupId = 0, StoredTic = 1 });
        Assert.False(use.ReadRegroupHint(null, negGroup, negTic));
        Assert.Equal(-1, negGroup.Value);

        use.Hints.Insert(0, new RegroupHint1348 { Actor = one, GroupId = 6, StoredTic = 2 });
        Assert.True(use.ReadRegroupHint(one, negGroup, negTic));
        Assert.Equal(6, negGroup.Value);
        Assert.Equal(2, negTic.Value);
    }

    static VmAiDirector1348 Armed() => new() { Enable = true, LocalAuthority = true };

    static Actor1348 Monster(double x, double y, int health = 100, int flags = 0, int flags2 = 0) => new()
    {
        X = x,
        Y = y,
        Health = health,
        Flags = flags,
        Flags2 = flags2,
        Flags3 = VmAiDirector1348.Mf3IsMonster,
    };

    const string DefaultStatus =
        "\n=== HCDE AI Director ===\n" +
        "  sv_aidirector_enable      = off\n" +
        "  sv_aidirector_sweep_tics  = 7\n" +
        "  authority-drives          = no\n" +
        "  authority-active          = no\n" +
        "  ticks-since-start         = 0\n" +
        "  last-sweep-tic            = 0\n" +
        "  sweep-count               = 0\n" +
        "  last-sweep-wallclock-ms   = 0 (diagnostic only)\n" +
        "  max-sweep-wallclock-ms    = 0 (diagnostic only)\n" +
        "  monsters-observed         = 0\n" +
        "  monsters-alive            = 0\n" +
        "  monsters-dormant          = 0\n" +
        "  monsters-corpse           = 0\n" +
        "  groups-observed           = 0\n" +
        "  largest-group-size        = 0\n" +
        "  sv_aidirector_regroup_hint = off\n" +
        "  regroup-hints-stored      = 0\n" +
        "  deterministic-rng         = pr_aidirector named stream; draws=0\n" +
        "  hints-issued-this-tic     = 0\n" +
        "  hints-issued-total        = 0\n" +
        "  hints-consumed-total      = 0 (regroup direction biases applied)\n" +
        "  last-tick-wallclock-ms    = 0 (diagnostic only)\n" +
        "  phase                     = Phase 3: deterministic regroup chase-direction bias; no replication.\n" +
        "  see docs/HCDE_AIDIRECTOR_AUDIT.md for boundaries.\n" +
        "========================\n";

    const string PopulatedStatus =
        "\n=== HCDE AI Director ===\n" +
        "  sv_aidirector_enable      = on\n" +
        "  sv_aidirector_sweep_tics  = 7\n" +
        "  authority-drives          = yes\n" +
        "  authority-active          = yes\n" +
        "  ticks-since-start         = 1\n" +
        "  last-sweep-tic            = 42\n" +
        "  sweep-count               = 1\n" +
        "  last-sweep-wallclock-ms   = 10 (diagnostic only)\n" +
        "  max-sweep-wallclock-ms    = 10 (diagnostic only)\n" +
        "  monsters-observed         = 5\n" +
        "  monsters-alive            = 4\n" +
        "  monsters-dormant          = 0\n" +
        "  monsters-corpse           = 1\n" +
        "  groups-observed           = 2\n" +
        "  largest-group-size        = 3\n" +
        "  sv_aidirector_regroup_hint = on\n" +
        "  regroup-hints-stored      = 3\n" +
        "  deterministic-rng         = pr_aidirector named stream; draws=0\n" +
        "  hints-issued-this-tic     = 3\n" +
        "  hints-issued-total        = 3\n" +
        "  hints-consumed-total      = 0 (regroup direction biases applied)\n" +
        "  last-tick-wallclock-ms    = 1000 (diagnostic only)\n" +
        "  phase                     = Phase 3: deterministic regroup chase-direction bias; no replication.\n" +
        "  see docs/HCDE_AIDIRECTOR_AUDIT.md for boundaries.\n" +
        "========================\n";
}
