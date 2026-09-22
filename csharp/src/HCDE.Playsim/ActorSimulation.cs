using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public readonly struct PlayerCommand
{
    public short ForwardMove { get; init; }
    public short SideMove { get; init; }
    public short YawDelta { get; init; }
}

public class Actor : Thinker
{
    public const int PlayerStartMin = 1;
    public const int PlayerStartMax = 4;

    public uint Id { get; init; }
    public int DoomEdNum { get; init; }
    public Fixed X { get; set; }
    public Fixed Y { get; set; }
    public BamAngle Angle { get; set; }
    public int Health { get; set; } = 100;
    public Fixed Radius { get; set; } = Fixed.FromInt(20);
    public Fixed Height { get; set; } = Fixed.FromInt(56);
    public PlayLevel? Level { get; init; }

    public static bool IsPlayerStart(int type) => type is >= PlayerStartMin and <= PlayerStartMax;
}

public sealed class PlayerPawn : Actor
{
    public byte PlayerNum { get; init; }
    public PlayerCommand Pending { get; set; }

    public override void Tick()
    {
        var command = Pending;
        Pending = default;
        Angle = new BamAngle(unchecked(Angle.Raw + (uint)(command.YawDelta << 16)));
        if (Level != null)
        {
            var (dx, dy) = Movement.Thrust(Angle, command.ForwardMove, command.SideMove);
            var moved = LineSlide.Move(Level, X.ToDouble(), Y.ToDouble(), Radius.ToDouble(), dx, dy);
            X = Fixed.FromDouble(moved.X);
            Y = Fixed.FromDouble(moved.Y);
        }

        base.Tick();
    }
}

public static class Movement
{
    /// <summary>
    /// One tic of ground thrust from <c>PlayerPawn.MovePlayer</c>.
    /// The wire value is the classic move shifted left by 8. It is then scaled by
    /// <c>Speed / 256</c> and <c>ORIG_FRICTION_FACTOR</c> (<c>2048/65536</c>), which is
    /// <c>command / 8192</c> map units. Angle 0 faces east. Side thrust uses angle minus 90 degrees.
    /// </summary>
    public static (double X, double Y) Thrust(BamAngle angle, short forwardMove, short sideMove)
    {
        var radians = angle.Raw * (Math.PI / 0x80000000u);
        var forward = forwardMove / 8192.0;
        var side = sideMove / 8192.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return (forward * cos + side * sin, forward * sin - side * cos);
    }
}

public static class LineSlide
{
    public static (double X, double Y) Move(PlayLevel level, double x, double y, double radius, double dx, double dy)
    {
        if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9)
            return (x, y);

        if (CanOccupy(level, x + dx, y + dy, radius, x, y))
            return (x + dx, y + dy);

        LevelLine? hit = null;
        foreach (var line in level.Lines)
        {
            if (!line.BlocksMovement)
                continue;
            if (Hits(x + dx, y + dy, radius, x, y, line))
            {
                hit = line;
                break;
            }
        }

        if (hit == null)
            return (x, y);

        var lx = hit.X2 - hit.X1;
        var ly = hit.Y2 - hit.Y1;
        var lengthSquared = lx * lx + ly * ly;
        if (lengthSquared < 1e-9)
            return (x, y);

        var scale = (dx * lx + dy * ly) / lengthSquared;
        var slideX = lx * scale;
        var slideY = ly * scale;
        if (CanOccupy(level, x + slideX, y + slideY, radius, x, y))
            return (x + slideX, y + slideY);

        return (x, y);
    }

    private static bool CanOccupy(PlayLevel level, double x, double y, double radius, double fromX, double fromY)
    {
        foreach (var line in level.Lines)
        {
            if (line.BlocksMovement && Hits(x, y, radius, fromX, fromY, line))
                return false;
        }

        return true;
    }

    private static bool Hits(double x, double y, double radius, double fromX, double fromY, LevelLine line)
    {
        if (DistanceToSegment(x, y, line) <= radius)
            return true;
        return SegmentsCross(fromX, fromY, x, y, line.X1, line.Y1, line.X2, line.Y2);
    }

    private static double DistanceToSegment(double x, double y, LevelLine line)
    {
        var dx = line.X2 - line.X1;
        var dy = line.Y2 - line.Y1;
        var lengthSquared = dx * dx + dy * dy;
        if (lengthSquared < 1e-9)
            return Distance(x, y, line.X1, line.Y1);

        var t = Math.Clamp(((x - line.X1) * dx + (y - line.Y1) * dy) / lengthSquared, 0, 1);
        return Distance(x, y, line.X1 + t * dx, line.Y1 + t * dy);
    }

    private static double Distance(double ax, double ay, double bx, double by)
    {
        var dx = ax - bx;
        var dy = ay - by;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static bool SegmentsCross(
        double ax, double ay, double bx, double by,
        double cx, double cy, double dx, double dy)
    {
        var d1 = Cross(cx, cy, dx, dy, ax, ay);
        var d2 = Cross(cx, cy, dx, dy, bx, by);
        var d3 = Cross(ax, ay, bx, by, cx, cy);
        var d4 = Cross(ax, ay, bx, by, dx, dy);
        return d1 * d2 < 0 && d3 * d4 < 0;
    }

    private static double Cross(double ax, double ay, double bx, double by, double px, double py) =>
        (bx - ax) * (py - ay) - (by - ay) * (px - ax);
}

public static class ActorSpawner
{
    public static IReadOnlyList<Actor> Spawn(
        PlayLevel level,
        ThinkerCollection thinkers,
        DehackedPatchResult? dehacked = null)
    {
        var actors = new List<Actor>();
        uint nextId = 1;
        foreach (var thing in level.Things)
        {
            if (thing.Type == 0)
                continue;

            var defaults = dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == thing.Type);
            var playerStart = Actor.IsPlayerStart(thing.Type);
            Actor actor = playerStart
                ? new PlayerPawn
                {
                    PlayerNum = (byte)(thing.Type - 1),
                    Id = nextId,
                    DoomEdNum = thing.Type,
                    X = Fixed.FromInt(thing.X),
                    Y = Fixed.FromInt(thing.Y),
                    Angle = BamAngle.FromDegrees(thing.Angle),
                    Health = defaults?.Health ?? 100,
                    Radius = RadiusOf(defaults, player: true),
                    Height = HeightOf(defaults),
                    Level = level,
                }
                : new Actor
                {
                    Id = nextId,
                    DoomEdNum = thing.Type,
                    X = Fixed.FromInt(thing.X),
                    Y = Fixed.FromInt(thing.Y),
                    Angle = BamAngle.FromDegrees(thing.Angle),
                    Health = defaults?.Health ?? 30,
                    Radius = RadiusOf(defaults, player: false),
                    Height = HeightOf(defaults),
                    Level = level,
                };

            nextId++;
            thinkers.Add(actor, playerStart ? ThinkerStat.Player : ThinkerStat.Default);
            actors.Add(actor);
        }

        return actors;
    }

    private static Fixed RadiusOf(DehackedActor? defaults, bool player)
    {
        if (defaults != null && defaults.Radius > 0)
            return Fixed.FromDouble(defaults.Radius);
        return Fixed.FromInt(player ? 16 : 20);
    }

    private static Fixed HeightOf(DehackedActor? defaults) =>
        defaults != null && defaults.Height > 0 ? Fixed.FromDouble(defaults.Height) : Fixed.FromInt(56);
}

public sealed class AuthoritySimulation
{
    private AuthoritySimulation(
        PlayLevel level,
        ThinkerCollection thinkers,
        IReadOnlyList<Actor> actors,
        int rngSeed)
    {
        Level = level;
        Thinkers = thinkers;
        Actors = actors;
        RngSeed = rngSeed;
        RecomputeChecksum();
    }

    public PlayLevel Level { get; }
    public ThinkerCollection Thinkers { get; }
    public IReadOnlyList<Actor> Actors { get; }
    public IEnumerable<PlayerPawn> Players => Actors.OfType<PlayerPawn>();
    public int RngSeed { get; }
    public uint Checksum { get; private set; }

    public static AuthoritySimulation Start(PlayLevel level, int rngSeed = 0, DehackedPatchResult? dehacked = null)
    {
        var thinkers = new ThinkerCollection();
        var actors = ActorSpawner.Spawn(level, thinkers, dehacked);
        return new AuthoritySimulation(level, thinkers, actors, rngSeed);
    }

    public void QueueCommand(byte playerNum, PlayerCommand command)
    {
        foreach (var player in Players)
        {
            if (player.PlayerNum == playerNum)
            {
                player.Pending = command;
                return;
            }
        }
    }

    public void Tick()
    {
        Thinkers.Run();
        RecomputeChecksum();
    }

    private void RecomputeChecksum()
    {
        var hash = 2166136261u;
        hash = Mix(hash, unchecked((uint)Thinkers.Clock.Tic));
        hash = Mix(hash, unchecked((uint)RngSeed));
        foreach (var actor in Actors.OrderBy(actor => actor.Id))
        {
            hash = Mix(hash, actor.Id);
            hash = Mix(hash, unchecked((uint)actor.X.Raw));
            hash = Mix(hash, unchecked((uint)actor.Y.Raw));
            hash = Mix(hash, unchecked((uint)actor.Health));
        }

        Checksum = hash;
    }

    private static uint Mix(uint hash, uint value) => (hash ^ value) * 16777619u;
}

public static class HeadlessMapBoot
{
    public static bool TryBoot(
        ReadOnlySpan<byte> wad,
        string mapName,
        out AuthoritySimulation? simulation,
        out string? error,
        int rngSeed = 0)
    {
        simulation = null;
        if (!LevelBuilder.TryFromWad(wad, mapName, out var level, out error))
            return false;

        simulation = AuthoritySimulation.Start(level, rngSeed);
        simulation.Tick();
        return true;
    }
}
