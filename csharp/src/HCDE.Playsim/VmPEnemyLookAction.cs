namespace HCDE.Playsim;

/// <summary><c>LookForEnemiesEx</c>, <c>A_Look</c>, and <c>A_Wander</c> from <c>p_enemy.cpp</c> (1451–1478, 1944–2054, 2283–2322).</summary>
public static class VmPEnemyLookAction
{
    public const int Mf5Inconversation = 1;
    public const int Mf5Chasegoal = 1 << 1;
    public const int ThingSetGoal = 14;
    public const int Ticrate = 35;
    public const int CompatfSoundtarget = 1;
    public const int MfNosector = 1;
    public const int CfNotarget = 1;
    public const int MfFriendly = 1;
    public const int MfStealth = 1;
    public const int MfShootable = 1;
    public const int MfAmbush = 1;
    public const int Mf4Lookallaround = 1;
    public const int Mf2Boss = 1;
    public const int Mf8Fullvolsee = 1;
    public const int Mf4Incombat = 1;
    public const int Mf4Standstill = 1 << 1;
    public const int ChfNodirectionturn = 64;
    public const int ChfNorandomturn = 32;
    public const int ChfStopifblocked = 256;
    public const int DiNodir = 8;

    public sealed class LookActor
    {
        public int Flags5 { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags4 { get; set; }
        public int Flags8 { get; set; }
        public int Special { get; set; }
        public int[] Args { get; } = new int[4];
        public object? Goal { get; set; }
        public LookActor? Target { get; set; }
        public LookActor? LastHeard { get; set; }
        public int TidToHate { get; set; }
        public int Threshold { get; set; }
        public int Reactiontime { get; set; }
        public int Maptime { get; set; }
        public int Visdir { get; set; }
        public int Movedir { get; set; }
        public int Movecount { get; set; }
        public double Yaw { get; set; }
        public object? SeeState { get; set; }
        public string SeeSound { get; set; } = "";
        public LookActor? SoundTarget { get; set; }
        public int CompatFlags { get; set; }
        public object? Player { get; set; }
        public int Health { get; set; }
        public int PlayerCheats { get; set; }
    }

    public sealed class LookForEnemiesExContext
    {
        public Func<double, IEnumerable<LookActor>>? BlockThingsIterator { get; init; }
        public Func<LookActor, LookActor, bool>? ValidEnemyInBlock { get; init; }
        public Func<LookActor, bool, object?, bool>? IsTargetablePlayer { get; init; }
    }

    public sealed class LookContext
    {
        public Func<int, int, IEnumerable<LookActor>>? PatrolIterator { get; init; }
        public Func<LookActor, bool, object?, bool>? LookForPlayers { get; init; }
        public Func<LookActor, LookActor, bool>? CheckSight { get; init; }
        public Func<LookActor, LookActor, bool>? IsFriend { get; init; }
        public Action<LookActor, object?>? SetState { get; init; }
        public Action<LookActor, int, string, int>? PlaySound { get; init; }
        public Action<LookActor, int>? Wander { get; init; }
    }

    public sealed class WanderContext
    {
        public Func<LookActor, bool>? SmartMove { get; init; }
        public Action<LookActor>? RandomChaseDir { get; init; }
    }

    public static int LookForEnemiesEx(
        LookForEnemiesExContext ctx,
        LookActor self,
        List<LookActor> targets,
        double range,
        bool noPlayers,
        bool allaround)
    {
        if (range < 0)
            range = 20 * 128;

        foreach (var thing in ctx.BlockThingsIterator?.Invoke(range) ?? Array.Empty<LookActor>())
        {
            if ((thing.Player == null && ctx.ValidEnemyInBlock?.Invoke(self, thing) == true) ||
                (!noPlayers && thing.Player != null && ctx.IsTargetablePlayer?.Invoke(thing, allaround, null) == true))
                targets.Add(thing);
        }

        return targets.Count;
    }

    public static void Look(LookActor self, LookContext ctx)
    {
        if ((self.Flags5 & Mf5Inconversation) != 0)
            return;

        if (self.Special == ThingSetGoal && self.Args[0] == 0)
        {
            var goal = ctx.PatrolIterator?.Invoke(0, self.Args[1]).FirstOrDefault();
            self.Special = 0;
            self.Goal = goal;
            self.Reactiontime = self.Args[2] * Ticrate + self.Maptime;
            if (self.Args[3] == 0)
                self.Flags5 &= ~Mf5Chasegoal;
            else
                self.Flags5 |= Mf5Chasegoal;
        }

        self.Threshold = 0;
        LookActor? targ;
        if (self.TidToHate != 0)
            targ = self.Target;
        else
        {
            var useSector = (self.CompatFlags & CompatfSoundtarget) != 0 || (self.Flags & MfNosector) != 0;
            targ = useSector ? self.SoundTarget : self.LastHeard;
            if (targ != null && targ.Health <= 0)
                targ = null;
            if (targ != null && targ.Player != null && ((targ.PlayerCheats & CfNotarget) != 0 || (targ.Flags & MfFriendly) == 0))
                return;
        }

        if ((self.Flags & MfStealth) != 0)
            self.Visdir = -1;

        if (targ != null && (targ.Flags & MfShootable) != 0)
        {
            if (ctx.IsFriend?.Invoke(self, targ) == true)
            {
                if (ctx.LookForPlayers?.Invoke(self, (self.Flags4 & Mf4Lookallaround) != 0, null) == true)
                    goto seeyou;
                if (self.SeeState != null)
                    ctx.SetState?.Invoke(self, self.SeeState);
                else
                    ctx.Wander?.Invoke(self, 0);
            }
            else
            {
                self.Target = targ;
                if ((self.Flags & MfAmbush) != 0)
                {
                    if (ctx.CheckSight?.Invoke(self, self.Target!) == true)
                        goto seeyou;
                }
                else
                    goto seeyou;
            }
        }

        if (ctx.LookForPlayers?.Invoke(self, (self.Flags4 & Mf4Lookallaround) != 0, null) != true)
            return;

        seeyou:
        if (self.Target == self.Goal)
        {
            if (self.Reactiontime > self.Maptime)
                self.Target = null;
        }
        else if (self.SeeSound.Length > 0)
        {
            var atten = ((self.Flags2 & Mf2Boss) != 0 || (self.Flags8 & Mf8Fullvolsee) != 0) ? 0 : 1;
            ctx.PlaySound?.Invoke(self, 0, self.SeeSound, atten);
        }

        if (self.Target != null && self.SeeState != null)
            ctx.SetState?.Invoke(self, self.SeeState);
    }

    public static void Wander(LookActor self, int flags, WanderContext ctx)
    {
        self.Flags4 &= ~Mf4Incombat;
        if ((self.Flags5 & Mf5Inconversation) != 0)
            return;
        if ((self.Flags4 & Mf4Standstill) != 0)
            return;
        if (self.Reactiontime != 0)
        {
            self.Reactiontime--;
            return;
        }

        if ((flags & ChfNodirectionturn) == 0 && self.Movedir < DiNodir)
        {
            self.Yaw = Math.Floor(self.Yaw / 45) * 45;
            var delta = self.Movedir * 45 - self.Yaw;
            if (delta < 0)
                self.Yaw -= 45;
            else if (delta > 0)
                self.Yaw += 45;
        }

        if ((--self.Movecount < 0 && (flags & ChfNorandomturn) == 0) ||
            (ctx.SmartMove?.Invoke(self) == false && (flags & ChfStopifblocked) == 0))
        {
            ctx.RandomChaseDir?.Invoke(self);
            self.Movecount += 5;
        }
    }
}
