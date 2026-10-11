namespace HCDE.Playsim;

public enum MonsterAttack { Melee, Hitscan, Fireball }
public enum MonsterMode { Idle, Chase, Windup, Recovery, Pain, Dead, Heal, Raise }

[Flags]
public enum MonsterChaseFlags
{
    None = 0,
    NoPlayActive = 2,
    NightmareFast = 4,
    DontMove = 16,
    NoDirectionTurn = 64,
    StopIfBlocked = 256,
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
    public MonsterMode Mode { get; private set; }
    public uint? TargetId { get; private set; }
    /// <summary>Native <c>lastenemy</c>. Updated when wake-up switches chase targets.</summary>
    public uint? LastEnemyId { get; private set; }
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
        if (!Enabled || !actor.CanTakeDamage || !target.CanTakeDamage || target is PlayerPawn { NoTarget: true } || actor.IsFriend(target) || TargetId != null
            || actor.Ambush && !CombatTrace.HasLineOfSight(sim, actor, target)) return;
        if (actor.States.Current == actor.SpawnState) Threshold = 0;
        TargetId = target.Id;
        _lastX = target.X.ToDouble();
        _lastY = target.Y.ToDouble();
        if (actor.States.Current == actor.SpawnState) ActorSoundActions.SeeSound(actor);
        if (actor.States.Current == actor.SpawnState && actor.States.HasState(actor.SeeState))
            actor.States.Enter(actor, actor.SeeState);
    }

    internal void ClearTarget() => TargetId = null;

    internal void ClearActionTargets() { TargetId = null; LastEnemyId = null; }
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

    public void Tick(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = MonsterChaseFlags.None,
        bool useMelee = true, bool useMissile = true, bool nightmareFast = false)
    {
        if ((flags & ~(MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.NightmareFast | MonsterChaseFlags.DontMove | MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.StopIfBlocked | MonsterChaseFlags.DontLookAllAround)) != 0)
            throw new NotSupportedException("The requested native chase flags are not yet supported.");
        if (_inChase) return;
        _inChase = true;
        try { TickCore(sim, actor, flags, useMelee, useMissile,
            nightmareFast || (flags & MonsterChaseFlags.NightmareFast) != 0); }
        finally { _inChase = false; }
    }

    private void TickCore(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags, bool useMelee, bool useMissile, bool nightmareFast)
    {
        if (!Enabled) return;
        if (Threshold != 0)
        {
            var thresholdTarget = sim.Actors.FirstOrDefault(candidate => candidate.Id == TargetId && !candidate.Destroyed);
            Threshold = thresholdTarget == null || thresholdTarget.IsDead ? 0 : unchecked(Threshold - 1);
        }
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
        actor.VelocityX = actor.VelocityY = default;
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
        if (ReactionTics != 0) ReactionTics = unchecked(ReactionTics - 1);
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
        // HCDE rechecks the cheat during pursuit, clearing memory as well as the current target.
        if (sim.Actors.Any(candidate => candidate.Id == TargetId && candidate.Id != actor.GoalId
            && candidate is PlayerPawn { NoTarget: true }))
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
        target ??= sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.LastHeardTargetId && candidate.CanTakeDamage && !actor.IsFriend(candidate)
            && candidate is not PlayerPawn { NoTarget: true }
            && (!actor.Ambush || CombatTrace.HasLineOfSight(sim, actor, candidate)));
        target ??= sim.Players.Where(p => p.CanTakeDamage && !p.NoTarget && !actor.IsFriend(p)
                && ((flags & MonsterChaseFlags.DontLookAllAround) == 0 || InAcquisitionView(actor, p))
                && CombatTrace.HasLineOfSight(sim, actor, p))
            .OrderBy(p => Distance(actor, p)).ThenBy(p => p.Id).FirstOrDefault();
        target ??= TryResumeLastEnemy(sim, actor);
        if (target == null)
        {
            Mode = MonsterMode.Idle; TargetId = null; WindupTics = 0; _attackTic = -1; return;
        }
        if (TargetId != target.Id) { WindupTics = 0; _attackTic = -1; }
        TargetId = target.Id;
        if (previousTargetId != target.Id && actor.States.Current == actor.SpawnState)
            ActorSoundActions.SeeSound(actor);
        if (actor.States.Current == actor.SpawnState && actor.States.HasState(actor.SeeState))
            actor.States.Enter(actor, actor.SeeState);
        if (_nativeType == 64 && _attackTic < 0 && ArchvileActions.TryRaise(sim, actor, target))
        {
            _healTics = 29; Mode = MonsterMode.Heal; return;
        }
        var visible = CombatTrace.HasLineOfSight(sim, actor, target);
        if (visible) { _lastX = target.X.ToDouble(); _lastY = target.Y.ToDouble(); }
        var dx = _lastX - actor.X.ToDouble();
        var dy = _lastY - actor.Y.ToDouble();
        if ((flags & MonsterChaseFlags.NoDirectionTurn) == 0)
            actor.Angle = BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI);
        var melee = CheckMeleeRange(actor, target, visible);
        var meleeAttack = melee && useMelee && (_profile is null || _profile.MeleeDice > 0);
        var pursuingGoal = target.Id == actor.GoalId;
        if (pursuingGoal)
        {
            WindupTics = 0;
            _attackTic = -1;
        }
        var attackVisible = !pursuingGoal && SectorAllowsAttacks(actor) && visible;
        var missileEnabled = useMissile && Attack != MonsterAttack.Melee;
        var retaliate = false;
        if (attackVisible && !meleeAttack && missileEnabled && AttackCooldown == 0
            && _attackTic < 0 && WindupTics == 0 && actor.JustHit)
        {
            actor.JustHit = false;
            retaliate = !actor.Friendly || !target.IsDead && !actor.IsFriend(target);
        }
        var canAttack = attackVisible
            && (meleeAttack || missileEnabled && (retaliate || WindupTics > 0 || ReactionTics == 0
                && InOrdinaryMissileRange(actor, target)
                && !HitFriend(sim, actor, target)));
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
            WindupTics = 8; Mode = MonsterMode.Windup; return;
        }
        Mode = MonsterMode.Chase;
        // Local obstacle steering. This is not a navigation mesh or native P_NewChaseDir.
        if ((flags & MonsterChaseFlags.DontMove) == 0)
        {
            if (actor.X == _previousX && actor.Y == _previousY) _blockedTics++;
            else _blockedTics = 0;
            _previousX = actor.X; _previousY = actor.Y;
            var radians = Math.Atan2(dy, dx);
            if (_blockedTics > 2 && (flags & MonsterChaseFlags.StopIfBlocked) == 0)
                radians += (actor.Id % 2 == 0 ? 1 : -1) * Math.PI / 4;
            if (dx * dx + dy * dy > 1 && !melee)
            {
                actor.VelocityX = Fixed.FromDouble(Math.Cos(radians) * actor.ChaseSpeed);
                actor.VelocityY = Fixed.FromDouble(Math.Sin(radians) * actor.ChaseSpeed);
            }
        }
        if ((flags & MonsterChaseFlags.NoPlayActive) == 0 && sim.NextChaseByte() < 3)
            ActorSoundActions.PlayActiveSound(actor);
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
        var pitch = -Math.Atan2(target.Z.ToDouble() + target.Height.ToDouble() / 2
            - actor.Z.ToDouble() - actor.Height.ToDouble() / 2 - 8, Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI;
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
        var pitch = -Math.Atan2(target.Z.ToDouble() + target.Height.ToDouble() / 2
            - source.Z.ToDouble() - source.Height.ToDouble() / 2 - 8, Math.Max(1, distance)) * 180 / Math.PI;
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

    private static bool SectorAllowsAttacks(Actor actor) => actor.Level is not { } level
        || actor.SectorIndex < 0 || actor.SectorIndex >= level.Sectors.Count
        || !level.Sectors[actor.SectorIndex].NoAttack;

    private static bool InAcquisitionView(Actor actor, Actor target)
    {
        if (Distance(actor, target) <= actor.MeleeRange.ToDouble() + actor.Radius.ToDouble()) return true;
        var angle = Math.Atan2(target.Y.ToDouble() - actor.Y.ToDouble(), target.X.ToDouble() - actor.X.ToDouble()) * 180 / Math.PI;
        return Math.Abs(Math.IEEERemainder(angle - actor.Angle.ToDegrees(), 360)) <= 90;
    }

    private bool InOrdinaryMissileRange(Actor actor, Actor target)
    {
        var distance = Distance(actor, target);
        // Native range properties apply after the melee-state distance adjustments.
        if (_nativeType == 64) return distance - 64 - 128 <= 896;
        if (_nativeType == 66 && distance - 64 < 196) return false;
        return true;
    }

    private static double Distance(Actor a, Actor b) => Math.Sqrt(
        Math.Pow(a.X.ToDouble() - b.X.ToDouble(), 2) + Math.Pow(a.Y.ToDouble() - b.Y.ToDouble(), 2));
}
