using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorProximityCountTests
{
    [Fact]
    public void LegacySaveClearsGeneralTracerAndTracerChangesChecksum()
    {
        var sim = Room(); var caller = sim.Players.Single(); var target = sim.AddBot(20, 0, 3001);
        var legacy = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        sim.RestoreState(state); var originalHash = sim.Checksum;
        caller.TracerId = target.Id;
        var current = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(current, out var traced, out error), error);
        sim.RestoreState(traced); Assert.NotEqual(originalHash, sim.Checksum);
        sim.RestoreState(state); Assert.Null(caller.TracerId);
        Assert.Equal(originalHash, sim.Checksum); Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void GeneralTracerTransferAndDestroyedLookup()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var recipient = sim.AddBot(100, 0);
        var target = sim.AddBot(200, 0); caller.MasterId = recipient.Id; caller.TracerId = target.Id;
        ActorPointerActions.TransferPointer(caller, recipientSelector: AcsActorPointer.Master,
            sourceField: AcsActorPointer.Tracer);
        Assert.Same(target, AcsActorPointer.Resolve(sim, recipient, AcsActorPointer.Tracer));
        target.Destroy();
        Assert.Null(AcsActorPointer.Resolve(sim, recipient, AcsActorPointer.Tracer));
    }

    [Fact]
    public void MalformedTracerArchiveRejectsPresenceMarker()
    {
        var sim = Room(); sim.Players.Single().TracerId = null;
        var bytes = SimSavegame.Write(sim);
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-tracer-value", error);
    }

    [Fact]
    public void GeneralTargetAssignmentTransfersAndSurvivesSaveAndClear()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(20, 0, 3001);
        var legacy = SimSavegame.Write(sim);
        Assert.True(ActorProximity.Check(player, "DoomImp", 100, flags: 64));
        Assert.Same(target, AcsActorPointer.Resolve(sim, player, AcsActorPointer.Target));
        player.MasterId = target.Id;
        ActorPointerActions.TransferPointer(player, recipientSelector: AcsActorPointer.Master,
            recipientField: AcsActorPointer.Tracer);
        Assert.Equal(target.Id, player.TargetId);
        Assert.Null(target.TracerId); // Transfer suppresses self references.
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPointerActions.RearrangePointers(player, AcsActorPointer.Null);
        Assert.Null(player.TargetId);
        sim.RestoreState(state); Assert.Equal(target.Id, player.TargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(SimSavegame.TryRead(legacy, out var old, out error), error);
        sim.RestoreState(old); Assert.Null(player.TargetId);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void GeneralTargetRearrangementUsesOriginalPointers()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(20, 0, 3001);
        Assert.True(ActorProximity.Check(player, "DoomImp", 100, flags: 64));
        ActorPointerActions.RearrangePointers(player, AcsActorPointer.Null,
            AcsActorPointer.Target, AcsActorPointer.Target);
        Assert.Null(player.TargetId); Assert.Equal(target.Id, player.MasterId);
        Assert.Equal(target.Id, player.TracerId);
    }

    [Theory]
    [InlineData(1024, false)]
    [InlineData(512, false)]
    [InlineData(1024, true)]
    [InlineData(512, true)]
    public void ProjectileTracerAssignmentUsesSelectedOwnerAndSurvivesSave(int preference, bool onPointer)
    {
        var sim = Room();
        var caller = sim.Players.Single();
        var projectile = sim.SpawnProjectile(caller, ProjectileKind.Rocket);
        caller.MasterId = projectile.Id;
        var far = sim.AddBot(70, 0, 3001);
        var near = sim.AddBot(10, 0, 3001);
        Actor subject = onPointer ? caller : projectile;
        Assert.Equal(2, ActorProximity.CountProximity(subject, "DoomImp", 100,
            flags: 128 | 256 | preference | (onPointer ? 2048 : 0), pointerSelector: onPointer ? 4 : 0));
        var expected = preference == 1024 ? near : far;
        Assert.Equal(expected.Id, projectile.TracerTargetId);
        Assert.Equal(expected.Id, projectile.MasterId);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        projectile.RestoreTracerTarget(null);
        sim.RestoreState(state);
        Assert.Equal(expected.Id, projectile.TracerTargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void GeneralTracerAssignmentResolvesAndSurvivesSave()
    {
        var sim = Room();
        var caller = sim.Players.Single();
        var target = sim.AddBot(20, 0, 3001);
        Assert.True(ActorProximity.Check(caller, "DoomImp", 100, flags: 128 | 256));
        Assert.Equal(target.Id, caller.MasterId);
        Assert.Same(target, AcsActorPointer.Resolve(sim, caller, AcsActorPointer.Tracer));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        caller.TracerId = null;
        sim.RestoreState(state);
        Assert.Equal(target.Id, caller.TracerId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(1024, false)]
    [InlineData(512, false)]
    [InlineData(1024, true)]
    [InlineData(512, true)]
    public void MasterAssignmentUsesDistancePreferenceAndSelectedOwner(int preference, bool onPointer)
    {
        var sim = Room();
        var caller = sim.Players.Single();
        var reference = sim.AddBot(0, 0, 3004);
        caller.MasterId = reference.Id;
        var far = sim.AddBot(70, 0, 3001);
        var near = sim.AddBot(10, 0, 3001);
        var flags = 128 | preference | (onPointer ? 2048 : 0);
        Assert.True(ActorProximity.Check(caller, "DoomImp", 100, count: 0, flags: flags, pointerSelector: 4));
        var expected = preference == 1024 ? near : far;
        Assert.Equal(expected.Id, onPointer ? reference.MasterId : caller.MasterId);
        if (onPointer) Assert.Equal(reference.Id, caller.MasterId);
        else Assert.Null(reference.MasterId);
    }

    [Fact]
    public void CountingAssignsMasterAndMissingMatchesPreserveExistingPointer()
    {
        var sim = Room();
        var caller = sim.Players.Single();
        var imp = sim.AddBot(20, 0, 3001);
        Assert.Equal(1, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 128));
        Assert.Equal(imp.Id, caller.MasterId);
        Assert.False(ActorProximity.Check(caller, "DoomImp", 1, flags: 128));
        Assert.Equal(imp.Id, caller.MasterId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(32)]
    public void CountingReturnsEveryMatchRegardlessOfCountComparisonFlags(int flags)
    {
        var sim = Room(); sim.AddBot(10, 0, 3001); sim.AddBot(20, 0, 3001); sim.AddBot(30, 0, 3001);
        Assert.Equal(3, ActorProximity.CountProximity(sim.Players.Single(), "DoomImp", 100, flags));
    }

    [Fact]
    public void CountingUsesSameKilledAndReferenceFilters()
    {
        var sim = Room(); var caller = sim.AddBot(200, 0); var reference = sim.AddBot(0, 0, 3001);
        caller.Brain!.SetSpecialTarget(reference); var other = sim.AddBot(20, 0, 3001); other.Killed = true;
        Assert.Equal(0, ActorProximity.CountProximity(caller, "DoomImp", 100, pointerSelector: 2));
        Assert.Equal(1, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 8, pointerSelector: 2));
        Assert.Equal(0, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 8, pointerSelector: 1));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    public void AcsResolvesGlobalLocalAndMissingClassStrings(int source, int expected)
    {
        var sim = Room(); sim.AddBot(20, 0, 3001); var globals = new AcsGlobalStrings();
        var id = source switch { 0 => globals.Add("DoomImp"), 1 => AcsStringIds.FromGlobalIndex(999), _ => 0 };
        var stack = new List<int> { 0, id, 100 << 16 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            ["DoomImp"], globals, AcsCallFunctions.CheckProximity, 3, out var result));
        Assert.Equal(expected, result); Assert.Empty(stack);
    }

    [Fact]
    public void ActorClassCallResultCanBeUsedAsProximityClassArgument()
    {
        var sim = Room(); var imp = sim.AddBot(20, 0, 3001); imp.ThingId = 42;
        var globals = new AcsGlobalStrings(); var binding = new AcsActivatorBinding { Value = sim.Players.Single() };
        var stack = new List<int> { 42 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], globals,
            AcsCallFunctions.GetActorClass, 1, out var classId));
        stack.AddRange([0, classId, 100 << 16]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], globals,
            AcsCallFunctions.CheckProximity, 3, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }],
    });
}
