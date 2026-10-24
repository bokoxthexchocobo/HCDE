namespace HCDE.Playsim;

public enum MonsterAttack { Melee, Hitscan, Fireball }
public enum MonsterMode { Idle, Chase, Windup, Recovery, Pain, Dead, Heal, Raise }

[Flags]
public enum MonsterChaseFlags
{
    None = 0,
    FastChase = 1,
    NoPlayActive = 2,
    NightmareFast = 4,
    Resurrect = 8,
    DontMove = 16,
    NoRandomTurn = 32,
    NoDirectionTurn = 64,
    NoPostAttackTurn = 128,
    StopIfBlocked = 256,
    DontIdle = 512,
    DontLookAllAround = 1024,
}

/// <summary>Deterministic managed look/chase/attack loop; specialized native actions remain separate.</summary>
public sealed class MonsterBrain(MonsterAttack attack)
{
    private DoomMonsterAttack? _profile;
    private int _nativeType;
    private int _attackTic = -1;
    private bool _separateMelee;
    public bool Charging { get; private set; }
    internal void RestoreCharge(bool charging)
    {
        Charging = charging;
        if (charging) Mode = MonsterMode.Recovery;
    }
    private int _deathTics;
    private uint? _deathTargetId;
    private int _healTics;
    private int _raiseTics;
    public int RaiseTics => _raiseTics;
    private bool _vileFire;
    private bool _inChase;
    internal bool IsInChase => _inChase;
    internal SimRepeatAttack? CaptureLostSoulAttack() => _nativeType == 3006 && _attackTic >= 0
        ? new(_nativeType, _attackTic, TargetId, Mode, WindupTics, AttackCooldown) : null;
    internal void RestoreLostSoulAttack(SimRepeatAttack? saved)
    {
        if (_nativeType != 3006) return;
        if (saved is { } s)
        {
            if (s.NativeType != 3006) throw new InvalidOperationException("Lost soul attack profile mismatch.");
            _attackTic = s.AttackTic; TargetId = s.TargetId; Mode = s.Mode;
            WindupTics = s.WindupTics; AttackCooldown = s.Cooldown; _separateMelee = false;
        }
        else if (_attackTic >= 0)
        { _attackTic = -1; WindupTics = 0; AttackCooldown = 0; Mode = MonsterMode.Idle; }
    }
    internal SimRepeatAttack? CaptureArchvileAttack() => _nativeType == 64 && _attackTic >= 0
        ? new(_nativeType, _attackTic, TargetId, Mode, WindupTics, AttackCooldown, FireExists: _vileFire) : null;
    internal void RestoreArchvileAttack(SimRepeatAttack? saved)
    {
        if (_nativeType != 64) return;
        if (saved is { } s)
        {
            if (s.NativeType != 64) throw new InvalidOperationException("Archvile attack profile mismatch.");
            _attackTic = s.AttackTic; TargetId = s.TargetId; Mode = s.Mode;
            WindupTics = s.WindupTics; AttackCooldown = s.Cooldown; _vileFire = s.FireExists; _separateMelee = false;
        }
        else if (_attackTic >= 0)
        { _attackTic = -1; TargetId = null; Mode = MonsterMode.Idle; WindupTics = 0; AttackCooldown = 0; _vileFire = false; }
    }
    internal SimRepeatAttack? CaptureRevenantAttack() => _nativeType == 66 && _attackTic >= 0
        ? new(_nativeType, _attackTic, TargetId, Mode, WindupTics, AttackCooldown, _separateMelee) : null;
    internal void RestoreRevenantAttack(SimRepeatAttack? saved)
    {
        if (_nativeType != 66) return;
        if (saved is { } s)
        {
            if (s.NativeType != 66) throw new InvalidOperationException("Revenant attack profile mismatch.");
            _attackTic = s.AttackTic; TargetId = s.TargetId; Mode = s.Mode;
            WindupTics = s.WindupTics; AttackCooldown = s.Cooldown; _separateMelee = s.SeparateMelee;
        }
        else if (_attackTic >= 0)
        { _attackTic = -1; TargetId = null; Mode = MonsterMode.Idle; WindupTics = 0; AttackCooldown = 0; _separateMelee = false; }
    }
    internal SimRepeatAttack? CaptureRepeatAttack() => _nativeType is 65 or 7 or 84 && _attackTic >= 0
        ? new(_nativeType, _attackTic, TargetId, Mode, WindupTics, AttackCooldown) : null;
    internal SimRepeatAttack? CaptureProfileAttack() => _profile != null
        && _nativeType is not (65 or 7 or 84 or 64 or 66 or 3006) && _attackTic >= 0
        ? new(_nativeType, _attackTic, TargetId, Mode, WindupTics, AttackCooldown) : null;
    internal void RestoreProfileAttack(SimRepeatAttack? saved)
    {
        if (_profile == null || _nativeType is 65 or 7 or 84 or 64 or 66 or 3006) return;
        if (saved is { } s)
        {
            if (s.NativeType != _nativeType) throw new InvalidOperationException("Attack profile mismatch.");
            _attackTic = s.AttackTic; TargetId = s.TargetId; Mode = s.Mode;
            WindupTics = s.WindupTics; AttackCooldown = s.Cooldown; _separateMelee = false;
        }
        else if (_attackTic >= 0) { _attackTic = -1; TargetId = null; Mode = MonsterMode.Idle; WindupTics = 0; AttackCooldown = 0; }
    }
    internal void RestoreRepeatAttack(SimRepeatAttack? saved)
    {
        if (_nativeType is not (65 or 7 or 84)) return;
        if (saved is { } s)
        {
            if (s.NativeType != _nativeType) throw new InvalidOperationException("Repeat attack profile mismatch.");
            _attackTic = s.AttackTic; TargetId = s.TargetId; Mode = s.Mode;
            WindupTics = s.WindupTics; AttackCooldown = s.Cooldown; _separateMelee = false;
        }
        else { _attackTic = -1; TargetId = null; Mode = MonsterMode.Idle; WindupTics = 0; AttackCooldown = 0; }
    }
    internal SimPainDeath? CapturePainDeath() => _nativeType == 71 ? new(_deathTics, _deathTargetId) : null;
    internal void RestorePainDeath(SimPainDeath state) { _deathTics = state.Tics; _deathTargetId = state.TargetId; }
    internal bool HasPendingDeathAction => _nativeType == 71 && _deathTics < 32;
    public bool Enabled { get; set; } = true;
    public MonsterAttack Attack { get; } = attack;
    internal MonsterAttack? BasicAttack => _nativeType == 0 && _profile is null ? Attack : null;
    internal int? ProfileType => _profile is not null ? _nativeType : null;
    internal SimBasicAttackState? CaptureBasicAttackState() => BasicAttack.HasValue
        && (Mode != MonsterMode.Idle || WindupTics != 0 || AttackCooldown != 0 || _blockedTics != 0
            || _previousX.Raw != 0 || _previousY.Raw != 0 || _lastX != 0 || _lastY != 0 || _raiseTics != 0)
            ? new(Mode, WindupTics, AttackCooldown, _blockedTics, _previousX.Raw, _previousY.Raw,
                Fixed.FromDouble(_lastX).Raw, Fixed.FromDouble(_lastY).Raw, _raiseTics) : null;
    internal void RestoreBasicAttackState(SimBasicAttackState? saved)
    {
        if (!BasicAttack.HasValue) return;
        Mode = saved?.Mode ?? MonsterMode.Idle;
        WindupTics = saved?.WindupTics ?? 0;
        AttackCooldown = saved?.Cooldown ?? 0;
        _raiseTics = saved?.RaiseTics ?? 0;
        RestoreChaseHistory(saved?.BlockedTics ?? 0, saved?.PreviousX ?? 0, saved?.PreviousY ?? 0,
            saved?.LastSeenX ?? 0, saved?.LastSeenY ?? 0);
    }
    public MonsterMode Mode { get; private set; }
    private uint? _targetId;
    public uint? TargetId
    {
        get => _owner is { } owner ? owner.ReadActorPointer(ref _targetId) : _targetId;
        private set => _targetId = value;
    }
    /// <summary>Native <c>lastenemy</c>. Updated when wake-up switches chase targets.</summary>
    private uint? _lastEnemyId;
    public uint? LastEnemyId
    {
        get => _owner is { } owner ? owner.ReadActorPointer(ref _lastEnemyId) : _lastEnemyId;
        private set => _lastEnemyId = value;
    }
    /// <summary>Native chase threshold. While nonzero, <see cref="OkayToSwitchTarget"/> blocks a new target.</summary>
    public int Threshold { get; private set; }
    internal bool HasChaseThresholdOverride { get; private set; }
    internal void RestoreChaseThreshold(SimChaseThreshold? saved)
    {
        Threshold = saved?.Current ?? 0;
        DefThreshold = saved?.Default ?? 100;
        HasChaseThresholdOverride = saved.HasValue;
    }
    internal void SetChaseThreshold(int threshold, bool setDefault)
    {
        HasChaseThresholdOverride = true;
        if (setDefault) DefThreshold = Math.Max(0, threshold);
        else Threshold = Math.Max(0, threshold);
    }
    /// <summary>Native <c>DefThreshold</c>. Wake-up reloads <see cref="Threshold"/> from this value.</summary>
    public int DefThreshold { get; set; } = 100;
    private Actor? _owner;
    private int _unboundReactionTics = 10;
    public int ReactionTics
    {
        get => _owner?.ReactionTime ?? _unboundReactionTics;
        private set { if (_owner is { } actor) actor.ReactionTime = value; else _unboundReactionTics = value; }
    }
    internal void SetReactionTime(int value) => ReactionTics = value;
    internal void ValidateOwner(Actor actor)
    {
        if (_owner != null && !ReferenceEquals(_owner, actor))
            throw new InvalidOperationException("A monster brain cannot be attached to two actors.");
    }
    internal void AttachOwner(Actor actor) { ValidateOwner(actor); _owner = actor; }
    internal void DetachOwner(Actor actor)
    {
        if (!ReferenceEquals(_owner, actor)) return;
        _unboundReactionTics = actor.ReactionTime;
        _owner = null;
    }
    public int AttackCooldown { get; private set; }
    public int WindupTics { get; private set; }
    private double _lastX, _lastY;
    private int _blockedTics;
    private Fixed _previousX, _previousY;

    internal SimProfileChaseHistory? CaptureProfileChaseHistory()
    {
        if (_profile is null) return null;
        var history = new SimProfileChaseHistory(_blockedTics, _previousX.Raw, _previousY.Raw,
            Fixed.FromDouble(_lastX).Raw, Fixed.FromDouble(_lastY).Raw);
        return history != default || Mode != MonsterMode.Idle || WindupTics != 0 || AttackCooldown != 0
            || _healTics != 0 || _raiseTics != 0
            ? history with { Runtime = new(Mode, WindupTics, AttackCooldown, _healTics, _raiseTics) } : null;
    }

    internal void RestoreProfileChaseHistory(SimProfileChaseHistory? saved)
    {
        if (_profile is null) return;
        _healTics = saved?.Runtime?.HealTics ?? 0;
        _raiseTics = saved?.Runtime?.RaiseTics ?? 0;
        RestoreChaseHistory(saved?.BlockedTics ?? 0, saved?.PreviousX ?? 0, saved?.PreviousY ?? 0,
            saved?.LastSeenX ?? 0, saved?.LastSeenY ?? 0);
        if (saved?.Runtime is { } runtime)
        {
            Mode = runtime.Mode; WindupTics = runtime.WindupTics; AttackCooldown = runtime.Cooldown;
        }
        else if (_attackTic < 0 && !Charging)
        {
            Mode = MonsterMode.Idle; WindupTics = 0; AttackCooldown = 0;
        }
    }

    private void RestoreChaseHistory(int blockedTics, int previousX, int previousY, int lastSeenX, int lastSeenY)
    {
        _blockedTics = blockedTics;
        _previousX = new Fixed(previousX); _previousY = new Fixed(previousY);
        _lastX = new Fixed(lastSeenX).ToDouble(); _lastY = new Fixed(lastSeenY).ToDouble();
    }

    internal uint StateChecksum
    {
        get
        {
            var hash = 2166136261u;
            foreach (var value in new[] { (uint)Attack, (uint)Mode, TargetId ?? 0, LastEnemyId ?? 0, (uint)Threshold, (uint)DefThreshold, (uint)ReactionTics,
                (uint)AttackCooldown, (uint)WindupTics, (uint)_blockedTics, (uint)_previousX.Raw,
                (uint)_previousY.Raw, (uint)Fixed.FromDouble(_lastX).Raw, (uint)Fixed.FromDouble(_lastY).Raw, Enabled ? 1u : 0u,
                (uint)_nativeType, unchecked((uint)_attackTic), _separateMelee ? 1u : 0u, Charging ? 1u : 0u,
                (uint)_deathTics, _deathTargetId ?? 0, (uint)_healTics, (uint)_raiseTics, _vileFire ? 1u : 0u })
                hash = unchecked((hash ^ value) * 16777619u);
            return hash;
        }
    }

    public static MonsterBrain? ForType(int type)
    {
        if (DoomMonsterAttacks.Find(type) is { } profile)
            return new(profile.Kind) { _profile = profile, _nativeType = type, _unboundReactionTics = 8 };
        return null;
    }

    /// <summary>Native <c>ReactToDamage</c> wake-up for a non-player. Pain runs before this in <see cref="ActorDamage.Apply"/>.</summary>
    internal void WakeOnDamage(Actor actor, Actor? source, int dealt, bool forcedPain)
    {
        if (actor is PlayerPawn) return;
        if (dealt <= 0 && !forcedPain) return;
        ReactionTics = 0;
        if (source == null || source.Destroyed || source.Id == actor.Id) return;
        if (TargetId == source.Id)
            Threshold = DefThreshold;
        else if (TargetId == null)
        {
            if (actor.OkayToSwitchTarget(source, this))
            {
                if (actor.Simulation != null)
                    RememberLastEnemy(actor.Simulation, actor, null);
                TargetId = source.Id;
                Threshold = DefThreshold;
            }
        }
        else if (actor.OkayToSwitchTarget(source, this))
        {
            if (actor.Simulation != null)
                RememberLastEnemy(actor.Simulation, actor, TargetId);
            TargetId = source.Id;
            Threshold = DefThreshold;
        }
        if (TargetId == source.Id
            && actor.States.Current == actor.SpawnState && actor.States.HasState(actor.SeeState))
            actor.States.Enter(actor, actor.SeeState);
    }

    /// <summary>Native damage-retaliation memory: preserve living players, or any living enemy when TIDtoHate is nonzero.</summary>
    private void RememberLastEnemy(AuthoritySimulation sim, Actor owner, uint? previousTargetId)
    {
        var last = LastEnemyId == null ? null : sim.Actors.FirstOrDefault(actor => actor.Id == LastEnemyId);
        if (last == null || (last is not PlayerPawn && owner.TidToHate == 0) || last.IsDead || last.Destroyed)
            LastEnemyId = previousTargetId;
    }

    /// <summary>Native <c>P_LookForPlayers</c> / look fallbacks when no target is acquired.</summary>
    private Actor? TryResumeLastEnemy(AuthoritySimulation sim, Actor actor)
    {
        if (LastEnemyId is not { } lastId) return null;
        var last = sim.Actors.FirstOrDefault(candidate => candidate.Id == lastId);
        if (last == null || last.Destroyed)
        {
            LastEnemyId = null;
            return null;
        }
        if (last.IsDead) return null;
        if (actor.IsFriend(last))
        {
            LastEnemyId = null;
            return null;
        }
        LastEnemyId = null;
        TargetId = last.Id;
        _lastX = last.X.ToDouble();
        _lastY = last.Y.ToDouble();
        return last;
    }

    internal void Hear(AuthoritySimulation sim, Actor actor, Actor target)
    {
        if (!Enabled || actor.InConversation || actor.TidToHate != 0 || actor.IsDead || actor.Destroyed || TargetId != null) return;
        var idleFrame = actor.States.Current == actor.SpawnState || actor.States.Current == actor.IdleState;
        if (idleFrame) Threshold = 0;
        if (!target.IsDead && !target.Destroyed && target is PlayerPawn playerSource && (playerSource.NoTargetCheat || !playerSource.Friendly)) return;
        if (!target.Shootable || target.IsDead || target.Destroyed)
        {
            if (!idleFrame) return;
            var replacement = FindHearingFallback(sim, actor);
            if (replacement is null) return;
            target = replacement;
        }
        var friendlyNoise = actor.IsFriend(target);
        var blockedAmbush = actor.Ambush && !friendlyNoise
            && !(sim.NativeMonsterMovement
                ? CombatTrace.CheckSight(sim, actor, target, ActorSightFlags.SeePastBlockEverything)
                : CombatTrace.HasLineOfSight(sim, actor, target));
        if (friendlyNoise || blockedAmbush)
        {
            if (!idleFrame) return;
            var replacement = FindHearingFallback(sim, actor);
            if (replacement is null)
            {
                if (friendlyNoise && actor.States.HasState(actor.SeeState)) actor.States.Enter(actor, actor.SeeState);
                return;
            }
            target = replacement;
        }
        if (idleFrame && target.Id == actor.GoalId && ReactionTics > sim.Thinkers.Clock.Tic)
        {
            Mode = MonsterMode.Idle; WindupTics = 0; _attackTic = -1;
            return;
        }
        TargetId = target.Id;
        _lastX = target.X.ToDouble();
        _lastY = target.Y.ToDouble();
        if (idleFrame && target.Id != actor.GoalId) ActorSoundActions.SeeSound(actor);
        if ((actor.States.Current == actor.SpawnState || actor.States.Current == actor.IdleState) && actor.States.HasState(actor.SeeState))
            actor.States.Enter(actor, actor.SeeState);
    }

    private Actor? FindHearingFallback(AuthoritySimulation sim, Actor actor)
        => FindPlayerTarget(sim, actor, MonsterChaseFlags.None)
            ?? sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed)
            ?? TryResumeLastEnemy(sim, actor);

    internal void ClearTarget() => TargetId = null;

    internal void ClearActionTargets() { TargetId = null; LastEnemyId = null; }
    internal void RememberHateTarget(uint? previousTarget)
    {
        if (previousTarget.HasValue) LastEnemyId = previousTarget;
    }
    internal void FilterHateMemory(AuthoritySimulation sim, int tid)
    {
        if (LastEnemyId is { } id && !sim.Actors.Any(a => a.Id == id && !a.Destroyed && a.ThingId == tid)) LastEnemyId = null;
    }
    internal void ForgetDestroyedActor(uint id)
    {
        if (TargetId == id) TargetId = null;
        if (LastEnemyId == id) LastEnemyId = null;
    }
    internal void RestoreTargetMemory(SimTargetMemory memory)
    { TargetId = memory.Target; LastEnemyId = memory.LastEnemy; }

    internal void SetSpecialTarget(Actor? target) => TargetId = target?.Id;

    internal void ClearResurrectionEnemy(uint corpseId)
    {
        if (LastEnemyId == TargetId || LastEnemyId == corpseId) LastEnemyId = null;
        if (TargetId == corpseId) TargetId = null;
    }

    internal void SetTargetThingId(AuthoritySimulation sim, int thingId)
    {
        if (thingId == 0)
        {
            TargetId = null;
            return;
        }

        var target = sim.Actors.LastOrDefault(actor => !actor.Destroyed && actor.ThingId == thingId);
        TargetId = target?.Id;
    }

    /// <summary>Runs chase decisions. Native movement opts into direction/count-based collision walking;
    /// callers must select the same mode when continuing a saved simulation.</summary>
    public void Tick(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = MonsterChaseFlags.None,
        bool useMelee = true, bool useMissile = true, bool nightmareFast = false, bool nativeMovement = false)
    {
        if ((flags & ~(MonsterChaseFlags.FastChase | MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.NightmareFast | MonsterChaseFlags.Resurrect | MonsterChaseFlags.DontMove | MonsterChaseFlags.NoRandomTurn | MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPostAttackTurn | MonsterChaseFlags.StopIfBlocked | MonsterChaseFlags.DontIdle | MonsterChaseFlags.DontLookAllAround)) != 0)
            throw new NotSupportedException("The requested native chase flags are not yet supported.");
        if (!nativeMovement && (flags & MonsterChaseFlags.NoRandomTurn) != 0)
            throw new NotSupportedException("NoRandomTurn requires native chase movement.");
        if (!nativeMovement && (flags & MonsterChaseFlags.DontIdle) != 0)
            throw new NotSupportedException("DontIdle requires native chase movement.");
        if (!nativeMovement && (flags & MonsterChaseFlags.Resurrect) != 0)
            throw new NotSupportedException("Resurrect requires native chase movement.");
        if (!nativeMovement && (flags & MonsterChaseFlags.FastChase) != 0)
            throw new NotSupportedException("FastChase requires native chase movement.");
        if (_inChase) return;
        _inChase = true;
        try
        {
            if (Enabled && (flags & MonsterChaseFlags.Resurrect) != 0
                && ArchvileActions.TryRaise(sim, actor, actor, nativeMovement: true)) return;
            if (actor.InConversation) return;
            TickCore(sim, actor, flags, useMelee, useMissile,
                nightmareFast || (flags & MonsterChaseFlags.NightmareFast) != 0, nativeMovement);
        }
        finally { _inChase = false; }
    }

    private void TickCore(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags, bool useMelee, bool useMissile, bool nightmareFast,
        bool nativeMovement)
    {
        if (!Enabled) return;
        if (actor.Stealth && !actor.IsDead && !actor.Destroyed) actor.VisibilityDirection = -1;
        if (nightmareFast && sim.FastMonsters && actor.States.RemainingTics > 3)
            actor.States.SetTics(Math.Max(3, actor.States.RemainingTics - actor.States.RemainingTics / 2));
        if (Charging && (actor.IsDead || actor.Destroyed || actor.States.Current == actor.PainState))
            StopCharge(actor);
        if (Charging) return;
        if (actor.IsDead || actor.Destroyed)
        {
            if (_nativeType == 71 && actor.IsDead && !actor.Destroyed && _deathTics < 32)
            {
                if (_deathTics == 0) _deathTargetId = TargetId;
                if (++_deathTics == 32)
                {
                    var deathTarget = sim.Actors.FirstOrDefault(candidate => candidate.Id == _deathTargetId && !candidate.Destroyed);
                    if (deathTarget != null && actor.IsFriend(deathTarget)) actor.Friendly = false;
                    foreach (var offset in new[] { 90, 180, 270 })
                        sim.SpawnLostSoul(actor, deathTarget, actor.Angle.ToDegrees() + offset);
                }
            }
            Mode = MonsterMode.Dead; TargetId = null; WindupTics = 0; _attackTic = -1; _raiseTics = 0; return;
        }
        if (!nativeMovement) actor.VelocityX = actor.VelocityY = default;
        _deathTics = 0; _deathTargetId = null;
        if (AttackCooldown > 0) AttackCooldown--;
        if (actor.States.Current == actor.PainState)
        {
            _raiseTics = 0;
            _healTics = 0;
            Mode = MonsterMode.Pain; WindupTics = 0; _attackTic = -1; return;
        }
        if (actor.RaiseState >= 0 && actor.States.Current == actor.RaiseState) return;
        if (AdvanceRaiseFrame()) return;
        var attackTimelineActive = _attackTic >= 0 || WindupTics > 0 || _healTics > 0;
        if (Threshold != 0 && !attackTimelineActive)
        {
            var thresholdTarget = sim.Actors.FirstOrDefault(candidate => candidate.Id == TargetId && !candidate.Destroyed);
            Threshold = thresholdTarget == null || thresholdTarget.IsDead ? 0 : unchecked(Threshold - 1);
        }
        var lookingForTarget = Mode == MonsterMode.Idle && TargetId is null;
        if (lookingForTarget && actor.Special == 229 && actor.SpecialArgs[0] == 0)
        {
            actor.Special = 0;
            actor.GoalId = AcsActorTid.AllFromTid(sim, actor.SpecialArgs[1])
                .FirstOrDefault(candidate => candidate.ClassDoomEdNum == PatrolMarkers.PointType)?.Id;
            actor.ReactionTime = unchecked(actor.SpecialArgs[2] * 35 + sim.Thinkers.Clock.Tic);
            actor.ChaseGoal = actor.SpecialArgs[3] != 0;
        }
        // A_Look treats goal reaction time as a map timestamp; A_Chase decrements it.
        if (ReactionTics != 0 && !attackTimelineActive && !lookingForTarget)
            ReactionTics = unchecked(ReactionTics - 1);
        var nativeTurned = nativeMovement && !attackTimelineActive;
        if (nativeTurned)
        {
            var facingTarget = sim.Actors.FirstOrDefault(candidate => candidate.Id == TargetId && !candidate.Destroyed);
            if (actor.StrafeCount != 0)
            {
                if (facingTarget is not null) ActorOrientationActions.Face(actor, facingTarget);
            }
            else if ((flags & MonsterChaseFlags.NoDirectionTurn) == 0)
                ActorWanderActions.TurnTowardMoveDirection(actor);
        }
        if (_healTics > 0) { _healTics--; Mode = MonsterMode.Heal; return; }
        if (_attackTic >= 0 && _profile?.RefireTic is >= 0)
        {
            var attackTarget = sim.Actors.FirstOrDefault(a => a.Id == TargetId && !a.Destroyed);
            if (attackTarget == null || attackTarget.IsDead)
            {
                TickProfile(sim, actor, attackTarget, false, false);
                return;
            }
        }
        var previousTargetId = TargetId;
        var hiddenTargetCleared = _attackTic < 0 && WindupTics == 0
            && sim.Actors.Any(candidate => candidate.Id == TargetId && candidate.Id != actor.GoalId
                && (candidate.Invisible || candidate.MinVisible));
        if (hiddenTargetCleared)
        {
            TargetId = null;
            Threshold = 0;
        }
        var unshootableTarget = _attackTic < 0 && WindupTics == 0
            ? sim.Actors.FirstOrDefault(candidate => candidate.Id == TargetId && candidate.Id != actor.GoalId
                && !candidate.Destroyed && !candidate.IsDead && !actor.IsFriend(candidate) && !candidate.Shootable
                && candidate is not PlayerPawn { NoTargetCheat: true })
            : null;
        if (unshootableTarget is not null)
        {
            if (unshootableTarget.NonShootable)
            {
                LastEnemyId = unshootableTarget.Id;
                Threshold = 0;
            }
            TargetId = null;
        }
        // HCDE rechecks the cheat during pursuit, clearing memory as well as the current target.
        if (sim.Actors.Any(candidate => candidate.Id == TargetId && candidate.Id != actor.GoalId
            && candidate is PlayerPawn { NoTargetCheat: true }))
        {
            TargetId = null;
            LastEnemyId = null;
            actor.LastHeardTargetId = null;
            Threshold = 0;
            WindupTics = 0;
            _attackTic = -1;
        }
        var target = sim.Actors.FirstOrDefault(a => a.Id == TargetId && !a.Destroyed
            && (a.Id == actor.GoalId || !a.IsDead && !actor.IsFriend(a)));
        var heardTargetId = actor.NoSector || sim.Compat.HasFlag(CompatSurface.SoundTarget)
            ? sim.SectorSoundTargetId(actor.SectorIndex) : actor.LastHeardTargetId;
        if (target is null && lookingForTarget && actor.TidToHate == 0
            && sim.Actors.Any(candidate => candidate.Id == heardTargetId && !candidate.Destroyed && !candidate.IsDead
                && candidate is PlayerPawn playerNoise && (playerNoise.NoTargetCheat || !playerNoise.Friendly)))
        {
            Mode = MonsterMode.Idle; TargetId = null; WindupTics = 0; _attackTic = -1;
            return;
        }
        var heard = target is null && actor.TidToHate == 0
            ? sim.Actors.FirstOrDefault(candidate => candidate.Id == heardTargetId && candidate.Shootable && !candidate.IsDead && !candidate.Destroyed
                && candidate is not PlayerPawn { NoTargetCheat: true }
                && (!actor.Ambush || actor.IsFriend(candidate) || (nativeMovement
                    ? CombatTrace.CheckSight(sim, actor, candidate, ActorSightFlags.SeePastBlockEverything)
                    : CombatTrace.HasLineOfSight(sim, actor, candidate))))
            : null;
        var friendlyNoise = lookingForTarget && heard is not null && actor.IsFriend(heard)
            && (actor.States.Current == actor.SpawnState || actor.States.Current == actor.IdleState);
        if (heard is not null && !actor.IsFriend(heard)) target = heard;
        if (target is null && actor.TidToHate != 0)
        {
            target = FindLocalHatee(sim, actor);
            if (target is not null) actor.LastLookActorId = target.Id;
            target ??= FindGlobalHatee(sim, actor, flags);
            target ??= sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed);
            target ??= TryResumeLastEnemy(sim, actor);
        }
        target ??= FindPlayerTarget(sim, actor, flags);
        target ??= sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed);
        target ??= TryResumeLastEnemy(sim, actor);
        var unshootableReplacementFound = unshootableTarget is not null && target is not null;
        target ??= unshootableTarget;
        if (target == null)
        {
            Mode = MonsterMode.Idle; TargetId = null; WindupTics = 0; _attackTic = -1;
            if (nativeMovement && (actor.Friendly || (flags & MonsterChaseFlags.DontIdle) != 0))
            {
                // Native A_DoChase calls ordinary A_Wander without forwarding chase flags.
                if ((flags & MonsterChaseFlags.DontMove) == 0) ActorWanderActions.WanderNative(actor);
                return;
            }
            if (friendlyNoise && actor.States.HasState(actor.SeeState)) actor.States.Enter(actor, actor.SeeState);
            else if (!lookingForTarget && !actor.Friendly) actor.SetIdle();
            return;
        }
        if (lookingForTarget && target.Id == actor.GoalId && ReactionTics > sim.Thinkers.Clock.Tic)
        {
            TargetId = null; WindupTics = 0; _attackTic = -1; Mode = MonsterMode.Idle;
            return;
        }
        if (TargetId != target.Id) { WindupTics = 0; _attackTic = -1; }
        TargetId = target.Id;
        if (previousTargetId != target.Id && target.Id != actor.GoalId && (actor.States.Current == actor.SpawnState || actor.States.Current == actor.IdleState))
            ActorSoundActions.SeeSound(actor);
        if ((actor.States.Current == actor.SpawnState || actor.States.Current == actor.IdleState) && actor.States.HasState(actor.SeeState))
        {
            Mode = MonsterMode.Chase;
            actor.States.Enter(actor, actor.SeeState);
            return;
        }
        if ((hiddenTargetCleared || unshootableReplacementFound) && target.Id != actor.GoalId) return;
        if (_nativeType == 64 && _attackTic < 0 && ArchvileActions.TryRaise(sim, actor, target, nativeMovement: nativeMovement))
        {
            _healTics = 29; Mode = MonsterMode.Heal; return;
        }
        if (actor.JustAttacked && _attackTic < 0 && WindupTics == 0)
        {
            actor.JustAttacked = false;
            if (!actor.IsFast() && (flags & (MonsterChaseFlags.DontMove | MonsterChaseFlags.NoPostAttackTurn | MonsterChaseFlags.StopIfBlocked)) == 0)
                MoveTowardTarget(sim, actor, target.X.ToDouble() - actor.X.ToDouble(),
                    target.Y.ToDouble() - actor.Y.ToDouble(), CheckMeleeRange(actor, target, true), flags,
                    nativeMovement, forceNewDirection: nativeMovement);
            if (nativeMovement && (flags & MonsterChaseFlags.StopIfBlocked) != 0)
                actor.MoveCount = sim.NextTryWalkByte() & 15;
            return;
        }
        var patrolGoal = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed);
        if (patrolGoal is not null && (actor.ChaseGoal || target == patrolGoal)
            && CheckMeleeRange(actor, patrolGoal, true))
        {
            var nextTid = patrolGoal.SpecialArgs[0];
            var specials = AcsActorTid.AllFromTid(sim, patrolGoal.ThingId)
                .Where(candidate => candidate.ClassDoomEdNum == PatrolMarkers.SpecialType).ToArray();
            foreach (var special in specials)
            {
                if (special.Destroyed) continue;
                ActorSpecialActions.CallSpecial(actor, special.Special, special.SpecialArgs[0], special.SpecialArgs[1],
                    special.SpecialArgs[2], special.SpecialArgs[3], special.SpecialArgs[4]);
            }
            var finalGoal = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed);
            var nextGoal = AcsActorTid.AllFromTid(sim, nextTid)
                .FirstOrDefault(candidate => candidate.ClassDoomEdNum == PatrolMarkers.PointType);
            var goalWasTarget = actor.TargetId == actor.GoalId;
            var delay = nextGoal is not null && goalWasTarget ? nextGoal.SpecialArgs[1] : 0;
            if (nextGoal is not null && goalWasTarget)
                actor.ReactionTime = unchecked(delay * 35 + sim.Thinkers.Clock.Tic);
            else
            {
                actor.ReactionTime = actor.SpawnReactionTime ?? 8;
                actor.Angle = (finalGoal ?? patrolGoal).Angle;
            }
            if (goalWasTarget) actor.SetTargetPointer(null);
            actor.JustAttacked = true;
            WindupTics = 0; _attackTic = -1;
            Mode = delay != 0 ? MonsterMode.Idle : MonsterMode.Chase;
            if (delay != 0)
            {
                actor.InCombat = true;
                actor.SetIdle();
            }
            actor.GoalId = nextGoal?.Id;
            return;
        }
        var visible = CombatTrace.HasLineOfSight(sim, actor, target);
        var pursuingGoal = target.Id == actor.GoalId;
        if (visible || pursuingGoal) { _lastX = target.X.ToDouble(); _lastY = target.Y.ToDouble(); }
        var dx = _lastX - actor.X.ToDouble();
        var dy = _lastY - actor.Y.ToDouble();
        if (!nativeTurned && nativeMovement && actor.StrafeCount != 0) ActorOrientationActions.Face(actor, target);
        else if (!nativeTurned && (flags & MonsterChaseFlags.NoDirectionTurn) == 0)
        {
            if (nativeMovement) ActorWanderActions.TurnTowardMoveDirection(actor);
            else actor.Angle = BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI);
        }
        var melee = CheckMeleeRange(actor, target, visible);
        if (nativeMovement && !attackTimelineActive && !pursuingGoal
            && (flags & (MonsterChaseFlags.FastChase | MonsterChaseFlags.DontMove)) == MonsterChaseFlags.FastChase)
        {
            if (actor.FastChaseStrafeCount > 0) actor.FastChaseStrafeCount--;
            else
            {
                actor.FastChaseStrafeCount = 0;
                actor.VelocityX = actor.VelocityY = default;
                if (Distance(actor, target) < 640 && sim.NextChaseByte() < 100)
                {
                    var radians = Math.Atan2(target.Y.ToDouble() - actor.Y.ToDouble(), target.X.ToDouble() - actor.X.ToDouble())
                        + (sim.NextChaseByte() < 128 ? Math.PI / 2 : -Math.PI / 2);
                    actor.VelocityX = Fixed.FromDouble(13 * Math.Cos(radians));
                    actor.VelocityY = Fixed.FromDouble(13 * Math.Sin(radians));
                    actor.FastChaseStrafeCount = 3;
                }
            }
        }
        if (pursuingGoal)
        {
            WindupTics = 0;
            _attackTic = -1;
        }
        var fearAllowsAttack = !nativeMovement || attackTimelineActive || pursuingGoal
            || !(actor.Frightened || target is PlayerPawn && target.Frightening)
            || sim.NextScaredyCatByte() < 43;
        if (nativeMovement && !attackTimelineActive && !pursuingGoal)
            melee = fearAllowsAttack && useMelee && (_profile is null || _profile.MeleeDice > 0)
                && CheckNativeMeleeRange(sim, actor, target);
        var meleeAttack = melee && useMelee && (_profile is null || _profile.MeleeDice > 0);
        // Native chase checks the signed walk count before missile-range checks (including JUSTHIT).
        var missileEnabled = useMissile && Attack != MonsterAttack.Melee
            && (!nativeMovement || actor.IsFast() || actor.MoveCount == 0 || _attackTic >= 0 || WindupTics > 0);
        var attackVisible = !pursuingGoal && fearAllowsAttack && SectorAllowsAttacks(actor)
            && (nativeMovement ? meleeAttack || missileEnabled && CombatTrace.HasMissileLineOfSight(sim, actor, target) : visible);
        var retaliate = false;
        if (attackVisible && !meleeAttack && missileEnabled && AttackCooldown == 0
            && _attackTic < 0 && WindupTics == 0 && actor.JustHit)
        {
            actor.JustHit = false;
            retaliate = !actor.Friendly || !target.IsDead && !actor.IsFriend(target);
        }
        var canAttack = attackVisible
            && (meleeAttack || missileEnabled && (retaliate || WindupTics > 0 || ReactionTics == 0
                && InOrdinaryMissileRange(actor, target, nativeMovement)
                && !HitFriend(sim, actor, target)
                && (!nativeMovement || CheckNativeMissileProbability(sim, actor, target))));
        if (canAttack && meleeAttack && AttackCooldown == 0 && _attackTic < 0 && WindupTics == 0
            && actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var attackSound) && !string.IsNullOrEmpty(attackSound))
            ActorSoundActions.PlayWeaponSound(actor, attackSound);
        if (_profile != null)
        {
            if (_attackTic >= 0)
            {
                TickProfile(sim, actor, target, melee, visible);
                return;
            }
            if (canAttack && AttackCooldown == 0)
            {
                if (!meleeAttack) actor.JustAttacked = actor.InCombat = true;
                _attackTic = 0;
                _vileFire = false;
                _separateMelee = meleeAttack && _profile.MeleeShotTic >= 0;
                if ((_separateMelee ? _profile.MeleeFaceTics : _profile.FaceTics)?.Contains(0) == true)
                    ActorOrientationActions.Face(actor, target);
                if (_nativeType == 66 && _separateMelee)
                    ActorSoundActions.PlayWeaponSound(actor, "skeleton/swing");
                if (_nativeType == 67)
                    ActorSoundActions.PlayWeaponSound(actor, "fatso/raiseguns");
                if (_nativeType == 64)
                    ActorSoundActions.StartSound(actor, "vile/start", channel: 2);
                WindupTics = _separateMelee ? _profile.MeleeShotTic : _profile.Shots[0];
                Mode = MonsterMode.Windup;
                return;
            }
        }
        else if (WindupTics > 0)
        {
            if (!canAttack) WindupTics = 0;
            else if (--WindupTics == 0)
            {
                if (meleeAttack) ActorDamage.Apply(target, 10, actor, inflictor: actor);
                else if (Attack == MonsterAttack.Fireball) sim.SpawnProjectile(actor, ProjectileKind.ImpBall, target);
                else
                {
                    FireHitscan(sim, actor, 1024, sim.NextCombatSpread() * 5.625, 0, 5);
                }
                AttackCooldown = 25;
                Mode = MonsterMode.Recovery;
            }
            return;
        }
        if (_profile == null && canAttack && AttackCooldown == 0)
        {
            if (!meleeAttack) actor.JustAttacked = actor.InCombat = true;
            WindupTics = 8; Mode = MonsterMode.Windup; return;
        }
        if (nativeMovement && Threshold == 0
            && (sim.GameMode != SpawnGameMode.Single || sim.Players.Count() > 1 || actor.TidToHate != 0)
            && !CombatTrace.CheckSight(sim, actor, target))
        {
            var lookForBetter = actor.NoSightCheck;
            Actor? replacement;
            try
            {
                if (lookForBetter) actor.NoSightCheck = false;
                replacement = FindForLook(sim, actor, new ActorLookParameters { NativeSight = true },
                    (flags & MonsterChaseFlags.DontLookAllAround) == 0, chaseGoal: true);
            }
            finally
            {
                if (lookForBetter) actor.NoSightCheck = true;
            }
            if (replacement is not null && replacement.Id != TargetId)
            {
                TargetId = replacement.Id;
                return;
            }
        }
        Mode = MonsterMode.Chase;
        if (nativeMovement && actor.StrafeCount != 0) actor.StrafeCount = unchecked(actor.StrafeCount - 1);
        if ((flags & MonsterChaseFlags.DontMove) == 0
            && ((flags & MonsterChaseFlags.FastChase) == 0 || actor.FastChaseStrafeCount == 0))
            MoveTowardTarget(sim, actor, dx, dy, melee, flags, nativeMovement);
        else if (nativeMovement && (flags & MonsterChaseFlags.DontMove) != 0 && actor.MoveCount > 0) actor.MoveCount--;
        if ((flags & MonsterChaseFlags.NoPlayActive) == 0 && sim.NextChaseByte() < 3)
            ActorSoundActions.PlayActiveSound(actor);
    }

    private void MoveTowardTarget(AuthoritySimulation sim, Actor actor, double dx, double dy, bool melee, MonsterChaseFlags flags,
        bool nativeMovement, bool forceNewDirection = false)
    {
        if ((flags & MonsterChaseFlags.DontMove) == 0)
        {
            // CHASEGOAL changes the movement destination without replacing the attack target.
            var movementGoal = actor.ChaseGoal
                ? sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.GoalId && !candidate.Destroyed)
                : null;
            var movementMelee = melee;
            if (movementGoal is not null)
            {
                dx = movementGoal.X.ToDouble() - actor.X.ToDouble();
                dy = movementGoal.Y.ToDouble() - actor.Y.ToDouble();
                movementMelee = CheckMeleeRange(actor, movementGoal, true);
            }
            if (!nativeMovement)
            {
                if (actor.X == _previousX && actor.Y == _previousY) _blockedTics++;
                else _blockedTics = 0;
                _previousX = actor.X; _previousY = actor.Y;
            }
            if (nativeMovement || dx * dx + dy * dy > 1 && !movementMelee)
            {
                if (nativeMovement)
                {
                    var movementTarget = movementGoal ?? sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
                    if (movementTarget is not null)
                    {
                        dx = movementTarget.X.ToDouble() - actor.X.ToDouble();
                        dy = movementTarget.Y.ToDouble() - actor.Y.ToDouble();
                        if (movementTarget.Id != actor.GoalId && !actor.NoFear && (actor.Frightened || movementTarget.Frightening))
                        { dx = -dx; dy = -dy; }
                    }
                    var oldX = actor.X.ToDouble();
                    var oldY = actor.Y.ToDouble();
                    string? oldFloorPic = actor.CantLeaveFloorPic && actor.SectorIndex >= 0
                        && actor.SectorIndex < sim.Level.Sectors.Count
                        ? sim.Level.Sectors[actor.SectorIndex].FloorPic : null;
                    if (forceNewDirection) ChooseNativeChaseDirection(sim, actor, dx, dy);
                    else
                    {
                        actor.MoveCount = unchecked(actor.MoveCount - 1);
                        if (actor.MoveCount < 0 && (flags & MonsterChaseFlags.NoRandomTurn) == 0
                            || !ActorWanderActions.Move(sim, actor) && (flags & MonsterChaseFlags.StopIfBlocked) == 0)
                            ChooseNativeChaseDirection(sim, actor, dx, dy);
                    }
                    if (oldFloorPic is not null && actor.SectorIndex >= 0 && actor.SectorIndex < sim.Level.Sectors.Count
                        && !string.Equals(oldFloorPic, sim.Level.Sectors[actor.SectorIndex].FloorPic, StringComparison.OrdinalIgnoreCase))
                    {
                        ActorPhysics.TryMove(sim, actor, oldX, oldY, out _);
                        if ((flags & MonsterChaseFlags.StopIfBlocked) == 0)
                            ChooseNativeChaseDirection(sim, actor, dx, dy);
                    }
                }
                else
                {
                    var radians = Math.Atan2(dy, dx);
                    if (_blockedTics > 2 && (flags & MonsterChaseFlags.StopIfBlocked) == 0)
                        radians += (actor.Id % 2 == 0 ? 1 : -1) * Math.PI / 4;
                    actor.VelocityX = Fixed.FromDouble(Math.Cos(radians) * actor.ChaseSpeed);
                    actor.VelocityY = Fixed.FromDouble(Math.Sin(radians) * actor.ChaseSpeed);
                }
            }
        }
    }

    private static void ChooseNativeChaseDirection(AuthoritySimulation sim, Actor actor, double dx, double dy)
    {
        actor.StrafeCount = 0;
        if (ActorPhysics.TryDropOffEscapeVector(sim, actor, out var escapeX, out var escapeY))
        {
            ActorWanderActions.ChooseChaseDirection(actor, escapeX, escapeY, avoidingDropOff: true);
            actor.MoveCount = 1;
        }
        else
        {
            var enemy = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
            if (actor.AvoidMelee && enemy is { Health: > 0 } && enemy.Id != actor.GoalId && !actor.IsFriend(enemy))
            {
                var distance = Distance(actor, enemy);
                var meleeAttacker = enemy is PlayerPawn player
                    ? player.Inventory.Selected is WeaponKind.Fist or WeaponKind.Chainsaw && distance < 192
                    : enemy.Brain?.Attack == MonsterAttack.Melee && distance < (enemy.MeleeRange.ToDouble() + enemy.Radius.ToDouble()) * 2;
                if (meleeAttacker)
                {
                    actor.StrafeCount = sim.NextEnemyStrafeByte() & 15;
                    dx = -dx; dy = -dy;
                }
            }
            ActorWanderActions.ChooseChaseDirection(actor, dx, dy);
            if (actor.StrafeCount != 0) actor.MoveCount = actor.StrafeCount;
        }
    }

    private bool CheckProfileRefire(AuthoritySimulation sim, Actor actor, Actor? target, bool visible)
    {
        if (target != null && HitFriend(sim, actor, target)) return false;
        if (target != null) ActorOrientationActions.Face(actor, target);
        return sim.CheckCPosRefire(visible, _nativeType is 7 or 68 ? (byte)10 : (byte)40);
    }

    private void TickProfile(AuthoritySimulation sim, Actor actor, Actor? target, bool melee, bool visible)
    {
        var profile = _profile!;
        _attackTic++;
        WindupTics = Math.Max(0, (_separateMelee ? profile.MeleeShotTic : profile.Shots[0]) - _attackTic);
        var end = _separateMelee ? profile.MeleeEndTic : profile.EndTic;
        if (!_separateMelee && _attackTic == profile.RefireTic
            && !CheckProfileRefire(sim, actor, target, visible))
        {
            _attackTic = -1; AttackCooldown = 1; Mode = MonsterMode.Chase; return;
        }
        if (_attackTic >= end)
        {
            if (!_separateMelee && profile.RepeatFrom >= 0
                && (profile.RefireTic >= 0 || visible))
                _attackTic = profile.RepeatFrom;
            else { _attackTic = -1; AttackCooldown = 1; Mode = MonsterMode.Chase; return; }
        }
        if (target != null && (_separateMelee ? profile.MeleeFaceTics : profile.FaceTics)?.Contains(_attackTic) == true)
            ActorOrientationActions.Face(actor, target);
        var shot = _separateMelee ? (_attackTic == profile.MeleeShotTic ? 0 : -1) : Array.IndexOf(profile.Shots, _attackTic);
        if (shot < 0 || target == null) return;
        Mode = MonsterMode.Recovery;
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        ActorOrientationActions.Face(actor, target);
        if (_nativeType == 64)
        {
            if (shot == 0) _vileFire = true;
            else ArchvileActions.Attack(sim, actor, target, _vileFire);
            return;
        }
        if (_nativeType == 3006)
        {
            ActorSkullActions.SkullAttack(actor);
            return;
        }
        if (_nativeType == 71)
        {
            sim.SpawnLostSoul(actor, target, actor.Angle.ToDegrees());
            return;
        }
        if (_separateMelee || Attack == MonsterAttack.Melee || profile.MeleeShotTic < 0 && melee && profile.MeleeDice > 0)
        {
            if (melee && visible)
            {
                var damage = _nativeType == 66 ? sim.NextSkelFistDamage()
                    : _nativeType is 3002 or 58 ? sim.NextSargAttackDamage()
                    : _nativeType == 3001 ? sim.NextTroopAttackDamage()
                    : _nativeType == 3005 ? sim.NextHeadAttackDamage()
                    : _nativeType is 3003 or 69 ? sim.NextBruisAttackDamage()
                    : (1 + (int)(sim.NextCombatRandom() % (uint)profile.MeleeDice)) * profile.MeleeMultiplier;
                if (_nativeType == 66) ActorSoundActions.PlayWeaponSound(actor, "skeleton/melee");
                if (_nativeType == 3001) ActorSoundActions.PlayWeaponSound(actor, "imp/melee");
                if (_nativeType is 3003 or 69) ActorSoundActions.PlayWeaponSound(actor, "baron/melee");
                if (_nativeType == 3005 && actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var attackSound)
                    && !string.IsNullOrEmpty(attackSound))
                    ActorSoundActions.PlayWeaponSound(actor, attackSound);
                ActorDamage.Apply(target, damage, actor, damageType: "Melee", inflictor: actor);
            }
            return;
        }
        if (profile.Projectile is { } kind)
        {
            if (_nativeType == 67)
            {
                if (shot < 2)
                    actor.Angle = BamAngle.FromDegrees(actor.Angle.ToDegrees() + (shot == 0 ? 11.25 : -11.25));
                // SpawnMissile aims at the target; the second shot receives the extra yaw deviation.
                var offsets = shot switch { 0 => new[] { 0d, 11.25 }, 1 => new[] { 0d, -11.25 }, _ => new[] { -5.625, 5.625 } };
                foreach (var offset in offsets)
                {
                    var missile = sim.SpawnProjectile(actor, kind, target);
                    if (offset != 0) missile.RotateYaw(offset, useMovementSpeed: true);
                }
            }
            else sim.SpawnProjectile(actor, kind, target);
            return;
        }
        var pitch = -Math.Atan2(target.Center() - actor.AttackOriginZ(), Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI;
        if (_nativeType == 3004)
        {
            ActorSoundActions.PlayWeaponSound(actor, "grunt/attack");
            var attack = sim.NextPosAttackShot();
            FireHitscan(sim, actor, 2048, attack.Spread, pitch, attack.Damage);
            return;
        }
        if (_nativeType is 65 or 84)
        {
            if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var sound) && !string.IsNullOrEmpty(sound))
                ActorSoundActions.PlayWeaponSound(actor, sound);
            var attack = sim.NextCPosAttackShot();
            FireHitscan(sim, actor, 2048, attack.Spread, pitch, attack.Damage);
            return;
        }
        if (_nativeType is 9 or 7)
        {
            if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var sound) && !string.IsNullOrEmpty(sound))
                ActorSoundActions.PlayWeaponSound(actor, sound);
            for (var pellet = 0; pellet < 3; pellet++)
            {
                var attack = sim.NextSPosAttackPellet();
                FireHitscan(sim, actor, 2048, attack.Spread, pitch, attack.Damage);
            }
            return;
        }
        for (var pellet = 0; pellet < profile.Pellets; pellet++)
        {
            var spread = sim.NextCombatSpread() * (22.5 * 255 / 256);
            var damage = 3 * (1 + (int)(sim.NextCombatRandom() % 5));
            FireHitscan(sim, actor, 2048, spread, pitch, damage);
        }
    }

    internal static bool HitFriend(AuthoritySimulation sim, Actor source, Actor target)
    {
        if (!source.Friendly) return false;
        var dx = target.X.ToDouble() - source.X.ToDouble();
        var dy = target.Y.ToDouble() - source.Y.ToDouble();
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var pitch = -Math.Atan2(target.Center() - source.AttackOriginZ(), Math.Max(1, distance)) * 180 / Math.PI;
        var hit = CombatTrace.TraceLineAttack(sim, source, BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI),
            BamAngle.FromDegrees(pitch), distance, requireDamageable: false).Victim;
        return hit != null && hit != target && source.IsFriend(hit);
    }

    internal static void FireHitscan(AuthoritySimulation sim, Actor source, double range, double spread, double pitch, int damage)
    {
        var hit = CombatTrace.TraceLineAttack(sim, source,
            BamAngle.FromDegrees(source.Angle.ToDegrees() + spread), BamAngle.FromDegrees(pitch), range);
        if (hit.Victim is { } victim) ActorDamage.Apply(victim, damage, source, damageType: "Hitscan", inflictor: source);
        else GeometryLineAttack.Apply(sim, hit, damage);
    }

    // Die clears MF_SKULLFLY without applying the collision/pain charge reset.
    internal void ClearDeathCharge() => Charging = false;

    internal void StopCharge(Actor actor)
    {
        Charging = false;
        _attackTic = -1;
        WindupTics = 0;
        AttackCooldown = 1;
        Mode = MonsterMode.Chase;
        actor.VelocityX = actor.VelocityY = actor.VelocityZ = default;
    }

    internal bool AdvanceRaiseFrame()
    {
        if (_raiseTics <= 0) return false;
        _raiseTics--;
        return true;
    }

    internal void Revive(Actor actor)
    {
        StopCharge(actor);
        _deathTics = 0; _deathTargetId = null; _healTics = 0; _vileFire = false;
        TargetId = null;
        LastEnemyId = null;
        _raiseTics = actor.RaiseState >= 0 ? 0 : actor.RaiseDuration;
        AttackCooldown = 0;
        Mode = MonsterMode.Raise;
        actor.LastDamageSourceId = null;
    }

    internal void StartCharge(Actor actor, Actor target, double skullSpeed = 20)
    {
        if (skullSpeed <= 0) skullSpeed = 20;
        TargetId = target.Id;
        Charging = true;
        Mode = MonsterMode.Recovery;
        if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var attackSound)
            && !string.IsNullOrEmpty(attackSound))
            ActorSoundActions.StartSound(actor, attackSound, channel: 2);
        actor.Ambush = false;
        actor.Angle = BamAngle.FromDegrees(actor.AngleTo(target));
        actor.VelFromAngle(skullSpeed);
        var travel = actor.DistanceBySpeed(target, skullSpeed);
        actor.VelocityZ = Fixed.FromDouble((target.Z.ToDouble() + target.Height.ToDouble() / 2 - actor.Z.ToDouble()) / travel);
    }

    /// <summary>Native P_CheckMeleeRange distance, vertical, friendship and sight gates.</summary>
    internal static bool CheckMeleeRange(Actor actor, Actor target, bool visible, double range = -1) =>
        Distance(actor, target)
            < (range < 0 ? actor.MeleeRange.ToDouble() : range) + target.Radius.ToDouble()
        && (actor.GoalId == target.Id || SectorAllowsAttacks(actor)
        && (actor.NoVerticalMeleeRange || (target.Z.ToDouble() <= actor.Z.ToDouble() + actor.Height.ToDouble()
            && target.Z.ToDouble() + target.Height.ToDouble() >= actor.Z.ToDouble()))
        && !actor.IsFriend(target) && visible);

    internal static bool CheckNativeMeleeRange(AuthoritySimulation sim, Actor actor, Actor target, double range = -1)
        => CheckMeleeRange(actor, target, visible: true, range)
            && (actor.GoalId == target.Id || CombatTrace.CheckSight(sim, actor, target));

    private static bool SectorAllowsAttacks(Actor actor) => actor.Level is not { } level
        || actor.SectorIndex < 0 || actor.SectorIndex >= level.Sectors.Count
        || (!level.Sectors[actor.SectorIndex].NoAttack
            && (level.Sectors[actor.SectorIndex].RuntimeFlags & 2048) == 0);

    private static Actor? FindPlayerTarget(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags,
        ActorLookParameters? parameters = null)
    {
        if (actor.TidToHate != 0 && !actor.HuntPlayers) return null;
        if (actor.TidToHate == 0 && actor.Friendly && sim.Level.Blockmap is not null)
        {
            var enemy = FriendlyEnemySearch.Find(sim, actor, parameters);
            if (enemy is not null) return enemy;
            if (actor.FriendPlayer == 0 || sim.GameMode != SpawnGameMode.Deathmatch) return null;
        }
        else if (actor.TidToHate == 0 && actor.SeeFriendlyMonsters && sim.Level.Blockmap is not null)
        {
            var enemy = FriendlyEnemySearch.Find(sim, actor, parameters);
            if (enemy is not null) return enemy;
        }
        var slot = actor.TidToHate != 0 ? sim.NextPlayerSearchStart() : actor.LastLookPlayerNumber;
        var players = sim.Players.Where(p => !p.Destroyed).ToArray();
        for (var checkedSlots = 0; checkedSlots < 64; checkedSlots++)
        {
            slot = (slot + 1) & 63;
            var player = players.FirstOrDefault(p => p.PlayerNum == slot);
            if (player is null) continue;
            if (actor.TidToHate == 0) actor.LastLookPlayerNumber = slot;
            if (IsTargetablePlayer(sim, actor, player, (flags & MonsterChaseFlags.DontLookAllAround) == 0, parameters)) return player;
        }
        return null;
    }

    internal Actor? FindForLook(AuthoritySimulation sim, Actor actor, ActorLookParameters parameters, bool allAround, bool chaseGoal)
    {
        var flags = allAround ? MonsterChaseFlags.None : MonsterChaseFlags.DontLookAllAround;
        if (actor.TidToHate != 0)
        {
            var hatee = FindLocalHatee(sim, actor, parameters);
            if (hatee is not null) actor.LastLookActorId = hatee.Id;
            hatee ??= FindGlobalHatee(sim, actor, flags, parameters);
            if (hatee is not null)
            {
                if (actor.TargetId == actor.GoalId && actor.GoalId is not null) actor.ReactionTime = 0;
                return hatee;
            }
            var fallback = (chaseGoal ? sim.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == actor.GoalId) : null)
                ?? TryResumeLastEnemy(sim, actor);
            if (fallback is not null || !actor.HuntPlayers) return fallback;
        }
        return FindPlayerTarget(sim, actor, flags, parameters)
            ?? (chaseGoal ? sim.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == actor.GoalId) : null)
            ?? TryResumeLastEnemy(sim, actor);
    }

    internal static bool IsTargetablePlayer(AuthoritySimulation sim, Actor actor, PlayerPawn player, bool allAround,
        ActorLookParameters? parameters = null)
        => !player.Destroyed && player.Shootable && player.Health > 0 && !player.NoTargetCheat
            && !actor.IsFriend(player) && ActorVisibility.IsVisible(sim, actor, player, allAround, parameters)
            && DetectInvisiblePlayer(sim, actor, player);

    private static bool DetectInvisiblePlayer(AuthoritySimulation sim, Actor actor, PlayerPawn player)
    {
        if (actor.SeeInvisible || !(player.Ghost || player.Shadow && !sim.CompatibilityInvisibility)) return true;
        var vx = player.VelocityX.ToDouble(); var vy = player.VelocityY.ToDouble();
        if (Distance(actor, player) > 128 && vx * vx + vy * vy < 25) return false;
        return sim.NextPlayerLookByte() >= 225;
    }

    private bool InOrdinaryMissileRange(Actor actor, Actor target, bool nativeMovement)
    {
        if (nativeMovement)
        {
            var adjusted = AdjustedMissileDistance(actor, target);
            var maximum = actor.MaxTargetRange ?? (_nativeType == 64 ? 896 : 0);
            var minimum = actor.MeleeThreshold ?? (_nativeType == 66 ? 196 : 0);
            return !(maximum > 0 && adjusted > maximum)
                && !(_profile is { MeleeDice: > 0 } && adjusted < minimum);
        }
        var distance = Distance(actor, target);
        // Native range properties apply after the melee-state distance adjustments.
        if (_nativeType == 64) return distance - 64 - 128 <= 896;
        if (_nativeType == 66 && distance - 64 < 196) return false;
        return true;
    }

    /// <summary>Represented native CheckMissileRange action. Chase-only walk-count and fear gates are separate.</summary>
    public bool CheckMissileRange(AuthoritySimulation sim, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(actor);
        var target = sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.TargetId && !candidate.Destroyed);
        if (target is null || !SectorAllowsAttacks(actor) || !CombatTrace.HasMissileLineOfSight(sim, actor, target)) return false;
        if (actor.JustHit)
        {
            actor.JustHit = false;
            if (!actor.Friendly) return true;
            if (target.IsDead) return false;
            if (!actor.IsFriend(target)) return true;
            return (target is PlayerPawn || !target.JustHit) && sim.NextDefectByte() > 128;
        }
        if (actor.ReactionTime != 0 || actor.IsFriend(target) || HitFriend(sim, actor, target)) return false;
        return InOrdinaryMissileRange(actor, target, nativeMovement: true)
            && CheckNativeMissileProbability(sim, actor, target, forceProbability: true);
    }

    private bool CheckNativeMissileProbability(AuthoritySimulation sim, Actor actor, Actor target, bool forceProbability = false)
    {
        // Unconfigured generic brains retain their existing deterministic attack selection.
        if (!forceProbability && _profile is null && actor.MinMissileChance is null && actor.MissileChanceMult is null
            && actor.MaxTargetRange is null && actor.MeleeThreshold is null) return true;
        var distance = AdjustedMissileDistance(actor, target);
        distance *= actor.MissileChanceMult ?? (_nativeType is 7 or 16 or 66 or 3006 ? 0.5 : 1);
        var cap = (int)((actor.MinMissileChance ?? (_nativeType == 16 ? 160 : 200)) * (1 - sim.SkillAggressiveness));
        var threshold = Math.Min((int)Math.Clamp(distance, int.MinValue, int.MaxValue), cap);
        return sim.NextCheckMissileByte() >= threshold;
    }

    private double AdjustedMissileDistance(Actor actor, Actor target)
        => Distance(actor, target) - 64 - ((_profile?.MeleeDice ?? 0) == 0 ? 128 : 0);

    private static Actor? FindGlobalHatee(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags,
        ActorLookParameters? parameters = null)
    {
        var cursor = actor.LastLookActorId;
        if (cursor is { } id && !sim.Actors.Any(a => !a.Destroyed && a.Id == id && a.ThingId == actor.TidToHate))
            actor.LastLookActorId = cursor = null;
        var candidates = sim.Actors.Where(a => !a.Destroyed && a.ThingId == actor.TidToHate).ToArray();
        var budget = sim.NextHateSearchBudget();
        var index = Array.FindIndex(candidates, a => a.Id == cursor);
        var limit = index >= 0 ? candidates.Length - 1 : candidates.Length;
        for (var offset = 1; offset <= limit; offset++)
        {
            var candidate = candidates[(index + offset) % candidates.Length];
            if (candidate == actor || !candidate.Shootable || candidate.IsDead || candidate.Dormant) continue;
            actor.LastLookActorId = candidate.Id;
            if (--budget == 0) return null;
            if (actor.NoSightCheck || ActorVisibility.IsVisible(sim, actor, candidate,
                (flags & MonsterChaseFlags.DontLookAllAround) == 0, parameters)) return candidate;
        }
        actor.LastLookActorId = cursor;
        return null;
    }

    private static Actor? FindLocalHatee(AuthoritySimulation sim, Actor actor, ActorLookParameters? parameters = null)
    {
        if (sim.Level.Blockmap is not { } map) return null;
        var header = map.Header;
        var cellX = Math.Floor((actor.X.ToDouble() - header.OriginX) / 128);
        var cellY = Math.Floor((actor.Y.ToDouble() - header.OriginY) / 128);
        if (cellX < 0 || cellY < 0 || cellX >= header.Width || cellY >= header.Height) return null;
        var left = header.OriginX + cellX * 128; var bottom = header.OriginY + cellY * 128;
        foreach (var link in sim.Actors.Reverse())
        {
            if (link == actor || link.Destroyed || !link.IsBlockmapActor || !link.Shootable || link.IsDead || link.Dormant) continue;
            var radius = link.Radius.ToDouble();
            if (link.X.ToDouble() + radius < left || link.X.ToDouble() - radius >= left + 128
                || link.Y.ToDouble() + radius < bottom || link.Y.ToDouble() - radius >= bottom + 128) continue;
            var other = link.ThingId == actor.TidToHate ? link
                : sim.Actors.FirstOrDefault(candidate => !candidate.Destroyed && candidate.Id == link.TargetId
                    && candidate.ThingId == actor.TidToHate);
            if (other is null || !other.Shootable || other.IsDead || other.Dormant) continue;
            if (actor.NoSightCheck || ActorVisibility.IsVisible(sim, actor, other, true, parameters)) return other;
        }
        return null;
    }

    private static double Distance(Actor a, Actor b) => Math.Sqrt(
        Math.Pow(a.X.ToDouble() - b.X.ToDouble(), 2) + Math.Pow(a.Y.ToDouble() - b.Y.ToDouble(), 2));
}
