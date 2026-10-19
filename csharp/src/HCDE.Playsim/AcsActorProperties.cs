namespace HCDE.Playsim;

/// <summary>Native <c>APROP_*</c> subset for ACS get/set actor property.</summary>
internal static class AcsActorProperties
{
    public const int Health = 0;
    public const int Speed = 1;
    public const int Damage = 2;
    public const int Alpha = 3;
    public const int RenderStyle = 4;
    public const int Ambush = 10;
    public const int Invulnerable = 11;
    public const int JumpZ = 12;
    public const int ChaseGoal = 13;
    public const int Gravity = 15;
    public const int Friendly = 16;
    public const int SpawnHealth = 17;
    public const int Dropped = 18;
    public const int NoTarget = 19;
    public const int Score = 22;
    public const int NoTrigger = 23;
    public const int DamageFactor = 24;
    public const int MasterTid = 25;
    public const int TargetTid = 26;
    public const int TracerTid = 27;
    public const int ScaleX = 29;
    public const int ScaleY = 30;
    public const int Dormant = 31;
    public const int Mass = 32;
    public const int Accuracy = 33;
    public const int Stamina = 34;
    public const int Height = 35;
    public const int Radius = 36;
    public const int ReactionTime = 37;
    public const int MeleeRange = 38;
    public const int ViewHeight = 39;
    public const int AttackZOffset = 40;
    public const int Friction = 42;
    public const int DamageMultiplier = 43;
    public const int MaxStepHeight = 44;
    public const int MaxDropOffHeight = 45;
    public const int FriendlySeeBlocks = 48;

    public static void Set(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
    {
        if (tid == 0)
        {
            ApplySet(activator, property, value);
            return;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            ApplySet(actor, property, value);
    }

    public static int Get(AuthoritySimulation sim, Actor? activator, int tid, int property)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        return actor is null || actor.Destroyed ? 0 : Read(actor, property);
    }

    /// <summary>Native <c>CheckActorProperty</c> for integer and boolean <c>APROP_*</c> we support.</summary>
    public static bool Check(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed)
            return false;
        // Native CheckActorProperty rejects unknown properties, even when Get returns zero.
        if (property is not (Health or Speed or Damage or Alpha or RenderStyle or Ambush or Invulnerable or JumpZ or Gravity or Friendly
            or SpawnHealth or NoTarget or MasterTid or TargetTid or TracerTid or ScaleX or ScaleY or Dormant or ChaseGoal or Mass or Accuracy or Stamina or Height or Radius or ViewHeight or AttackZOffset
            or MaxStepHeight or MaxDropOffHeight or DamageFactor or DamageMultiplier or MeleeRange or Friction or NoTrigger or Score or Dropped or ReactionTime or FriendlySeeBlocks))
            return false;
        var actual = Read(actor, property);
        return IsBoolean(property) ? actual == (value != 0 ? 1 : 0) : actual == value;
    }

    private static bool IsBoolean(int property) => property is Ambush or Invulnerable or Friendly or NoTarget or NoTrigger or Dropped or ChaseGoal or Dormant;

    private static void ApplySet(Actor? actor, int property, int value)
    {
        if (actor is null || actor.Destroyed)
            return;

        switch (property)
        {
            case Alpha:
                actor.Alpha = value / 65536.0;
                break;
            case RenderStyle:
                if (ActorRenderStyle.TryFromAcs(value, out var style)) actor.RenderStyle = style;
                break;
            case Health:
                if (actor.Health <= 0)
                    return;
                actor.Health = value;
                break;
            case Ambush:
                actor.Ambush = value != 0;
                break;
            case Speed:
                actor.MovementSpeed = new Fixed(value);
                actor.HasTuningOverride = true;
                break;
            case Damage:
                actor.SetDamage(value);
                break;
            case Invulnerable:
                actor.Invulnerable = value != 0;
                actor.HasDefensePropertyOverride = true;
                break;
            case JumpZ:
                if (actor is PlayerPawn jumpPlayer) jumpPlayer.JumpZ = new Fixed(value);
                break;
            case ChaseGoal:
                actor.ChaseGoal = value != 0;
                break;
            case MasterTid:
                ApplySetMaster(actor, actor.Simulation is { } simulation ? AcsActorTid.SingleFromTid(simulation, value) : null);
                break;
            case FriendlySeeBlocks:
                actor.FriendlySeeBlocks = value;
                break;
            case ScaleX:
                actor.ScaleX = (float)(value / 65536.0);
                break;
            case ScaleY:
                actor.ScaleY = (float)(value / 65536.0);
                break;
            case Friendly:
                actor.Friendly = value != 0;
                break;
            case Gravity:
                actor.Gravity = new Fixed(value);
                actor.HasGravityOverride = true;
                break;
            case DamageFactor:
                actor.DamageFactor = new Fixed(value);
                break;
            case DamageMultiplier:
                actor.DamageMultiplier = new Fixed(value);
                break;
            case MeleeRange:
                actor.MeleeRange = new Fixed(value);
                break;
            case Friction:
                actor.Friction = new Fixed(value);
                break;
            case ViewHeight:
                if (actor is PlayerPawn viewPlayer)
                {
                    viewPlayer.DefaultViewHeight = new Fixed(value);
                    viewPlayer.ViewHeight = viewPlayer.DefaultViewHeight.ToDouble();
                }
                break;
            case NoTarget:
                actor.NoTarget = value != 0;
                break;
            case NoTrigger:
                actor.NoTrigger = value != 0;
                break;
            case Dropped:
                actor.Dropped = value != 0;
                break;
            case ReactionTime:
                actor.ReactionTime = value;
                break;
            case Score:
                actor.Score = value;
                break;
            case AttackZOffset:
                if (actor is PlayerPawn attackPlayer) attackPlayer.AttackZOffset = new Fixed(value);
                break;
            case SpawnHealth:
                if (actor is PlayerPawn healthPlayer) healthPlayer.MaxHealth = value;
                break;
            case Accuracy:
                actor.Accuracy = value;
                break;
            case Stamina:
                actor.Stamina = value;
                break;
            case Mass:
                actor.Mass = value;
                actor.HasDefensePropertyOverride = true;
                break;
            case MaxStepHeight:
                actor.MaxStepHeight = Fixed.FromDouble(value / 65536.0);
                break;
            case MaxDropOffHeight:
                actor.MaxDropOffHeight = Fixed.FromDouble(value / 65536.0);
                break;
        }
    }

    private static int Read(Actor actor, int property) => property switch
    {
        Health => actor.Health,
        Speed => actor.MovementSpeed.Raw,
        Damage => actor.GetMissileDamage(0, 1),
        Alpha => unchecked((int)Math.Round(actor.Alpha * 65536, MidpointRounding.ToEven)),
        RenderStyle => actor.RenderStyle.ToAcs(),
        Ambush => actor.Ambush ? 1 : 0,
        Invulnerable => actor.Invulnerable ? 1 : 0,
        JumpZ => actor is PlayerPawn jumpPlayer ? jumpPlayer.JumpZ.Raw : 0,
        ChaseGoal => actor.ChaseGoal ? 1 : 0,
        Dormant => actor.Dormant ? 1 : 0,
        ScaleX => unchecked((int)Math.Round(actor.ScaleX * 65536, MidpointRounding.ToEven)),
        ScaleY => unchecked((int)Math.Round(actor.ScaleY * 65536, MidpointRounding.ToEven)),
        Gravity => actor.Gravity.Raw,
        DamageFactor => actor.DamageFactor.Raw,
        DamageMultiplier => actor.DamageMultiplier.Raw,
        Friendly => actor.Friendly ? 1 : 0,
        NoTarget => actor.NoTarget ? 1 : 0,
        NoTrigger => actor.NoTrigger ? 1 : 0,
        Dropped => actor.Dropped ? 1 : 0,
        ReactionTime => actor.ReactionTime,
        Score => actor.Score,
        SpawnHealth => actor.GetMaxHealth(false),
        Mass => actor.Mass,
        Accuracy => actor.Accuracy,
        FriendlySeeBlocks => actor.FriendlySeeBlocks,
        Stamina => actor.Stamina,
        Height => actor.Height.Raw,
        Radius => actor.Radius.Raw,
        MeleeRange => actor.MeleeRange.Raw,
        Friction => actor.Friction.Raw,
        ViewHeight => actor is PlayerPawn viewPlayer ? viewPlayer.DefaultViewHeight.Raw : 0,
        AttackZOffset => actor is PlayerPawn attackPlayer ? attackPlayer.AttackZOffset.Raw : 0,
        MaxStepHeight => actor.MaxStepHeight.Raw,
        MaxDropOffHeight => actor.MaxDropOffHeight.Raw,
        TargetTid => TargetThingId(actor),
        MasterTid => MasterThingId(actor),
        TracerTid => ReferencedThingId(actor, actor.TracerId),
        _ => 0,
    };

    private static int TargetThingId(Actor actor)
    {
        if (actor is ProjectileActor projectile)
            return projectile.Owner is { Destroyed: false } owner ? owner.ThingId : 0;
        return ReferencedThingId(actor, actor.TargetId);
    }

    private static int MasterThingId(Actor actor)
    {
        if (actor.MasterId is not null) return ReferencedThingId(actor, actor.MasterId);
        if (actor.FriendPlayer == 0) return 0;
        return actor.Simulation?.Players.FirstOrDefault(player => !player.Destroyed
            && player.PlayerNum == actor.FriendPlayer - 1)?.ThingId ?? 0;
    }

    public static int SetMaster(Actor actor, Actor? master)
    {
        if (!actor.IsMonster)
            return 0;
        ApplySetMaster(actor, master);
        return 1;
    }

    private static void ApplySetMaster(Actor actor, Actor? master)
    {
        if (!actor.IsMonster) return;
        if (master is not null && !master.IsMonster && master is not PlayerPawn) return;
        if (master is { IsMonster: true })
        {
            actor.MasterId = master.Id;
            actor.FriendPlayer = 0;
            ActorPropertyActions.SetFriendly(actor, master.Friendly);
        }
        else if (master is PlayerPawn player)
        {
            actor.MasterId = null;
            actor.FriendPlayer = player.PlayerNum + 1;
            ActorPropertyActions.SetFriendly(actor, true);
            var attacker = actor.Simulation?.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == player.LastDamageSourceId);
            if (attacker is not null && (!attacker.Friendly
                || actor.Simulation?.GameMode == SpawnGameMode.Deathmatch && attacker.FriendPlayer != 0
                && attacker.FriendPlayer != actor.FriendPlayer))
            {
                actor.SetTargetPointer(attacker);
                actor.LastHeardTargetId = attacker.Id;
            }
        }
        else
        {
            actor.MasterId = null;
            actor.FriendPlayer = 0;
            ActorPropertyActions.SetFriendly(actor, actor.DefaultFriendly);
        }
        bool ShouldClear(uint? id)
        {
            if (id is null) return false;
            if (master is not null) return id == master.Id;
            var target = actor.Simulation?.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == id);
            return target is not null && !actor.IsHostile(target);
        }
        if (ShouldClear(actor.TargetId)) actor.SetTargetPointer(null);
        if (actor.Brain is { } brain && ShouldClear(brain.LastEnemyId))
            brain.RestoreTargetMemory(new SimTargetMemory(actor.TargetId, null, actor.LastHeardTargetId));
        if (ShouldClear(actor.LastHeardTargetId)) actor.LastHeardTargetId = null;
    }

    private static int ReferencedThingId(Actor actor, uint? targetId)
    {
        if (targetId is null) return 0;
        var target = actor.Simulation?.Actors.FirstOrDefault(candidate => candidate.Id == targetId && !candidate.Destroyed);
        return target?.ThingId ?? 0;
    }
}
