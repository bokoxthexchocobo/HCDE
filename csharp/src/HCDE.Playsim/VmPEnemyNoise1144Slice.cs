namespace HCDE.Playsim;

/// <summary>
/// Chase direction tables and the noise-alert sector mark from <c>p_enemy.cpp</c>
/// (<b>1–150</b>). Chunk <b>1144</b>. The recursive line walk begins at <b>152</b>.
/// </summary>
public static class VmPEnemyNoise1144Slice
{
    public const int East = 0;
    public const int NorthEast = 1;
    public const int North = 2;
    public const int NorthWest = 3;
    public const int West = 4;
    public const int SouthWest = 5;
    public const int South = 6;
    public const int SouthEast = 7;
    public const int NoDir = 8;
    public const int FloorPlane = 0;
    public const int CeilingPlane = 1;
    public const int NoSplashAlert = 0x00100000;
    public const double SqrtHalf = 0.7071075439453125;

    public readonly record struct RngStream(string Name, bool FileLocal);

    public readonly record struct SectorActor(bool IsTarget, int Flags4, double Distance);

    public readonly record struct NoiseMark(bool Flooded, int ValidCount, int SoundTraversed, IReadOnlyList<int> Heard);

    public static readonly int[] Opposite =
    {
        West, SouthWest, South, SouthEast, East, NorthEast, North, NorthWest, NoDir
    };

    public static readonly int[] Diagonals = { NorthWest, NorthEast, SouthWest, SouthEast };

    public static readonly double[] XSpeed = { 1, SqrtHalf, 0, -SqrtHalf, -1, -SqrtHalf, 0, SqrtHalf };

    public static readonly double[] YSpeed = { 0, SqrtHalf, 1, SqrtHalf, 0, -SqrtHalf, -1, -SqrtHalf };

    public static readonly RngStream[] RngStreams =
    {
        new("CheckMissileRange", true),
        new("OpenDoor", true),
        new("TryWalk", true),
        new("NewChaseDir", true),
        new("LookForMonsters", true),
        new("LookForPlayers", true),
        new("Anubis", true),
        new("Chase", false),
        new("FaceTarget", false),
        new("RailFace", false),
        new("LookyLooky", true),
        new("IGotHooky", true),
        new("SlooK", true),
        new("Dropoff", true),
        new("Defect", true),
        new("AvoidCrush", true),
        new("StayOnLift", true),
        new("SkipTarget", true),
        new("EnemyStrafe", true)
    };

    public static bool NoMonsterInterpolationDefault()
    {
        return false;
    }

    public static int DropStyleDefault()
    {
        return 0;
    }

    public static (double X, double Y) MoveSpeed(int direction)
    {
        return (XSpeed[direction], YSpeed[direction]);
    }

    public static (bool CheckAbove, bool CheckBelow) PortalSoundChecks(bool ceilingBlocksSound, bool floorBlocksSound)
    {
        if (ceilingBlocksSound)
        {
            if (floorBlocksSound)
                return (false, false);
            return (false, true);
        }
        if (floorBlocksSound)
            return (true, false);
        return (true, true);
    }

    public static NoiseMark MarkSector(
        int sectorValidCount,
        int globalValidCount,
        int soundTraversed,
        int soundBlocks,
        bool splash,
        double maxDistance,
        IReadOnlyList<SectorActor> actors)
    {
        if (sectorValidCount == globalValidCount && soundTraversed <= soundBlocks + 1)
            return new NoiseMark(true, sectorValidCount, soundTraversed, System.Array.Empty<int>());

        var heard = new List<int>();
        for (var i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            if (actor.IsTarget)
                continue;
            if (splash && (actor.Flags4 & NoSplashAlert) != 0)
                continue;
            if (maxDistance != 0 && actor.Distance > maxDistance)
                continue;
            heard.Add(i);
        }
        return new NoiseMark(false, globalValidCount, soundBlocks + 1, heard);
    }
}
