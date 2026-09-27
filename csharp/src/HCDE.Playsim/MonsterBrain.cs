namespace HCDE.Playsim;

public enum MonsterAttack { Melee, Hitscan, Fireball }
public enum MonsterMode { Idle, Chase, Windup, Recovery, Pain, Dead }

/// <summary>Deterministic managed look/chase/attack loop; specialized native actions remain separate.</summary>
public sealed class MonsterBrain(MonsterAttack attack)
{
    public bool Enabled { get; set; } = true;
    public MonsterAttack Attack { get; } = attack;
    public MonsterMode Mode { get; private set; }
    public uint? TargetId { get; private set; }
    public int ReactionTics { get; private set; } = 10;
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
            foreach (var value in new[] { (uint)Attack, (uint)Mode, TargetId ?? 0, (uint)ReactionTics,
                (uint)AttackCooldown, (uint)WindupTics, (uint)_blockedTics, (uint)_previousX.Raw,
                (uint)_previousY.Raw, (uint)Fixed.FromDouble(_lastX).Raw, (uint)Fixed.FromDouble(_lastY).Raw, Enabled ? 1u : 0u })
                hash = unchecked((hash ^ value) * 16777619u);
            return hash;
        }
    }

    public static MonsterBrain? ForType(int type) => type switch
    {
        3004 or 9 or 65 or 84 => new(MonsterAttack.Hitscan),
        3001 or 3003 or 3005 or 69 => new(MonsterAttack.Fireball),
        3002 or 58 => new(MonsterAttack.Melee),
        _ => null,
    };

    public void Tick(AuthoritySimulation sim, Actor actor)
    {
        if (!Enabled) return;
        actor.VelocityX = actor.VelocityY = default;
        if (actor.IsDead || actor.Destroyed)
        {
            Mode = MonsterMode.Dead; TargetId = null; WindupTics = 0; return;
        }
        if (AttackCooldown > 0) AttackCooldown--;
        if (ReactionTics > 0) { ReactionTics--; return; }
        if (actor.States.Current == actor.PainState)
        {
            Mode = MonsterMode.Pain; WindupTics = 0; return;
        }
        var target = sim.Actors.FirstOrDefault(a => a.Id == TargetId && a.CanTakeDamage);
        var attacker = sim.Actors.FirstOrDefault(a => a.Id == actor.LastDamageSourceId && a.CanTakeDamage && a.Id != actor.Id);
        if (attacker != null && attacker is not ProjectileActor) target = attacker;
        target ??= sim.Players.Where(p => p.CanTakeDamage && Distance(actor, p) <= 2048 && CombatTrace.HasLineOfSight(sim, actor, p))
            .OrderBy(p => Distance(actor, p)).ThenBy(p => p.Id).FirstOrDefault();
        if (target == null)
        {
            Mode = MonsterMode.Idle; TargetId = null; WindupTics = 0; return;
        }
        if (TargetId != target.Id) WindupTics = 0;
        TargetId = target.Id;
        var visible = CombatTrace.HasLineOfSight(sim, actor, target);
        if (visible) { _lastX = target.X.ToDouble(); _lastY = target.Y.ToDouble(); }
        var dx = _lastX - actor.X.ToDouble();
        var dy = _lastY - actor.Y.ToDouble();
        actor.Angle = BamAngle.FromDegrees(Math.Atan2(dy, dx) * 180 / Math.PI);
        var melee = Distance(actor, target) <= actor.Radius.ToDouble() + target.Radius.ToDouble() + 24
            && actor.Z.ToDouble() < target.Z.ToDouble() + target.Height.ToDouble()
            && target.Z.ToDouble() < actor.Z.ToDouble() + actor.Height.ToDouble();
        var canAttack = visible && (melee || Attack != MonsterAttack.Melee && Distance(actor, target) <= 1024);
        if (WindupTics > 0)
        {
            if (!canAttack) WindupTics = 0;
            else if (--WindupTics == 0)
            {
                if (melee) ActorDamage.Apply(target, 10, actor);
                else if (Attack == MonsterAttack.Fireball) sim.SpawnProjectile(actor, ProjectileKind.ImpBall, target);
                else
                {
                    var hit = CombatTrace.FindTarget(sim, actor, 1024, sim.NextCombatSpread() * 5.625);
                    if (hit != null) ActorDamage.Apply(hit, 5, actor);
                }
                AttackCooldown = 25;
                Mode = MonsterMode.Recovery;
            }
            return;
        }
        if (canAttack && AttackCooldown == 0)
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

    private static double Distance(Actor a, Actor b) => Math.Sqrt(
        Math.Pow(a.X.ToDouble() - b.X.ToDouble(), 2) + Math.Pow(a.Y.ToDouble() - b.Y.ToDouble(), 2));
}
