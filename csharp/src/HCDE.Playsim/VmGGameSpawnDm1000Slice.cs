namespace HCDE.Playsim;

/// <summary>
/// `G_SpawnPlayer` bot tail, `CheckSpot`, DM start selection, and `PickDeathmatchStart` from <c>g_game.cpp</c> (**2021–2170**).
/// </summary>
public static class VmGGameSpawnDm1000Slice
{
    public const int DeathmatchRandomTries = 20;
    public const int EmptyDeathmatchAngle = int.MaxValue;
    public const uint LevelUsePlayerStartZ = 1u << 0;

    public readonly struct BotPreserveState
    {
        public int Skill { get; init; }
    }

    public readonly struct DeathmatchStartPick
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
        public int Angle { get; init; }
        public bool HasStarts { get; init; }
    }

    public static int? ResolveReadyWeaponAfterDefaultInventory(
        int gameState,
        int gsTitleLevel,
        int pendingWeapon) =>
        gameState != gsTitleLevel ? pendingWeapon : null;

    public static bool TryPreserveBotSkill(object? bot, int skill, out BotPreserveState preserved)
    {
        if (bot == null)
        {
            preserved = default;
            return false;
        }

        preserved = new BotPreserveState { Skill = skill };
        return true;
    }

    public static bool IsInvalidMapthingType(int type) => type == 0;

    public static bool ShouldZeroSpotZ(uint levelFlags) =>
        (levelFlags & LevelUsePlayerStartZ) == 0;

    public static double ComposeSpotZ(double thingZ, double floorZAtSpot, bool useThingZ) =>
        useThingZ ? thingZ + floorZAtSpot : floorZAtSpot;

    public static bool AnotherLivingPlayerAtSpot(
        int playernum,
        double spotX,
        double spotY,
        IReadOnlyList<(int Index, bool HasMo, double X, double Y)> players)
    {
        for (var i = 0; i < playernum; i++)
        {
            var p = players[i];
            if (p.HasMo && p.X == spotX && p.Y == spotY)
                return true;
        }

        return false;
    }

    public static double NearestLivingPlayerDistance(
        double spotX,
        double spotY,
        IReadOnlyList<(bool InGame, bool HasMo, int Health, double X, double Y)> players)
    {
        var closest = double.MaxValue;
        foreach (var p in players)
        {
            if (!p.InGame || !p.HasMo || p.Health <= 0)
                continue;
            var dx = p.X - spotX;
            var dy = p.Y - spotY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < closest)
                closest = distance;
        }

        return closest;
    }

    public static int SelectFarthestDeathmatchIndex<T>(
        IReadOnlyList<T> starts,
        Func<T, double> distanceFromPlayers)
    {
        var bestDistance = 0.0;
        var bestIndex = 0;
        for (var i = 0; i < starts.Count; i++)
        {
            var distance = distanceFromPlayers(starts[i]);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int SelectRandomOpenDeathmatchIndex(
        int selectionCount,
        Func<int> randomRoll,
        Func<int, bool> spotIsOpen,
        out int lastAttempt)
    {
        lastAttempt = 0;
        for (var attempt = 0; attempt < DeathmatchRandomTries; attempt++)
        {
            var index = randomRoll() % selectionCount;
            lastAttempt = index;
            if (spotIsOpen(index))
                return index;
        }

        return lastAttempt;
    }

    public static DeathmatchStartPick ResolvePickDeathmatchStart(
        int selectionCount,
        int randomIndex,
        int startAngle,
        double startX,
        double startY,
        double startZ)
    {
        if (selectionCount == 0)
        {
            return new DeathmatchStartPick
            {
                X = 0,
                Y = 0,
                Z = 0,
                Angle = EmptyDeathmatchAngle,
                HasStarts = false,
            };
        }

        var index = randomIndex % selectionCount;
        return new DeathmatchStartPick
        {
            X = startX,
            Y = startY,
            Z = startZ,
            Angle = startAngle,
            HasStarts = true,
        };
    }

    public static int CountPickDeathmatchReturns(bool provideVector, bool provideAngle) =>
        (provideVector ? 1 : 0) + (provideAngle ? 1 : 0);
}
