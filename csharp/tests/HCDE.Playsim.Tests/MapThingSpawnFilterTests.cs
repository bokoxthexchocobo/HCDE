namespace HCDE.Playsim.Tests;

public class MapThingSpawnFilterTests
{
    [Fact]
    public void RejectsThingWithoutMatchingSinglePlayerMask()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 3004,
            Flags = MapThingSpawnFlags.Cooperative,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.SinglePlayer,
        };

        Assert.False(MapThingSpawnFilter.ShouldSpawnMapThing(request, context, out var reason));
        Assert.Equal(MapThingSpawnFilter.MapThingSpawnRejectReason.GameModeMask, reason);
    }

    [Fact]
    public void AcceptsDeathmatchOnlyThingInDeathmatch()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 2001,
            Flags = MapThingSpawnFlags.Deathmatch,
            SkillFilter = ushort.MaxValue,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.Deathmatch,
            SkillSpawnMask = 0xFFFF,
        };

        Assert.True(MapThingSpawnFilter.ShouldSpawnMapThing(request, context, out _));
    }

    [Fact]
    public void SkillFilterHonorsEntryNoSkillFlagsBypass()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 9,
            Flags = MapThingSpawnFlags.SinglePlayer,
            SkillFilter = 0,
            EntryIgnoresSkillFlags = true,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.SinglePlayer,
            SkillSpawnMask = 0x0001,
        };

        Assert.True(MapThingSpawnFilter.PassesGameModeAndSkillFilter(request, context, out _));
    }

    [Fact]
    public void ClassFilterRejectsWrongSinglePlayerClass()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 3001,
            Flags = MapThingSpawnFlags.SinglePlayer,
            SkillFilter = ushort.MaxValue,
            ClassFilter = 0x0002,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.SinglePlayer,
            ConsolePlayerSpawnClass = 0x0001,
            SkillSpawnMask = ushort.MaxValue,
        };

        Assert.False(MapThingSpawnFilter.PassesGameModeAndSkillFilter(request, context, out var reason));
        Assert.Equal(MapThingSpawnFilter.MapThingSpawnRejectReason.ClassMaskSingle, reason);
    }

    [Fact]
    public void CoopCombinesSpawnClassesForMask()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 3001,
            Flags = MapThingSpawnFlags.Cooperative,
            SkillFilter = ushort.MaxValue,
            ClassFilter = 0x0004,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.CooperativeMultiplayer,
            CooperativeSpawnClasses = new[] { 0x0001, 0x0002 },
            SkillSpawnMask = ushort.MaxValue,
        };

        Assert.False(MapThingSpawnFilter.PassesGameModeAndSkillFilter(request, context, out var reason));
        Assert.Equal(MapThingSpawnFilter.MapThingSpawnRejectReason.ClassMaskCoop, reason);
    }

    [Fact]
    public void CoopDmOnlyThingBlockedWhenFlagSet()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 5,
            Flags = MapThingSpawnFlags.Deathmatch,
            SkillFilter = ushort.MaxValue,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.CooperativeMultiplayer,
            DmNoCoopThingSpawn = true,
            SkillSpawnMask = ushort.MaxValue,
        };

        Assert.False(MapThingSpawnFilter.PassesLateSpawnRules(request, context, out var reason));
        Assert.Equal(MapThingSpawnFilter.MapThingSpawnRejectReason.CoopDmOnlyThing, reason);
    }

    [Fact]
    public void NoMonstersSkillBlocksMonsterSpawn()
    {
        var request = new MapThingSpawnFilter.MapThingSpawnRequest
        {
            EditorNumber = 3001,
            Flags = MapThingSpawnFlags.SinglePlayer | MapThingSpawnFlags.Cooperative,
            ActorIsMonster = true,
        };
        var context = new MapThingSpawnFilter.MapThingSpawnContext
        {
            GameMode = MapThingSpawnGameMode.SinglePlayer,
            DmNoMonsters = true,
        };

        Assert.False(MapThingSpawnFilter.PassesLateSpawnRules(request, context, out var reason));
        Assert.Equal(MapThingSpawnFilter.MapThingSpawnRejectReason.NoMonsters, reason);
    }
}

public class MapThingPlayerStartRegistrarTests
{
    [Fact]
    public void PlayerStartPositionMustMatch()
    {
        var registrar = new MapThingPlayerStartRegistrar(4);
        var start = new MapThingPlayerStartRegistrar.PlayerStart(1, 2, 100, 200, 0, 90);
        Assert.False(registrar.TryRegisterPlayerStart(0, 1, start, randomPlayerStarts: false, out _));
        Assert.True(registrar.TryRegisterPlayerStart(0, 2, start, randomPlayerStarts: false, out var selected));
        Assert.Equal(start, selected);
    }

    [Fact]
    public void LaterSinglePlayerStartOverridesEarlierSlot()
    {
        var registrar = new MapThingPlayerStartRegistrar(4);
        var first = new MapThingPlayerStartRegistrar.PlayerStart(1, 0, 1, 1, 0, 0);
        var second = new MapThingPlayerStartRegistrar.PlayerStart(1, 0, 2, 2, 0, 0);
        Assert.True(registrar.TryRegisterPlayerStart(0, 0, first, false, out _));
        Assert.True(registrar.TryRegisterPlayerStart(0, 0, second, false, out _));
        Assert.Single(registrar.AllPlayerStarts);
        Assert.Equal(second, registrar.AllPlayerStarts[0]);
    }

    [Fact]
    public void RandomStartsAccumulateAllPositions()
    {
        var registrar = new MapThingPlayerStartRegistrar(4);
        var a = new MapThingPlayerStartRegistrar.PlayerStart(1, 0, 0, 0, 0, 0);
        var b = new MapThingPlayerStartRegistrar.PlayerStart(2, 0, 1, 1, 0, 0);
        Assert.True(registrar.TryRegisterPlayerStart(0, 0, a, true, out _));
        Assert.True(registrar.TryRegisterPlayerStart(1, 0, b, true, out _));
        Assert.Equal(2, registrar.AllPlayerStarts.Count);
    }
}
