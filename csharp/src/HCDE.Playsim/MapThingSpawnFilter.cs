namespace HCDE.Playsim;

/// <summary>Native <c>FLevelLocals::SpawnMapThing</c> early rejection filters (game mode, skill, class masks).</summary>
public static class MapThingSpawnFilter
{
    public sealed class MapThingSpawnRequest
    {
        public int EditorNumber { get; init; }
        public MapThingSpawnFlags Flags { get; init; }
        public ushort SkillFilter { get; init; }
        public ushort ClassFilter { get; init; }
        public bool EntryIgnoresSkillFlags { get; init; }
        public int Special { get; init; }
        public uint ThingId { get; init; }
        public bool ActorNotDeathmatch { get; init; }
        public bool ActorIsMonster { get; init; }
        public bool ActorIsInventory { get; init; }
        public bool ActorWeaponSpawn { get; init; }
        public bool LevelNoMonsters { get; init; }
        public bool DmNoMonsters { get; init; }
    }

    public sealed class MapThingSpawnContext
    {
        public MapThingSpawnGameMode GameMode { get; init; } = MapThingSpawnGameMode.SinglePlayer;
        public bool FilterStarts { get; init; }
        public bool SpawnMulti { get; init; }
        public bool SpawnMultiCoopOnly { get; init; }
        public ushort SkillSpawnMask { get; init; } = ushort.MaxValue;
        public int ConsolePlayerSpawnClass { get; init; }
        public IReadOnlyList<int> CooperativeSpawnClasses { get; init; } = Array.Empty<int>();
        public bool DmNoCoopThingSpawn { get; init; }
        public bool DmNoCoopOnlyThings { get; init; }
        public bool DmNoCoopOnlyItems { get; init; }
        public bool DmNoCoopWeaponSpawn { get; init; }
        public bool LevelNoMonsters { get; init; }
        public bool DmNoMonsters { get; init; }
    }

    public enum MapThingSpawnRejectReason
    {
        None,
        InvalidEditorNumber,
        GameModeMask,
        SkillMask,
        ClassMaskSingle,
        ClassMaskCoop,
        DeathmatchNotDmActor,
        CoopDmOnlyThing,
        CoopOnlyThing,
        CoopOnlyItem,
        CoopWeaponSpawn,
        NoMonsters,
    }

    public static bool PassesGameModeAndSkillFilter(MapThingSpawnRequest request, MapThingSpawnContext context, out MapThingSpawnRejectReason reason)
    {
        reason = MapThingSpawnRejectReason.None;
        if (request.EditorNumber is 0 or -1)
        {
            reason = MapThingSpawnRejectReason.InvalidEditorNumber;
            return false;
        }

        if (!context.FilterStarts)
        {
            var mask = MapThingSpawnFlagMask.MaskForGameMode(context.GameMode, context.SpawnMulti, context.SpawnMultiCoopOnly);
            if ((request.Flags & mask) == 0)
            {
                reason = MapThingSpawnRejectReason.GameModeMask;
                return false;
            }

            if ((request.SkillFilter & context.SkillSpawnMask) == 0 && !request.EntryIgnoresSkillFlags)
            {
                reason = MapThingSpawnRejectReason.SkillMask;
                return false;
            }

            if (context.GameMode == MapThingSpawnGameMode.SinglePlayer)
            {
                var spawnClass = context.ConsolePlayerSpawnClass;
                if (spawnClass != 0 && (request.ClassFilter & spawnClass) == 0)
                {
                    reason = MapThingSpawnRejectReason.ClassMaskSingle;
                    return false;
                }
            }
            else if (context.GameMode == MapThingSpawnGameMode.CooperativeMultiplayer)
            {
                var combined = 0;
                var anyUnset = false;
                foreach (var spawnClass in context.CooperativeSpawnClasses)
                {
                    if (spawnClass == 0)
                    {
                        anyUnset = true;
                        break;
                    }

                    combined |= spawnClass;
                }

                if (!anyUnset && combined != 0 && (request.ClassFilter & combined) == 0)
                {
                    reason = MapThingSpawnRejectReason.ClassMaskCoop;
                    return false;
                }
            }
        }

        return true;
    }

    public static bool PassesLateSpawnRules(MapThingSpawnRequest request, MapThingSpawnContext context, out MapThingSpawnRejectReason reason)
    {
        reason = MapThingSpawnRejectReason.None;
        if (context.GameMode == MapThingSpawnGameMode.Deathmatch && request.ActorNotDeathmatch)
        {
            reason = MapThingSpawnRejectReason.DeathmatchNotDmActor;
            return false;
        }

        if ((context.LevelNoMonsters || context.DmNoMonsters) && request.ActorIsMonster)
        {
            reason = MapThingSpawnRejectReason.NoMonsters;
            return false;
        }

        if (context.GameMode != MapThingSpawnGameMode.CooperativeMultiplayer)
            return true;

        var dmSingle = request.Flags & (MapThingSpawnFlags.Deathmatch | MapThingSpawnFlags.SinglePlayer);
        if (context.DmNoCoopThingSpawn && dmSingle == MapThingSpawnFlags.Deathmatch)
        {
            reason = MapThingSpawnRejectReason.CoopDmOnlyThing;
            return false;
        }

        if (request.Special == 0 && request.ThingId == 0 &&
            (request.Flags & (MapThingSpawnFlags.Cooperative | MapThingSpawnFlags.SinglePlayer)) == MapThingSpawnFlags.Cooperative)
        {
            if (context.DmNoCoopOnlyThings)
            {
                reason = MapThingSpawnRejectReason.CoopOnlyThing;
                return false;
            }

            if (context.DmNoCoopOnlyItems && request.ActorIsInventory)
            {
                reason = MapThingSpawnRejectReason.CoopOnlyItem;
                return false;
            }
        }

        if (context.DmNoCoopWeaponSpawn && request.ActorWeaponSpawn &&
            dmSingle == MapThingSpawnFlags.Deathmatch)
        {
            reason = MapThingSpawnRejectReason.CoopWeaponSpawn;
            return false;
        }

        return true;
    }

    public static bool ShouldSpawnMapThing(MapThingSpawnRequest request, MapThingSpawnContext context, out MapThingSpawnRejectReason reason)
    {
        if (!PassesGameModeAndSkillFilter(request, context, out reason))
            return false;

        return PassesLateSpawnRules(request, context, out reason);
    }
}
