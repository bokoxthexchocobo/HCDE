namespace HCDE.Playsim;

/// <summary>Lift/hazard/tag helpers, <c>A_FaceTarget</c>, <c>P_DropItem</c>, and <c>FLevelLocals::Massacre</c> from <c>p_enemy.cpp</c>.</summary>
public static class VmPEnemySmartAssist
{
    public const int SecmfLift = 1;
    public const int Mf9Nosectordamage = 1;
    public const int SecmfHurtinair = 1 << 1;
    public const int SecmfHurtmonsters = 1 << 2;
    public const int Mf2Dormant = 1;
    public const int Mf3Ismonster = 1;
    public const int MfFriendly = 1;

    public sealed class MoveActor
    {
        public object? SectorFloorData { get; set; }
        public bool SectorIsLift { get; set; }
        public int Flags9 { get; set; }
        public bool HasPlayer { get; set; }
        public bool AtSectorFloor { get; set; }
        public bool InWater { get; set; }
        public IEnumerable<TouchingSector>? TouchingSectors { get; set; }
    }

    public sealed class TouchingSector
    {
        public int CrushDirection { get; set; }
        public int DamageAmount { get; set; }
        public int MoreFlags { get; set; }
    }

    public sealed class TagSector
    {
        public Func<bool>? HasTags { get; init; }
        public Func<int>? FirstTag { get; init; }
    }

    public static bool IsOnLift(MoveActor actor, Func<object?, bool>? isPlat) =>
        (actor.SectorFloorData != null && isPlat?.Invoke(actor.SectorFloorData) == true) ||
        actor.SectorIsLift;

    public static int IsUnderDamage(MoveActor actor)
    {
        var dir = 0;
        foreach (var sec in actor.TouchingSectors ?? Array.Empty<TouchingSector>())
        {
            if (sec.CrushDirection > 0)
                dir |= sec.CrushDirection;
            if ((actor.Flags9 & Mf9Nosectordamage) == 0 && sec.DamageAmount > 0 &&
                ((sec.MoreFlags & SecmfHurtinair) != 0 || actor.AtSectorFloor || actor.InWater))
            {
                return actor.HasPlayer ||
                       (!actor.HasPlayer && (sec.MoreFlags & SecmfHurtmonsters) != 0)
                    ? -1
                    : 0;
            }
        }

        return dir;
    }

    public static bool CheckTags(TagSector sec1, TagSector sec2)
    {
        var sec1Tags = sec1.HasTags?.Invoke() == true;
        var sec2Tags = sec2.HasTags?.Invoke() == true;
        if (!sec1Tags || !sec2Tags)
            return ReferenceEquals(sec1, sec2);
        return sec1.FirstTag?.Invoke() == sec2.FirstTag?.Invoke();
    }

    public static void FaceTarget(object self, object? target, Action<object, object?>? face) =>
        face?.Invoke(self, target);

    public static object? DropItem(
        object source,
        object? type,
        int dropamount,
        int chance,
        Func<object, object?, int, int, object?>? vmDropItem) =>
        vmDropItem?.Invoke(source, type, dropamount, chance);

    public sealed class MassacreActor
    {
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags { get; set; }
        public Func<int>? Massacre { get; init; }
    }

    public static int Massacre(
        IEnumerable<MassacreActor> actors,
        bool baddies,
        Func<MassacreActor, bool>? classFilter)
    {
        var killcount = 0;
        foreach (var actor in actors)
        {
            if (classFilter?.Invoke(actor) == false)
                continue;
            if ((actor.Flags2 & Mf2Dormant) != 0)
                continue;
            if ((actor.Flags3 & Mf3Ismonster) == 0)
                continue;
            if (baddies && (actor.Flags & MfFriendly) != 0)
                continue;
            killcount += actor.Massacre?.Invoke() ?? 0;
        }

        return killcount;
    }
}
