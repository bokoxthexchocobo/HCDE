namespace HCDE.Playsim;

public enum MonsterAttack { Melee, Hitscan, Fireball }
public enum MonsterMode { Idle, Chase, Windup, Recovery, Pain, Dead, Heal, Raise }

/// <summary>Deterministic managed look/chase/attack loop; specialized native actions remain separate.</summary>
public sealed class MonsterBrain(MonsterAttack attack)
{
    private DoomMonsterAttack? _profile;
    private int _nativeType;
    private int _attackTic = -1;
    private bool _separateMelee;
    public bool Charging { get; private set; }
    private int _deathTics;
    private uint? _deathTargetId;
    private int _healTics;
    private int _raiseTics;
    public int RaiseTics => _raiseTics;
    private bool _vileFire;
    internal SimPainDeath? CapturePainDeath() => _nativeType == 71 ? new(_deathTics, _deathTargetId) : null;
    internal void RestorePainDeath(SimPainDeath state) { _deathTics = state.Tics; _deathTargetId = state.TargetId; }
    internal bool HasPendingDeathAction => _nativeType == 71 && _deathTics < 32;
    public bool Enabled { get; set; } = true;
    public MonsterAttack Attack { get; } = attack;
    public MonsterMode Mode { get; private set; }
    public uint? TargetId { get; private set; }
    /// <summary>Native <c>lastenemy</c>. Updated when wake-up switches chase targets.</summary>
    public uint? LastEnemyId { get; private set; }
    /// <summary>Native chase threshold. While positive, <see cref="OkayToSwitchTarget"/> blocks a new target.</summary>
    public int Threshold { get; private set; }
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
        if (source == null || !source.CanTakeDamage || source.Id == actor.Id) return;
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
        if (last == null || !last.CanTakeDamage || actor.IsFriend(last))
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
        if (!Enabled || !actor.CanTakeDamage || !target.CanTakeDamage || actor.IsFriend(target) || TargetId != null
            || actor.Ambush && !CombatTrace.HasLineOfSight(sim, actor, target)) return;
        TargetId = target.Id;
        _lastX = target.X.ToDouble();
        _lastY = target.Y.ToDouble();
    }

    internal void ClearTarget() => TargetId = null;

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

    public void Tick(AuthoritySimulation sim, Actor actor)
    {
        if (!Enabled) return;
        actor.JustHit = false;
        if (Threshold != 0)
        {
            var thresholdTarget = sim.Actors.FirstOrDefault(candidate => candidate.Id == TargetId && !candidate.Destroyed);
            Threshold = thresholdTarget == null || thresholdTarget.IsDead ? 0 : unchecked(Threshold - 1);
        }
        if (Charging && (actor.IsDead || actor.Destroyed || actor.States.Current == actor.PainState))
            StopCharge(actor);
        if (Charging) return;
        actor.VelocityX = actor.VelocityY = default;
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
        _deathTics = 0; _deathTargetId = null;
        if (AttackCooldown > 0) AttackCooldown--;
        if (actor.States.Current == actor.PainState)
        {
            _raiseTics = 0;
            _healTics = 0;
            Mode = MonsterMode.Pain; WindupTics = 0; _attackTic = -1; return;
        }
        if (AdvanceRaiseFrame()) return;
        if (ReactionTics != 0) { ReactionTics = unchecked(ReactionTics - 1); return; }
        if (_healTics > 0) { _healTics--; Mode = MonsterMode.Heal; return; }
        var target = sim.Actors.FirstOrDefault(a => a.Id == TargetId && a.CanTakeDamage && !actor.IsFriend(a));
        target ??= sim.Actors.FirstOrDefault(candidate => candidate.Id == actor.LastHeardTargetId && candidate.CanTakeDamage && !actor.IsFriend(candidate)
            && (!actor.Ambush || CombatTrace.HasLineOfSight(sim, actor, candidate)));
        target ??= sim.Players.Where(p => p.CanTakeDamage && !actor.IsFriend(p) && Distance(actor, p) <= 2048 && CombatTrace.HasLineOfSight(sim, actor, p))
            .OrderBy(p => Distance(actor, p)).ThenBy(p => p.Id).FirstOrDefault();
        target ??= TryResumeLastEnemy(sim, actor);
        if (target == null)
        {
            Mode = MonsterMode.Idle; TargetId = null; WindupTics = 0; _attackTic = -1; return;
        }
        if (TargetId != target.Id) { WindupTics = 0; _attackTic = -1; }
        TargetId = target.Id;
        if (_nativeType == 64 && _attackTic < 0 && ArchvileActions.TryRaise(sim, actor, target))
        {
            _healTics = 29; Mode = MonsterMode.Heal; return;
        }
        var visible = CombatTrace.HasLineOfSight(sim, actor, target);
        if (visible) { _lastX = target.X.ToDouble(); _lastY = target.Y.ToDouble(); }
        var dx = _lastX - actor.X.ToDouble();
        var dy = _lastY - actor.Y.ToDouble();
        actor.Angle = BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI);
        var melee = CheckMeleeRange(actor, target, visible);
        var canAttack = visible && (melee || Attack != MonsterAttack.Melee && Distance(actor, target) <= (_nativeType == 64 ? 896 : 1024));
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
                _separateMelee = melee && _profile.MeleeShotTic >= 0;
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
                if (melee) ActorDamage.Apply(target, 10, actor, inflictor: actor);
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
        if (actor.X == _previousX && actor.Y == _previousY) _blockedTics++;
        else _blockedTics = 0;
        _previousX = actor.X; _previousY = actor.Y;
        var radians = Math.Atan2(dy, dx);
        if (_blockedTics > 2) radians += (actor.Id % 2 == 0 ? 1 : -1) * Math.PI / 4;
        if (dx * dx + dy * dy > 1 && !melee)
        {
            actor.VelocityX = Fixed.FromDouble(Math.Cos(radians) * actor.ChaseSpeed);
            actor.VelocityY = Fixed.FromDouble(Math.Sin(radians) * actor.ChaseSpeed);
        }
    }

    private void TickProfile(AuthoritySimulation sim, Actor actor, Actor target, bool melee, bool visible)
    {
        var profile = _profile!;
        _attackTic++;
        WindupTics = Math.Max(0, (_separateMelee ? profile.MeleeShotTic : profile.Shots[0]) - _attackTic);
        var end = _separateMelee ? profile.MeleeEndTic : profile.EndTic;
        if (_attackTic >= end)
        {
            if (!_separateMelee && profile.RepeatFrom >= 0 && visible)
                _attackTic = profile.RepeatFrom;
            else { _attackTic = -1; AttackCooldown = 1; Mode = MonsterMode.Chase; return; }
        }
        var shot = _separateMelee ? (_attackTic == profile.MeleeShotTic ? 0 : -1) : Array.IndexOf(profile.Shots, _attackTic);
        if (shot < 0) return;
        Mode = MonsterMode.Recovery;
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        actor.Angle = BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI);
        if (_nativeType == 64)
        {
            if (shot == 0) _vileFire = true;
            else if (visible) ArchvileActions.Attack(sim, actor, target, _vileFire);
            return;
        }
        if (_nativeType == 3006)
        {
            StartCharge(actor, target);
            return;
        }
        if (_nativeType == 71)
        {
            sim.SpawnLostSoul(actor, target, actor.Angle.ToDegrees());
            return;
        }
        if (_separateMelee || Attack == MonsterAttack.Melee || melee && profile.MeleeDice > 0)
        {
            if (melee && visible) ActorDamage.Apply(target,
                (1 + (int)(sim.NextCombatRandom() % (uint)profile.MeleeDice)) * profile.MeleeMultiplier, actor, inflictor: actor);
            return;
        }
        if (profile.Projectile is { } kind)
        {
            if (_nativeType == 67)
            {
                // SpawnMissile aims at the target; the second shot receives the extra yaw deviation.
                var offsets = shot switch { 0 => new[] { 0d, 11.25 }, 1 => new[] { 0d, -11.25 }, _ => new[] { -5.625, 5.625 } };
                foreach (var offset in offsets) sim.SpawnProjectile(actor, kind, target).RotateYaw(offset);
            }
            else sim.SpawnProjectile(actor, kind, target);
            return;
        }
        var pitch = -Math.Atan2(target.Z.ToDouble() + target.Height.ToDouble() / 2
            - actor.Z.ToDouble() - actor.Height.ToDouble() / 2 - 8, Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI;
        for (var pellet = 0; pellet < profile.Pellets; pellet++)
        {
            var spread = sim.NextCombatSpread() * (22.5 * 255 / 256);
            var damage = 3 * (1 + (int)(sim.NextCombatRandom() % 5));
            FireHitscan(sim, actor, 2048, spread, pitch, damage);
        }
    }

    internal static void FireHitscan(AuthoritySimulation sim, Actor source, double range, double spread, double pitch, int damage)
    {
        var hit = CombatTrace.TraceLineAttack(sim, source,
            BamAngle.FromDegrees(source.Angle.ToDegrees() + spread), BamAngle.FromDegrees(pitch), range);
        if (hit.Victim is { } victim) ActorDamage.Apply(victim, damage, source, inflictor: source);
        else GeometryLineAttack.Apply(sim, hit, damage);
    }

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
        _raiseTics = actor.RaiseDuration;
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
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var radians = Math.Atan2(dy, dx);
        actor.Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        actor.VelocityX = Fixed.FromDouble(skullSpeed * Math.Cos(radians));
        actor.VelocityY = Fixed.FromDouble(skullSpeed * Math.Sin(radians));
        var travel = Math.Max(1, Math.Sqrt(dx * dx + dy * dy) / skullSpeed);
        actor.VelocityZ = Fixed.FromDouble((target.Z.ToDouble() + target.Height.ToDouble() / 2 - actor.Z.ToDouble()) / travel);
    }

    /// <summary>Native P_CheckMeleeRange distance, vertical, friendship and sight gates.</summary>
    internal static bool CheckMeleeRange(Actor actor, Actor target, bool visible) =>
        Distance(actor, target) < actor.MeleeRange.ToDouble() + target.Radius.ToDouble()
        && target.Z.ToDouble() <= actor.Z.ToDouble() + actor.Height.ToDouble()
        && target.Z.ToDouble() + target.Height.ToDouble() >= actor.Z.ToDouble()
        && !actor.IsFriend(target) && visible;

    private static double Distance(Actor a, Actor b) => Math.Sqrt(
        Math.Pow(a.X.ToDouble() - b.X.ToDouble(), 2) + Math.Pow(a.Y.ToDouble() - b.Y.ToDouble(), 2));
}
