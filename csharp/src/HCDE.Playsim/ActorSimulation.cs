using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public readonly struct PlayerCommand
{
    public short ForwardMove { get; init; }
    public short SideMove { get; init; }
    public short YawDelta { get; init; }
    public short PitchDelta { get; init; }
    public bool Attack { get; init; }
    public bool Jump { get; init; }
    public bool Use { get; init; }
    public ReadOnlyMemory<byte> WeaponSelections { get; init; }
}

public class Actor : Thinker
{
    public const int PlayerStartMin = 1;
    public const int PlayerStartMax = 4;

    public uint Id { get; init; }
    public int DoomEdNum { get; init; }
    public int ThingId { get; init; }
    public double SpawnZOffset { get; init; }
    public Fixed X { get; set; }
    public Fixed Y { get; set; }
    public Fixed Z { get; set; }
    public Fixed VelocityX { get; set; }
    public Fixed VelocityY { get; set; }
    public Fixed VelocityZ { get; set; }
    public Fixed PreviousZ { get; private set; }
    public int SectorIndex { get; internal set; } = -1;
    public bool OnGround { get; internal set; } = true;
    public bool NoGravity { get; set; }
    public bool Floating { get; set; }
    public double FloatSpeed { get; set; } = 4;
    public bool NoRadiusDamage { get; set; }
    public bool Ambush { get; set; }
    public int PainChance { get; set; } = 256;
    public int ResurrectionHealth { get; internal set; }
    public int RaiseDuration { get; internal set; }
    public int Mass { get; set; } = 100;
    public double ChaseSpeed { get; set; } = 1;
    public bool Solid { get; set; } = true;
    public bool Shootable { get; set; } = true;
    public bool Invulnerable { get; set; }
    public Fixed MaxStepHeight { get; set; } = Fixed.FromInt(24);
    internal AuthoritySimulation? Simulation { get; set; }
    public MonsterBrain? Brain { get; set; }
    public Fixed PreviousX { get; private set; }
    public Fixed PreviousY { get; private set; }
    public BamAngle Angle { get; set; }
    private int _health = 100;
    public int Health
    {
        get => _health;
        set
        {
            var dead = _health <= 0;
            _health = Math.Max(0, value);
            if (!dead && IsDead)
            {
                DeathCount++;
                if (States.HasState(DeathState)) States.Enter(this, DeathState);
                if (this is PlayerPawn player) { player.ClearCommands(); player.AttackPressed = false; }
            }
            else if (dead && !IsDead && States.HasState(SpawnState)) States.Enter(this, SpawnState);
        }
    }
    public ActorStateMachine States { get; } = new();
    public int SpawnState { get; set; } = ActorStateMachine.Spawn;
    public int PainState { get; set; } = ActorStateMachine.Pain;
    public int DeathState { get; set; } = ActorStateMachine.Death;
    public int DeathCount { get; private set; }
    public uint? LastDamageSourceId { get; internal set; }
    public uint? LastHeardTargetId { get; internal set; }
    internal void RestoreHealth(int health) => _health = Math.Max(0, health);
    public Fixed Radius { get; set; } = Fixed.FromInt(20);
    public Fixed Height { get; set; } = Fixed.FromInt(56);
    public PlayLevel? Level { get; init; }

    public bool IsDead => Health <= 0;

    public bool BlocksActors =>
        Solid && !IsDead && !Destroyed && DoomEdNum != LineSpecials.TeleportDestType
        && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public bool CanTakeDamage => Shootable && !IsDead && !Destroyed
        && DoomEdNum != LineSpecials.TeleportDestType && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public static bool IsPlayerStart(int type) => type is >= PlayerStartMin and <= PlayerStartMax;

    public void RememberPosition()
    {
        PreviousX = X;
        PreviousY = Y;
        PreviousZ = Z;
    }

    public override void Tick()
    {
        RememberPosition();
        States.Tick(this);
        if (!Destroyed && Simulation != null)
        {
            Brain?.Tick(Simulation, this);
            TickMovement(Simulation);
        }
        base.Tick();
    }

    protected virtual void TickMovement(AuthoritySimulation sim) => ActorPhysics.Step(sim, this);
}

public sealed class PlayerPawn : Actor
{
    public const int CommandQueueCapacity = 128;
    private readonly Queue<PlayerCommand> _commands = new();
    public byte PlayerNum { get; init; }
    /// <summary>Compatibility slot for direct input/restore; assignment replaces buffered input.</summary>
    public PlayerCommand Pending
    {
        get => _commands.TryPeek(out var command) ? command : default;
        set { _commands.Clear(); _commands.Enqueue(value); }
    }
    public int BufferedCommandCount => _commands.Count;
    public bool TryQueueCommand(PlayerCommand command)
    {
        if (IsDead || Destroyed) return true; // Consume dead-player input without saving it for resurrection.
        if (_commands.Count >= CommandQueueCapacity) return false;
        _commands.Enqueue(command);
        return true;
    }
    public void ClearCommands() => _commands.Clear();
    public PlayerInventory Inventory { get; } = new();
    public bool AttackPressed { get; set; }
    public bool UsePressed { get; set; }
    public bool UseHeld { get; internal set; }
    private double _pitchDegrees;
    public double PitchDegrees
    {
        get => _pitchDegrees;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            _pitchDegrees = Fixed.FromDouble(Math.Clamp(value, -89, 89)).ToDouble();
        }
    }
    public int WeaponCooldown { get; internal set; }

    public override void Tick()
    {
        RememberPosition();
        var command = _commands.TryDequeue(out var queued) ? queued : default;
        UsePressed = !IsDead && command.Use && !UseHeld;
        UseHeld = !IsDead && command.Use;
        if (WeaponCooldown > 0) WeaponCooldown--;
        if (IsDead)
        {
            AttackPressed = false;
            base.Tick();
            return;
        }
        foreach (var slot in command.WeaponSelections.Span)
            Inventory.SelectSlot(slot);
        if (command.Attack)
            AttackPressed = true;
        Angle = new BamAngle(unchecked(Angle.Raw + (uint)(command.YawDelta << 16)));
        PitchDegrees = Math.Clamp(PitchDegrees + command.PitchDelta * (360.0 / 65536), -89, 89);
        if (Level != null)
        {
            var (dx, dy) = Movement.Thrust(Angle, command.ForwardMove, command.SideMove);
            VelocityX = Fixed.FromDouble(VelocityX.ToDouble() + dx);
            VelocityY = Fixed.FromDouble(VelocityY.ToDouble() + dy);
            if (command.Jump && OnGround)
            {
                VelocityZ = Fixed.FromInt(8);
                OnGround = false;
            }
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
        if (level.Blockmap is MapBlockmapRecord blockmap)
        {
            foreach (var index in BlockmapLines.Near(blockmap, x, y, radius).Concat(BlockmapLines.Near(blockmap, fromX, fromY, radius)))
            {
                if ((uint)index >= (uint)level.Lines.Count)
                    continue;
                var line = level.Lines[index];
                if (line.BlocksMovement && Hits(x, y, radius, fromX, fromY, line))
                    return false;
            }

            return true;
        }

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

    public static bool Crosses(double fromX, double fromY, double toX, double toY, LevelLine line) =>
        SegmentsCross(fromX, fromY, toX, toY, line.X1, line.Y1, line.X2, line.Y2);

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
        DehackedPatchResult? dehacked = null, SpawnOptions? spawnOptions = null)
    {
        var actors = new List<Actor>();
        uint nextId = 1;
        foreach (var thing in level.Things)
        {
            if (thing.Type == 0 || !(spawnOptions ?? new SpawnOptions()).Includes(thing))
                continue;

            var defaults = dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == thing.Type && actor.Patched);
            var definitionType = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : thing.Type;
            var definition = DoomActorCatalog.Find(definitionType);
            var playerStart = Actor.IsPlayerStart(thing.Type);
            Actor actor = playerStart
                ? new PlayerPawn
                {
                    PlayerNum = (byte)(thing.Type - 1),
                    Id = nextId,
                    DoomEdNum = thing.Type,
                    X = Fixed.FromDouble(thing.X),
                    Y = Fixed.FromDouble(thing.Y),
                    ThingId = thing.Id, SpawnZOffset = thing.Z,
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
                    X = Fixed.FromDouble(thing.X),
                    Y = Fixed.FromDouble(thing.Y),
                    ThingId = thing.Id, SpawnZOffset = thing.Z,
                    Angle = BamAngle.FromDegrees(thing.Angle),
                    Health = defaults?.Health ?? definition?.Health ?? 30,
                    Radius = defaults == null && definition != null ? Fixed.FromInt(definition.Radius) : RadiusOf(defaults, player: false),
                    Height = defaults == null && definition != null ? Fixed.FromInt(definition.Height) : HeightOf(defaults),
                    PainChance = defaults?.PainChance ?? definition?.PainChance ?? 256,
                    ChaseSpeed = Math.Clamp((defaults?.Speed ?? definition?.Speed ?? 4) / 4.0, 0, ActorPhysics.MaxMove),
                    NoGravity = definition?.Floating ?? false,
                    Floating = definition?.Floating ?? false,
                    NoRadiusDamage = definition?.NoRadiusDamage ?? false,
                    Level = level,
                };

            nextId++;
            actor.ResurrectionHealth = actor.Health;
            actor.Mass = DoomActorCatalog.MassOf(definitionType);
            actor.RaiseDuration = ArchvileActions.RaiseDuration(definitionType);
            actor.Ambush = thing.Ambush;
            actor.Brain = MonsterBrain.ForType(definitionType);
            actor.RememberPosition();
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
    private readonly List<Actor> _actors;
    private uint _nextActorId;
    public uint CombatRandomState { get; private set; }
    private readonly List<SectorMotion> _motions = new();

    private AuthoritySimulation(
        PlayLevel level,
        ThinkerCollection thinkers,
        List<Actor> actors,
        int rngSeed,
        CompatSurface compat)
    {
        Level = level;
        Thinkers = thinkers;
        _actors = actors;
        _nextActorId = actors.Count == 0 ? 1 : checked(actors.Max(actor => actor.Id) + 1);
        RngSeed = rngSeed;
        CombatRandomState = unchecked((uint)rngSeed) ^ 0x9e3779b9u;
        Compat = compat;
        Floors = level.Sectors.Select(sector => Fixed.FromDouble(sector.FloorHeight).ToDouble()).ToArray();
        Ceilings = level.Sectors.Select(sector => Fixed.FromDouble(sector.CeilingHeight).ToDouble()).ToArray();
        foreach (var actor in _actors)
        {
            actor.Simulation = this;
            ActorPhysics.PlaceOnFloor(this, actor);
            actor.Z = Fixed.FromDouble(Math.Clamp(actor.Z.ToDouble() + actor.SpawnZOffset, short.MinValue, short.MaxValue));
            actor.OnGround = actor.SpawnZOffset <= 0;
            ActorPhysics.FitToSector(this, actor, carryFloor: false);
            actor.RememberPosition();
        }
        Acs = new AcsVm();
        Invasion = new InvasionDirector();
        Rewind = new RewindBuffer();
        ReverbActive = compat.HasFlag(CompatSurface.Eternity)
            && level.Sectors.Any(sector => sector.Special == CompatSurfaceRules.EternityReverbSpecial);
        RecomputeChecksum();
        PublishStatus();
    }

    public PlayLevel Level { get; }
    public ThinkerCollection Thinkers { get; }
    public IReadOnlyList<Actor> Actors => _actors;
    public IEnumerable<PlayerPawn> Players => _actors.OfType<PlayerPawn>();
    public int RngSeed { get; }
    public uint Checksum { get; private set; }
    public string StatusLine { get; private set; } = "";
    public CompatSurface Compat { get; }
    public bool Exited { get; private set; }
    public bool SecretExit { get; private set; }
    public bool ReverbActive { get; }
    public bool RewindEnabled { get; set; }
    public AcsVm Acs { get; }
    public InvasionDirector Invasion { get; }
    public RewindBuffer Rewind { get; }
    internal double[] Floors { get; }
    internal double[] Ceilings { get; }
    internal List<SectorMotion> Motions => _motions;

    public double FloorOf(int sector) => sector >= 0 && sector < Floors.Length ? Floors[sector] : (short)0;
    public double CeilingOf(int sector) => sector >= 0 && sector < Ceilings.Length ? Ceilings[sector] : (short)0;

    public static AuthoritySimulation Start(
        PlayLevel level,
        int rngSeed = 0,
        DehackedPatchResult? dehacked = null,
        CompatSurface compat = CompatSurface.None, SpawnOptions? spawnOptions = null)
    {
        var thinkers = new ThinkerCollection();
        var actors = ActorSpawner.Spawn(level, thinkers, dehacked, spawnOptions).ToList();
        return new AuthoritySimulation(level, thinkers, actors, rngSeed, compat);
    }

    public BotPawn AddBot(double x, double y)
    {
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var bot = new BotPawn
        {
            Id = id,
            DoomEdNum = 3004,
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Angle = new BamAngle(0),
            Health = 30,
            ResurrectionHealth = 30,
            RaiseDuration = ArchvileActions.RaiseDuration(3004),
            Level = Level,
            Brain = new MonsterBrain(MonsterAttack.Hitscan),
        };
        bot.RememberPosition();
        bot.Simulation = this;
        ActorPhysics.PlaceOnFloor(this, bot);
        _actors.Add(bot);
        Thinkers.Add(bot, ThinkerStat.Default);
        return bot;
    }

    public uint NextCombatRandom()
    {
        CombatRandomState = unchecked(1664525u * CombatRandomState + 1013904223u);
        return CombatRandomState;
    }

    internal double NextCombatSpread() => ((int)(NextCombatRandom() >> 24) - (int)(NextCombatRandom() >> 24)) / 255.0;

    public ProjectileActor SpawnProjectile(Actor owner, ProjectileKind kind, Actor? target = null)
    {
        var projectile = new ProjectileActor(owner, kind)
        {
            Id = _nextActorId, Level = Level, Simulation = this,
            X = owner.X, Y = owner.Y,
            Z = Fixed.FromDouble(owner.Z.ToDouble() + (owner is PlayerPawn ? owner.Height.ToDouble() / 2
                : kind == ProjectileKind.RevenantTracer ? 48 : 32)),
            Angle = owner.Angle,
        };
        _nextActorId = checked(_nextActorId + 1);
        projectile.Aim(target);
        _actors.Add(projectile);
        Thinkers.Add(projectile);
        return projectile;
    }

    internal Actor? SpawnLostSoul(Actor parent, Actor? target, double angle)
    {
        if (parent.SectorIndex >= 0 && parent.Z.ToDouble() + parent.Height.ToDouble() + 8 > CeilingOf(parent.SectorIndex))
            return null;
        var definition = DoomActorCatalog.Find(3006)!;
        var soul = new Actor
        {
            Id = _nextActorId, DoomEdNum = 3006, Level = Level, Simulation = this,
            X = parent.X, Y = parent.Y, Z = Fixed.FromDouble(parent.Z.ToDouble() + 8),
            Radius = Fixed.FromInt(definition.Radius), Height = Fixed.FromInt(definition.Height),
            Health = definition.Health, PainChance = definition.PainChance, ChaseSpeed = definition.Speed / 4.0,
            NoGravity = true, OnGround = false, SectorIndex = parent.SectorIndex,
            Floating = true,
            Mass = DoomActorCatalog.MassOf(3006),
            Brain = MonsterBrain.ForType(3006), Angle = BamAngle.FromDegrees(angle),
        };
        _nextActorId = checked(_nextActorId + 1);
        var distance = 4 + (parent.Radius.ToDouble() + soul.Radius.ToDouble()) * 1.5;
        var radians = angle * Math.PI / 180;
        var dx = Math.Cos(radians) * distance; var dy = Math.Sin(radians) * distance;
        var steps = Math.Max(1, (int)(1 + Math.Max(Math.Abs(dx), Math.Abs(dy)) / (definition.Radius - 1)));
        var solid = parent.Solid;
        try
        {
            parent.Solid = false;
            for (var i = 0; i < steps; i++)
                if (!ActorPhysics.TryMove(this, soul, soul.X.ToDouble() + dx / steps, soul.Y.ToDouble() + dy / steps, out _))
                    return null;
        }
        finally { parent.Solid = solid; }
        if (target?.CanTakeDamage == true) soul.Brain!.StartCharge(soul, target);
        soul.RememberPosition();
        _actors.Add(soul);
        Thinkers.Add(soul);
        Invasion.RegisterChild(parent, soul);
        return soul;
    }

    public void MarkExited(bool secret)
    {
        Exited = true;
        SecretExit = secret;
    }

    public string Describe() =>
        $"OK map={Level.MapName} tic={Thinkers.Clock.Tic} players={Players.Count()} bots={_actors.OfType<BotPawn>().Count()} checksum={Checksum:x8} invasion={Invasion.Phase} exited={(Exited ? 1 : 0)} secret={(SecretExit ? 1 : 0)}";

    public SimSaveState CaptureState()
    {
        var state = new SimSaveState
        {
            Tic = Thinkers.Clock.Tic,
            Exited = Exited,
            SecretExit = SecretExit,
            CombatRandomState = CombatRandomState,
        };
        foreach (var actor in _actors.OrderBy(actor => actor.Id))
        {
            state.Actors.Add(new SimActorPose
            {
                Id = actor.Id,
                X = actor.X.Raw,
                Y = actor.Y.Raw,
                Angle = actor.Angle.Raw,
                Health = actor.Health,
                Z = actor.Z.Raw,
                VelocityX = actor.VelocityX.Raw,
                VelocityY = actor.VelocityY.Raw,
                VelocityZ = actor.VelocityZ.Raw,
                State = actor.States.Current,
                StateTics = actor.States.RemainingTics,
                OnGround = actor.OnGround,
                WeaponCooldown = actor is PlayerPawn pawn ? pawn.WeaponCooldown : 0,
                Pitch = actor is PlayerPawn aiming ? Fixed.FromDouble(aiming.PitchDegrees).Raw : 0,
                UseHeld = actor is PlayerPawn usingPawn && usingPawn.UseHeld,
            });
        }

        for (var i = 0; i < Floors.Length; i++)
            state.Sectors.Add((Floors[i], Ceilings[i]));
        return state;
    }

    public void RestoreState(SimSaveState state)
    {
        SimSavegame.ValidateSectors(state);
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor != null && (pose.WeaponCooldown < 0 || Math.Abs((long)pose.Pitch) > 89L * 65536
                || pose.HasPhysics && (!actor.States.HasState(pose.State) || pose.StateTics < -1)))
                throw new InvalidOperationException("Saved actor state is not in the current table.");
        }
        Thinkers.Clock.Restore(state.Tic);
        Exited = state.Exited;
        SecretExit = state.SecretExit;
        if (state.CombatRandomState is { } randomState) CombatRandomState = randomState;
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor == null)
                continue;
            actor.X = new Fixed(pose.X);
            actor.Y = new Fixed(pose.Y);
            actor.Angle = new BamAngle(pose.Angle);
            actor.RestoreHealth(pose.Health);
            actor.Z = new Fixed(pose.Z);
            actor.VelocityX = new Fixed(pose.VelocityX);
            actor.VelocityY = new Fixed(pose.VelocityY);
            actor.VelocityZ = new Fixed(pose.VelocityZ);
            actor.OnGround = pose.OnGround;
            actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            if (pose.HasPhysics) actor.States.Restore(pose.State, pose.StateTics);
            else actor.States.Restore(actor.IsDead ? actor.DeathState : actor.SpawnState, -1);
            if (actor is PlayerPawn player)
            {
                player.ClearCommands(); player.AttackPressed = false; player.UsePressed = false;
                player.WeaponCooldown = pose.WeaponCooldown;
                player.PitchDegrees = new Fixed(pose.Pitch).ToDouble(); player.UseHeld = pose.UseHeld;
            }
            actor.RememberPosition();
        }

        var count = Math.Min(Floors.Length, state.Sectors.Count);
        for (var i = 0; i < count; i++)
        {
            Floors[i] = Fixed.FromDouble(state.Sectors[i].Floor).ToDouble();
            Ceilings[i] = Fixed.FromDouble(state.Sectors[i].Ceiling).ToDouble();
        }
        foreach (var pose in state.Actors.Where(pose => !pose.HasPhysics))
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor != null) ActorPhysics.PlaceOnFloor(this, actor);
        }

        RecomputeChecksum();
        PublishStatus();
    }

    public bool QueueCommand(byte playerNum, PlayerCommand command)
    {
        foreach (var player in Players)
        {
            if (player.PlayerNum == playerNum)
            {
                return player.TryQueueCommand(command);
            }
        }
        return false;
    }

    public void Tick()
    {
        Thinkers.Run();
        _actors.RemoveAll(actor => actor.Destroyed);
        CollectPickups();
        ResolveAttacks();
        LineSpecials.ActivateCrossings(this);
        LineSpecials.ActivateUses(this);
        LineSpecials.TickMotions(this);
        foreach (var actor in _actors.Where(actor => actor is not ProjectileActor)) ActorPhysics.FitToSector(this, actor);
        Acs.Tick(this);
        Invasion.Tick(this);
        if (RewindEnabled)
            Rewind.Capture(this);
        RecomputeChecksum();
        PublishStatus();
    }

    private void ResolveAttacks()
    {
        foreach (var player in Players.ToArray())
        {
            if (!player.AttackPressed)
                continue;
            player.AttackPressed = false;
            HitscanCombat.Fire(this, player);
        }
    }

    private void CollectPickups()
    {
        var taken = new List<Actor>();
        foreach (var player in Players)
        {
            if (player.IsDead)
                continue;
            foreach (var actor in _actors)
            {
                if (taken.Contains(actor) || !PickupCatalog.IsPickup(actor.DoomEdNum))
                    continue;
                if (!CirclesOverlap(player, actor))
                    continue;
                if (PickupCatalog.TryGive(player, actor.DoomEdNum))
                    taken.Add(actor);
            }
        }

        foreach (var actor in taken)
        {
            actor.Destroy();
            _actors.Remove(actor);
        }
    }

    private static bool CirclesOverlap(Actor left, Actor right)
    {
        var dx = left.X.ToDouble() - right.X.ToDouble();
        var dy = left.Y.ToDouble() - right.Y.ToDouble();
        var reach = left.Radius.ToDouble() + right.Radius.ToDouble();
        return dx * dx + dy * dy <= reach * reach
            && left.Z.ToDouble() < right.Z.ToDouble() + right.Height.ToDouble()
            && right.Z.ToDouble() < left.Z.ToDouble() + left.Height.ToDouble();
    }

    private void PublishStatus() => StatusLine = Describe();

    private void RecomputeChecksum()
    {
        var hash = 2166136261u;
        hash = Mix(hash, unchecked((uint)Thinkers.Clock.Tic));
        hash = Mix(hash, unchecked((uint)RngSeed));
        hash = Mix(hash, CombatRandomState);
        foreach (var actor in _actors.OrderBy(actor => actor.Id))
        {
            hash = Mix(hash, actor.Id);
            hash = Mix(hash, unchecked((uint)actor.X.Raw));
            hash = Mix(hash, unchecked((uint)actor.Y.Raw));
            hash = Mix(hash, unchecked((uint)actor.Z.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityX.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityY.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityZ.Raw));
            hash = Mix(hash, actor.Angle.Raw);
            hash = Mix(hash, unchecked((uint)actor.States.Current));
            hash = Mix(hash, unchecked((uint)actor.States.RemainingTics));
            hash = Mix(hash, unchecked((uint)actor.Health));
            hash = Mix(hash, unchecked((uint)actor.PainChance));
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.ChaseSpeed).Raw));
            hash = Mix(hash, (uint)actor.Radius.Raw);
            hash = Mix(hash, (uint)actor.Height.Raw);
            hash = Mix(hash, actor.NoGravity ? 1u : 0u);
            hash = Mix(hash, actor.LastHeardTargetId ?? 0);
            hash = Mix(hash, actor.Floating ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.FloatSpeed).Raw));
            hash = Mix(hash, (uint)actor.ResurrectionHealth);
            hash = Mix(hash, (uint)actor.RaiseDuration);
            hash = Mix(hash, (uint)actor.Mass);
            hash = Mix(hash, actor.NoRadiusDamage ? 1u : 0u);
            hash = Mix(hash, actor.Ambush ? 1u : 0u);
            hash = Mix(hash, actor.LastDamageSourceId ?? 0);
            if (actor is PlayerPawn player)
            {
                hash = Mix(hash, (uint)player.WeaponCooldown);
                hash = Mix(hash, player.UseHeld ? 1u : 0u);
                hash = Mix(hash, (uint)Fixed.FromDouble(player.PitchDegrees).Raw);
                hash = Mix(hash, player.Inventory.BlueKey ? 1u : 0u);
                hash = Mix(hash, player.Inventory.YellowKey ? 1u : 0u);
                hash = Mix(hash, player.Inventory.RedKey ? 1u : 0u);
                hash = Mix(hash, (uint)player.Inventory.Bullets);
                hash = Mix(hash, (uint)player.Inventory.Shells);
                hash = Mix(hash, (uint)player.Inventory.Rockets);
                hash = Mix(hash, (uint)player.Inventory.Cells);
                hash = Mix(hash, (uint)player.Inventory.Armor);
                hash = Mix(hash, (uint)player.Inventory.ArmorSavePercent);
                hash = Mix(hash, (uint)player.Inventory.Selected);
                hash = Mix(hash, (uint)player.Inventory.Weapons);
            }
            if (actor.Brain is { } brain)
            {
                hash = Mix(hash, brain.StateChecksum);
            }
            if (actor is ProjectileActor projectile)
            {
                hash = Mix(hash, projectile.Owner.Id);
                hash = Mix(hash, (uint)projectile.Kind);
                hash = Mix(hash, (uint)projectile.RemainingTics);
                hash = Mix(hash, projectile.TracerTargetId ?? 0);
            }
        }

        foreach (var floor in Floors)
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(floor).Raw));
        foreach (var ceiling in Ceilings)
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(ceiling).Raw));
        foreach (var motion in _motions.OrderBy(motion => motion.SectorIndex).ThenBy(motion => motion.Kind))
        {
            foreach (var value in new[] { motion.SectorIndex, (int)motion.Kind, motion.ClosedFloor,
                motion.TargetFloor, motion.ClosedCeiling, motion.TargetCeiling, motion.Speed, motion.Delay,
                motion.Wait, motion.CrushDamage, motion.Closing ? 1 : 0, motion.CloseAfterOpen ? 1 : 0,
                motion.Tag, motion.Paused ? 1 : 0, motion.Loop ? 1 : 0, motion.ReturnSpeed,
                motion.SlowOnCrush ? 1 : 0, motion.StopOnCrush ? 1 : 0, motion.Slowed ? 1 : 0,
                motion.StairGroup, motion.Completed ? 1 : 0 })
            {
                hash = Mix(hash, unchecked((uint)BitConverter.DoubleToInt64Bits(value)));
                hash = Mix(hash, unchecked((uint)(BitConverter.DoubleToInt64Bits(value) >> 32)));
            }
        }
        hash = Mix(hash, Exited ? 1u : 0u);

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
