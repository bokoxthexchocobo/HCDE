using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmSpecActivate1363SliceTests
{
    [Fact]
    public void ExitChecksAndPrefix()
    {
        var slice = new VmSpecActivate1363Slice();
        Assert.Equal(1 << 17, VmSpecActivate1363Slice.Df2KillMonsters);
        Assert.Equal(1 << 10, VmSpecActivate1363Slice.DfNoExit);
        Assert.Equal(1000000, VmSpecActivate1363Slice.TelefragDamage);
        Assert.Equal(1, VmSpecActivate1363Slice.ClusterHub);
        Assert.Equal(0x200, VmSpecActivate1363Slice.MlRepeatSpecial);
        Assert.Equal(0x00800000, VmSpecActivate1363Slice.MlFirstSideOnly);
        Assert.Equal(0x800, VmSpecActivate1363Slice.Mf2OnMobj);
        Assert.Equal(4, VmSpecActivate1363Slice.DmsgSpammy);
        Assert.Equal(0x00400000, VmSpecActivate1363Slice.Level2DummySwitches);
        Assert.Equal(12, VmSpecActivate1363Slice.GaCompleted);
        Assert.Equal(1, VmSpecActivate1363Slice.SpacCross);
        Assert.Equal(2, VmSpecActivate1363Slice.SpacUse);
        Assert.Equal(4, VmSpecActivate1363Slice.SpacMCross);
        Assert.Equal(8, VmSpecActivate1363Slice.SpacImpact);
        Assert.Equal(16, VmSpecActivate1363Slice.SpacPush);
        Assert.Equal(64, VmSpecActivate1363Slice.SpacUseThrough);
        Assert.Equal(1024, VmSpecActivate1363Slice.SpacUseBack);
        Assert.Equal(8192, VmSpecActivate1363Slice.SpacWalking);
        Assert.Equal(200, VmSpecActivate1363Slice.GenericFloor);
        Assert.Equal(205, VmSpecActivate1363Slice.GenericCrusher);
        Assert.Equal("Exit", VmSpecActivate1363Slice.NameExit);

        slice.DmFlags2 = VmSpecActivate1363Slice.Df2KillMonsters;
        slice.DmFlags = VmSpecActivate1363Slice.DfNoExit;
        slice.Deathmatch = true;
        slice.KilledMonsters = 1;
        slice.TotalMonsters = 4;
        Assert.True(slice.CheckIfExitIsGood(null, new VmSpecActivate1363Slice.LevelInfo()));
        Assert.Empty(slice.Damages);
        Assert.Empty(slice.Messages);

        var actor = new VmSpecActivate1363Slice.Actor { Health = 40 };
        Assert.False(slice.CheckIfExitIsGood(actor, null));
        Assert.Empty(slice.Damages);

        slice.KilledMonsters = 4;
        Assert.False(slice.CheckIfExitIsGood(actor, null));
        Assert.Single(slice.Damages);
        Assert.Equal(1000000, slice.Damages[0].Amount);
        Assert.Equal("Exit", slice.Damages[0].Type);
        Assert.Same(actor, slice.Damages[0].Target);
        Assert.Same(actor, slice.Damages[0].Inflictor);
        Assert.Same(actor, slice.Damages[0].Source);
        Assert.Empty(slice.Messages);

        slice.Damages.Clear();
        slice.Deathmatch = false;
        slice.AlwaysApplyDmFlags = true;
        Assert.False(slice.CheckIfExitIsGood(actor, null));
        Assert.Single(slice.Damages);

        slice.Damages.Clear();
        slice.AlwaysApplyDmFlags = false;
        slice.Deathmatch = true;
        slice.DmFlags = 1 << 9;
        actor.Player = new VmSpecActivate1363Slice.Player { Name = "Doom Marine", Index = 3 };
        slice.GameAction = 0;
        Assert.True(slice.CheckIfExitIsGood(actor, null));
        Assert.Empty(slice.Damages);
        Assert.Equal("Doom Marine exited the level\n", Assert.Single(slice.Messages));

        slice.Messages.Clear();
        slice.GameAction = VmSpecActivate1363Slice.GaCompleted;
        Assert.True(slice.CheckIfExitIsGood(actor, null));
        Assert.Empty(slice.Messages);

        slice.GameAction = 11;
        slice.Multiplayer = false;
        slice.Cluster = 4;
        slice.Deathmatch = true;
        var info = new VmSpecActivate1363Slice.LevelInfo { Cluster = 4 };
        slice.FindClusterInfo = cluster =>
        {
            Assert.Equal(4, cluster);
            return new VmSpecActivate1363Slice.ClusterInfo { Flags = VmSpecActivate1363Slice.ClusterHub | 8 };
        };
        actor.Health = 0;
        Assert.False(slice.CheckIfExitIsGood(actor, info));
        Assert.Equal(4, Assert.Single(slice.ClusterQueries));
        Assert.Empty(slice.Messages);

        slice.ClusterQueries.Clear();
        slice.FindClusterInfo = _ => new VmSpecActivate1363Slice.ClusterInfo { Flags = 8 };
        Assert.True(slice.CheckIfExitIsGood(actor, info));
        Assert.Single(slice.ClusterQueries);
        Assert.Equal("Doom Marine exited the level\n", Assert.Single(slice.Messages));

        slice.Messages.Clear();
        slice.ClusterQueries.Clear();
        slice.FindClusterInfo = _ => null;
        Assert.True(slice.CheckIfExitIsGood(actor, info));
        Assert.Single(slice.ClusterQueries);
        Assert.Single(slice.Messages);

        slice.Messages.Clear();
        slice.ClusterQueries.Clear();
        actor.Health = 1;
        Assert.True(slice.CheckIfExitIsGood(actor, info));
        Assert.Empty(slice.ClusterQueries);

        actor.Health = -2;
        slice.Multiplayer = true;
        Assert.True(slice.CheckIfExitIsGood(actor, info));
        Assert.Empty(slice.ClusterQueries);

        slice.Multiplayer = false;
        info.Cluster = 9;
        Assert.True(slice.CheckIfExitIsGood(actor, info));
        Assert.Empty(slice.ClusterQueries);
        info.Cluster = 4;

        slice.FindClusterInfo = null;
        var missing = Assert.Throws<InvalidOperationException>(() => slice.CheckIfExitIsGood(actor, info));
        Assert.Equal("FindClusterInfo", missing.Message);

        slice.FindClusterInfo = _ => null;
        actor.Player = null;
        slice.Deathmatch = true;
        slice.GameAction = 0;
        actor.Health = 5;
        slice.Messages.Clear();
        var name = Assert.Throws<InvalidOperationException>(() => slice.CheckIfExitIsGood(actor, null));
        Assert.Equal("GetName", name.Message);
        Assert.Empty(slice.Messages);

        var line = new VmSpecActivate1363Slice.MapLine
        {
            Special = 5,
            Flags = VmSpecActivate1363Slice.MlFirstSideOnly,
            Activation = VmSpecActivate1363Slice.SpacUseThrough,
        };
        Assert.False(slice.TestActivateLine(line, null, 1, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal(0, slice.UseThroughCalls);
        Assert.Equal(0, slice.TailCalls);
        Assert.Equal(1, slice.LevelReads);

        line.Special = 0;
        line.Flags = 0;
        Assert.False(slice.TestActivateLine(line, null, 0, VmSpecActivate1363Slice.SpacCross, null));
        Assert.Equal(0, slice.TailCalls);
        Assert.Equal(VmSpecActivate1363Slice.SpacUseThrough, line.Activation);

        line.Special = 9;
        line.Flags = VmSpecActivate1363Slice.MlFirstSideOnly;
        line.Activation = VmSpecActivate1363Slice.SpacUse;
        int seenActivation = -1;
        VmSpecActivate1363Slice.OptPos? seenPos = new VmSpecActivate1363Slice.OptPos();
        slice.Tail = (_, _, _, _, pos, activation) =>
        {
            seenActivation = activation;
            seenPos = pos;
            return true;
        };
        Assert.True(slice.TestActivateLine(line, null, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal(1, slice.TailCalls);
        Assert.Equal(0, slice.UseThroughCalls);
        Assert.Equal(9, line.Special);
        Assert.Equal(VmSpecActivate1363Slice.SpacUse, line.Activation);
        Assert.Equal(VmSpecActivate1363Slice.SpacUse, seenActivation);
        Assert.Null(seenPos);

        line.Activation = VmSpecActivate1363Slice.SpacWalking;
        var walker = new VmSpecActivate1363Slice.Actor { Z = 2, FloorZ = 1 };
        Assert.False(slice.TestActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacCross, null));
        Assert.Equal(1, slice.TailCalls);
        walker.Z = 1;
        Assert.True(slice.TestActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacMCross, null));
        Assert.Equal(2, slice.TailCalls);
        walker.Z = 3;
        walker.Flags2 = VmSpecActivate1363Slice.Mf2OnMobj;
        Assert.True(slice.TestActivateLine(line, walker, 0, 5, null));
        Assert.Equal(3, slice.TailCalls);
        walker.Flags2 = 0;
        Assert.True(slice.TestActivateLine(line, null, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal(4, slice.TailCalls);

        var floor = Assert.Throws<InvalidOperationException>(() =>
            slice.TestActivateLine(line, null, 0, VmSpecActivate1363Slice.SpacCross, null));
        Assert.Equal("mo", floor.Message);
        Assert.Equal(4, slice.TailCalls);

        line.Activation = VmSpecActivate1363Slice.SpacUseThrough;
        slice.Tail = (_, _, _, _, _, _) => throw new InvalidOperationException("wrong-tail");
        int throughActivation = -1;
        slice.UseThroughTail = (_, _, _, _, _, activation) =>
        {
            throughActivation = activation;
            return false;
        };
        Assert.False(slice.TestActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal(1, slice.UseThroughCalls);
        Assert.Equal(64, throughActivation);
        Assert.Equal(64, line.Activation);

        slice.UseThroughTail = null;
        var open = Assert.Throws<InvalidOperationException>(() =>
            slice.TestActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal("P_TestActivateLine", open.Message);

        line.Activation = VmSpecActivate1363Slice.SpacUse;
        slice.Tail = null;
        var plain = Assert.Throws<InvalidOperationException>(() =>
            slice.TestActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal("P_TestActivateLine", plain.Message);

        slice.Tail = (_, _, _, _, pos, activation) =>
        {
            seenPos = pos;
            seenActivation = activation;
            return true;
        };
        line.Special = 3;
        line.Level = new VmSpecActivate1363Slice.SpecLevel();
        line.SideDef0 = new object();
        slice.WorldLinePreActivated = (_, _, _, _) => { };
        slice.WorldLineActivated = (_, _, _) => { };
        slice.ExecuteSpecial = (_, _, _) => 1;
        slice.Players.Add(new VmSpecActivate1363Slice.Player { Mo = new VmSpecActivate1363Slice.Actor() });
        var placed = new VmSpecActivate1363Slice.OptPos { X = 1.5, Y = -2, Z = 3.25 };
        Assert.True(slice.ActivateLine(line, walker, 0, VmSpecActivate1363Slice.SpacUse, placed));
        Assert.Same(placed, slice.ObservedOpt);
        Assert.Same(placed, seenPos);
        Assert.Equal(VmSpecActivate1363Slice.SpacUse, seenActivation);

        line.Special = 0;
        Assert.False(slice.RemoteActivate(line, walker, 1, 2, 8, 9, 10));
        Assert.Equal(8, slice.ObservedOpt!.X);
        Assert.Equal(9, slice.ObservedOpt.Y);
        Assert.Equal(10, slice.ObservedOpt.Z);
        Assert.Same(placed, seenPos);

        line.Special = 3;
        seenPos = placed;
        Assert.True(slice.Activate(line, walker, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Null(slice.ObservedOpt);
        Assert.Null(seenPos);
    }

    [Fact]
    public void ActivateLineBody()
    {
        var slice = Ready(out var line, out var actor);
        line.Special = 0;
        Assert.False(slice.ActivateLine(line, actor, 0, VmSpecActivate1363Slice.SpacUse, null));
        Assert.Equal(1, slice.LevelReads);
        Assert.Equal(0, slice.PreCalls);
        Assert.Empty(slice.Diags);
        Assert.Empty(slice.Checksums);

        slice = Ready(out line, out actor);
        slice.Tail = (_, _, _, _, _, _) => false;
        Assert.False(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal(1, slice.LevelReads);
        Assert.Empty(slice.Executes);

        slice = Ready(out line, out actor);
        line.Level = null;
        var level = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("GetLevel", level.Message);
        Assert.Equal(2, slice.LevelReads);

        slice = Ready(out line, out actor);
        slice.WorldLinePreActivated = null;
        var pre = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("WorldLinePreActivated", pre.Message);

        slice = Ready(out line, out actor);
        slice.WorldLinePreActivated = (_, _, _, flag) => flag.Value = false;
        Assert.False(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal(1, slice.PreCalls);
        Assert.Equal(2, slice.LevelReads);
        Assert.Empty(slice.Executes);
        Assert.Empty(slice.Diags);

        slice = Ready(out line, out actor);
        line.LockNumber = 4;
        line.Special = 7;
        slice.CheckKeys = (_, _, _) => false;
        Assert.False(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.False(Assert.Single(slice.KeyChecks).Remote);
        Assert.Empty(slice.Executes);

        slice = Ready(out line, out actor);
        line.LockNumber = 2;
        line.Special = 10;
        slice.CheckKeys = null;
        var keys = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("P_CheckKeys", keys.Message);

        slice = Ready(out line, out actor);
        line.LockNumber = 0;
        slice.CheckKeys = (_, _, _) => throw new InvalidOperationException("keys");
        Assert.True(slice.Activate(line, actor, 2, VmSpecActivate1363Slice.SpacUse));
        Assert.Empty(slice.KeyChecks);
        Assert.False(Assert.Single(slice.Executes).BackSide);

        slice = Ready(out line, out actor);
        line.LockNumber = -1;
        line.Special = 14;
        Assert.True(slice.Activate(line, actor, 1, VmSpecActivate1363Slice.SpacUse));
        Assert.Empty(slice.KeyChecks);
        Assert.True(Assert.Single(slice.Executes).BackSide);

        slice = Ready(out line, out actor);
        line.LockNumber = 1;
        line.Special = 15;
        var sawRemote = false;
        slice.CheckKeys = (_, _, remote) =>
        {
            sawRemote = remote;
            return true;
        };
        slice.ExecuteSpecial = (map, _, _) =>
        {
            map.Special = 7;
            return 1;
        };
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.True(sawRemote);
        Assert.Equal(15, slice.Executes[0].Special);
        Assert.Equal(0, line.Special);
        Assert.Equal(7, slice.Activated[0].Special);

        foreach (var local in new[] { 7, 8, 11, 12, 13, 14 })
        {
            slice = Ready(out line, out actor);
            line.LockNumber = 3;
            line.Special = local;
            slice.CheckKeys = (_, _, remote) =>
            {
                Assert.False(remote);
                return true;
            };
            Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacPush));
        }

        slice = Ready(out line, out actor);
        line.LockNumber = 3;
        line.Special = 0;
        slice.CheckKeys = (_, _, _) => throw new InvalidOperationException("keys");
        Assert.False(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacPush));
        Assert.Empty(slice.KeyChecks);

        foreach (var far in new[] { 6, 9, 10, 15 })
        {
            slice = Ready(out line, out actor);
            line.LockNumber = 3;
            line.Special = far;
            slice.CheckKeys = (_, _, remote) =>
            {
                Assert.True(remote);
                return true;
            };
            Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacPush));
        }

        slice = Ready(out line, out actor);
        slice.ExecuteSpecial = null;
        var execute = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("P_ExecuteSpecial", execute.Message);
        Assert.Equal(3, slice.LevelReads);

        slice = Ready(out line, out actor);
        line.Args[0] = 5;
        line.Args[1] = 6;
        line.Args[2] = 7;
        line.Args[3] = 8;
        line.Args[4] = 9;
        slice.ExecuteSpecial = (_, _, _) => 0;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        var call = Assert.Single(slice.Executes);
        Assert.Equal(42, call.Special);
        Assert.Equal(5, call.Arg0);
        Assert.Equal(6, call.Arg1);
        Assert.Equal(7, call.Arg2);
        Assert.Equal(8, call.Arg3);
        Assert.Equal(9, call.Arg4);
        Assert.Same(actor, call.Mo);
        Assert.Empty(slice.Activated);
        Assert.Equal(42, line.Special);
        Assert.Empty(slice.Switches);
        var diag = Assert.Single(slice.Diags);
        Assert.Equal(-1, diag.Player);
        Assert.Equal(9, diag.Index);
        Assert.Equal(42, diag.Special);
        Assert.Equal(5, diag.Arg0);
        Assert.False(diag.Success);
        Assert.False(diag.FromServer);
        Assert.Equal((9, 42, false), Assert.Single(slice.Checksums));
        Assert.True(slice.LevelReads == 3);

        slice = Ready(out line, out actor);
        slice.WorldLineActivated = null;
        slice.ExecuteSpecial = (_, _, _) => 2;
        var activated = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("WorldLineActivated", activated.Message);
        Assert.Equal(42, line.Special);
        Assert.Empty(slice.Diags);

        slice = Ready(out line, out actor);
        var seenDuring = -1;
        slice.WorldLineActivated = (map, _, _) => seenDuring = map.Special;
        slice.ExecuteSpecial = (map, _, _) =>
        {
            map.Special = 300;
            return -1;
        };
        slice.Developer = 4;
        slice.HasRumble = true;
        var player = new VmSpecActivate1363Slice.Player { Index = 4, Mo = actor, Name = "A" };
        actor.Player = player;
        slice.Players[0] = player;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal(300, seenDuring);
        Assert.Equal(0, line.Special);
        Assert.Equal(44, Assert.Single(slice.Switches).Special);
        Assert.Equal(0, slice.Switches[0].UseAgain);
        Assert.Same(line.SideDef0, slice.Switches[0].Side);
        var rumble = Assert.Single(slice.Rumbles);
        Assert.Same(actor, rumble.Mo);
        Assert.Equal(VmSpecActivate1363Slice.SpacUse, rumble.Activation);
        Assert.Equal(3, rumble.LevelNum);
        Assert.Equal(11, rumble.LineNum);
        Assert.Equal(0, rumble.Special);
        Assert.Equal("Line special 44 activated on line 9\n", Assert.Single(slice.Messages));
        Assert.Equal(4, slice.Diags[0].Player);
        Assert.Equal(44, slice.Diags[0].Special);
        Assert.True(slice.Diags[0].Success);

        slice = Ready(out line, out actor);
        line.Flags = VmSpecActivate1363Slice.MlRepeatSpecial;
        slice.ExecuteSpecial = (map, _, _) =>
        {
            map.Special = 300;
            return 1;
        };
        slice.HasRumble = true;
        var owner = new VmSpecActivate1363Slice.Player { Mo = actor };
        slice.Players[0] = owner;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacPush));
        Assert.Equal(300, line.Special);
        Assert.Equal(0x200, slice.Switches[0].UseAgain);
        Assert.Equal(44, slice.Switches[0].Special);
        Assert.Equal(300, slice.Rumbles[0].Special);
        Assert.Equal(16, slice.Rumbles[0].Activation);

        slice = Ready(out line, out actor);
        slice.HasRumble = true;
        var cameraOwner = new VmSpecActivate1363Slice.Player
        {
            Mo = new VmSpecActivate1363Slice.Actor(),
            Camera = actor,
        };
        slice.Players[0] = cameraOwner;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Single(slice.Switches);
        Assert.Single(slice.Rumbles);
        Assert.Equal(0, line.Special);

        slice = Ready(out line, out actor);
        line.Activation = VmSpecActivate1363Slice.SpacUseThrough;
        slice.Tail = null;
        slice.UseThroughTail = (_, _, _, _, _, activation) =>
        {
            Assert.Equal(64, activation);
            return true;
        };
        slice.HasRumble = true;
        slice.Players[0].Mo = actor;
        line.SideDef0 = null;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUseThrough));
        Assert.Empty(slice.Switches);
        Assert.Single(slice.Rumbles);

        slice = Ready(out line, out actor);
        slice.HasRumble = true;
        slice.Players[0].Mo = actor;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUseBack));
        Assert.Empty(slice.Switches);
        Assert.Equal(VmSpecActivate1363Slice.SpacUseBack, Assert.Single(slice.Rumbles).Activation);

        slice = Ready(out line, out actor);
        slice.HasRumble = false;
        slice.Players[0].Mo = actor;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Single(slice.Switches);
        Assert.Empty(slice.Rumbles);

        slice = Ready(out line, out actor);
        slice.HasRumble = true;
        slice.Players[0].Mo = new VmSpecActivate1363Slice.Actor();
        slice.Players[0].Camera = new VmSpecActivate1363Slice.Actor();
        Assert.True(slice.Activate(line, null, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Empty(slice.Rumbles);
        Assert.Equal(-1, slice.Diags[0].Player);

        slice = Ready(out line, out actor);
        slice.Players[0].Mo = null;
        slice.Players[0].Camera = null;
        var klass = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, null, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("GetClass", klass.Message);
        Assert.Empty(slice.Rumbles);
        Assert.Empty(slice.Diags);
        Assert.Equal(0, line.Special);

        slice = Ready(out line, out actor);
        slice.Players.Clear();
        var players = Assert.Throws<IndexOutOfRangeException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("players", players.Message);
        Assert.Single(slice.Switches);
        Assert.Empty(slice.Diags);

        slice = Ready(out line, out actor);
        line.SideDef0 = null;
        slice.Players[0].Mo = actor;
        var side = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Equal("P_ChangeSwitchTexture", side.Message);
        Assert.Equal(0, line.Special);
        Assert.Empty(slice.Diags);

        slice = Ready(out line, out actor);
        slice.Developer = 3;
        Assert.True(slice.Activate(line, actor, 0, 10));
        Assert.Empty(slice.Messages);
        Assert.Empty(slice.Switches);
        Assert.Empty(slice.Rumbles);

        slice = Ready(out line, out actor);
        slice.Developer = 5;
        slice.ExecuteSpecial = (_, _, _) => 0;
        line.Level!.Flags2 = VmSpecActivate1363Slice.Level2DummySwitches;
        line.Special = 10;
        line.Args[0] = 7;
        slice.LineHasId = (_, id) => id == 7;
        slice.FindFirstSectorFromTag = tag => tag == 7 ? -1 : 0;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Equal(7, Assert.Single(slice.LineIdQueries));
        Assert.Equal(7, Assert.Single(slice.TagQueries));
        Assert.Equal(0, slice.Switches[0].UseAgain);
        Assert.Equal(10, slice.Switches[0].Special);
        Assert.Equal(0, line.Special);
        Assert.Empty(slice.Messages);
        Assert.False(slice.Diags[0].Success);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -2);
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Empty(slice.Switches);
        Assert.Equal(10, line.Special);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, 0);
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Empty(slice.Switches);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        slice.LineHasId = (_, _) => false;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Single(slice.LineIdQueries);
        Assert.Empty(slice.TagQueries);
        Assert.Empty(slice.Switches);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        line.Args[0] = 0;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Equal(0, Assert.Single(slice.LineIdQueries));
        Assert.Empty(slice.TagQueries);
        Assert.Empty(slice.Switches);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 256, -1);
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Empty(slice.LineIdQueries);
        Assert.Equal(256, line.Special);

        foreach (var generic in new[] { 200, 205, 456 })
        {
            slice = Ready(out line, out actor);
            ArmDummy(slice, line, generic, -1);
            Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
            Assert.Empty(slice.LineIdQueries);
            Assert.Equal(generic, line.Special);
        }

        foreach (var openSpecial in new[] { 199, 206, -1 })
        {
            slice = Ready(out line, out actor);
            ArmDummy(slice, line, openSpecial, -1);
            Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
            Assert.Single(slice.LineIdQueries);
        }

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        line.Flags = VmSpecActivate1363Slice.MlRepeatSpecial;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Empty(slice.LineIdQueries);
        Assert.Equal(10, line.Special);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        line.Level!.Flags2 = 0;
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Empty(slice.LineIdQueries);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        Assert.True(slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacUse));
        Assert.Empty(slice.LineIdQueries);
        Assert.Empty(slice.Switches);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        slice.LineHasId = null;
        var missingId = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Equal("LineHasId", missingId.Message);

        slice = Ready(out line, out actor);
        ArmDummy(slice, line, 10, -1);
        slice.FindFirstSectorFromTag = null;
        var missingTag = Assert.Throws<InvalidOperationException>(() =>
            slice.Activate(line, actor, 0, VmSpecActivate1363Slice.SpacImpact));
        Assert.Equal("FindFirstSectorFromTag", missingTag.Message);
        Assert.Single(slice.LineIdQueries);
        Assert.Empty(slice.TagQueries);
    }

    static VmSpecActivate1363Slice Ready(
        out VmSpecActivate1363Slice.MapLine line,
        out VmSpecActivate1363Slice.Actor actor)
    {
        var slice = new VmSpecActivate1363Slice
        {
            Tail = (_, _, _, _, _, _) => true,
            WorldLinePreActivated = (_, _, _, _) => { },
            WorldLineActivated = (_, _, _) => { },
            ExecuteSpecial = (_, _, _) => 1,
            LineHasId = (_, _) => true,
            FindFirstSectorFromTag = _ => -1,
        };
        slice.Players.Add(new VmSpecActivate1363Slice.Player
        {
            Mo = new VmSpecActivate1363Slice.Actor(),
        });
        line = new VmSpecActivate1363Slice.MapLine
        {
            Special = 42,
            Activation = VmSpecActivate1363Slice.SpacUse,
            Index = 9,
            LineNum = 11,
            SideDef0 = new object(),
            Level = new VmSpecActivate1363Slice.SpecLevel { LevelNum = 3 },
        };
        actor = new VmSpecActivate1363Slice.Actor { Health = 100 };
        return slice;
    }

    static void ArmDummy(VmSpecActivate1363Slice slice, VmSpecActivate1363Slice.MapLine line, int special, int tagResult)
    {
        slice.ExecuteSpecial = (_, _, _) => 0;
        line.Level!.Flags2 = VmSpecActivate1363Slice.Level2DummySwitches;
        line.Special = special;
        line.Args[0] = 7;
        slice.LineHasId = (_, _) => true;
        slice.FindFirstSectorFromTag = _ => tagResult;
    }
}
