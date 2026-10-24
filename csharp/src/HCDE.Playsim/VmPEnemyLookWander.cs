namespace HCDE.Playsim;

/// <summary><c>LookForEnemiesEx</c>, <c>A_Look</c>, and <c>A_Wander</c> from <c>p_enemy.cpp</c> (1451–1478, 1944–2054, 2283–2322).</summary>
public static class VmPEnemyLookWander
{
    public const int Mapblockunits = 128;
    public const int Mf5Inconversation = 1 << 0;
    public const int Mf5Chasegoal = 1 << 1;
    public const int ThingSetGoal = 14;
    public const int Ticrate = 35;
    public const int CompatfSoundtarget = 1;
    public const int MfNosector = 1;
    public const int CfNotarget = 1;
    public const int MfFriendly = 1;
    public const int MfStealth = 1 << 1;
    public const int MfShootable = 1;
    public const int MfAmbush = 1 << 2;
    public const int Mf4Lookallaround = 1;
    public const int Mf2Boss = 1;
    public const int Mf8Fullvolsee = 1;
    public const int Mf4Incombat = 1;
    public const int Mf4Standstill = 1 << 1;
    public const int ChfNodirectionturn = 64;
    public const int ChfNorandomturn = 32;
    public const int ChfStopifblocked = 256;
    public const int DiNodir = 8;
    public const int AttnNone = 0;
    public const int AttnNorm = 1;

    public sealed class Monster
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags8 { get; set; }
        public int Special { get; set; }
        public int[] Args { get; } = new int[5];
        public int CompatFlags { get; set; }
        public int Maptime { get; set; }
        public int Reactiontime { get; set; }
        public int Threshold { get; set; }
        public int TidToHate { get; set; }
        public int Visdir { get; set; }
        public int Movedir { get; set; }
        public int Movecount { get; set; }
        public double YawDegrees { get; set; }
        public int Friendlyseeblocks { get; set; } = 20;
        public object? Goal { get; set; }
        public Monster? Target { get; set; }
        public Monster? LastHeard { get; set; }
        public Monster? SoundTarget { get; set; }
        public object? Player { get; set; }
        public int PlayerCheats { get; set; }
        public int Health { get; set; }
        public object? SeeState { get; set; }
        public string SeeSound { get; set; } = "";
    }

    public sealed class LookExParams
    {
        public double MaxDist { get; set; }
        public double MinDist { get; set; }
    }

    public sealed class EnemiesExContext
    {
        public Func<Monster, double, IEnumerable<Monster>>? ThingsInRange { get; init; }
        public Func<Monster, Monster, LookExParams?, bool>? ValidEnemy { get; init; }
        public Func<Monster, bool, LookExParams?, bool>? TargetablePlayer { get; init; }
    }

    public sealed class LookContext
    {
        public Func<int, int, Monster?>? NextPatrolPoint { get; init; }
        public Func<Monster, bool, LookExParams?, bool>? LookForPlayers { get; init; }
        public Func<Monster, Monster, bool>? CheckSight { get; init; }
        public Func<Monster, Monster, bool>? IsFriend { get; init; }
        public Action<Monster, object?>? SetState { get; init; }
        public Action<Monster, int, string, int>? PlaySeeSound { get; init; }
        public Action<Monster, int>? CallWander { get; init; }
    }

    public sealed class WanderContext
    {
        public Func<Monster, bool>? SmartMove { get; init; }
        public Action<Monster>? NewChaseDir { get; init; }
    }

    public static int LookForEnemiesEx(
        EnemiesExContext ctx,
        Monster self,
        List<Monster> outTargets,
        double range,
        bool noPlayers,
        bool allAround,
        LookExParams? parms)
    {
        if (range < 0)
            range = self.Friendlyseeblocks * Mapblockunits;

        foreach (var thing in ctx.ThingsInRange?.Invoke(self, range) ?? Array.Empty<Monster>())
        {
            var isPlayer = thing.Player != null;
            if (!isPlayer && ctx.ValidEnemy?.Invoke(self, thing, parms) == true)
                outTargets.Add(thing);
            else if (!noPlayers && isPlayer && ctx.TargetablePlayer?.Invoke(thing, allAround, parms) == true)
                outTargets.Add(thing);
        }

        return outTargets.Count;
    }

    public static void Look(Monster self, LookContext ctx)
    {
        if ((self.Flags5 & Mf5Inconversation) != 0)
            return;

        if (self.Special == ThingSetGoal && self.Args[0] == 0)
        {
            self.Goal = ctx.NextPatrolPoint?.Invoke(0, self.Args[1]);
            self.Special = 0;
            self.Reactiontime = self.Args[2] * Ticrate + self.Maptime;
            if (self.Args[3] == 0)
                self.Flags5 &= ~Mf5Chasegoal;
            else
                self.Flags5 |= Mf5Chasegoal;
        }

        self.Threshold = 0;
        Monster? heard;
        if (self.TidToHate != 0)
        {
            heard = self.Target;
        }
        else
        {
            var sectorSound = (self.CompatFlags & CompatfSoundtarget) != 0 || (self.Flags & MfNosector) != 0;
            heard = sectorSound ? self.SoundTarget : self.LastHeard;
            if (heard != null && heard.Health <= 0)
                heard = null;
            if (heard != null && heard.Player != null &&
                ((heard.PlayerCheats & CfNotarget) != 0 || (heard.Flags & MfFriendly) == 0))
                return;
        }

        if ((self.Flags & MfStealth) != 0)
            self.Visdir = -1;

        if (heard != null && (heard.Flags & MfShootable) != 0)
        {
            if (ctx.IsFriend != null && ctx.IsFriend(self, heard))
            {
                if (ctx.LookForPlayers?.Invoke(self, (self.Flags4 & Mf4Lookallaround) != 0, null) == true)
                    goto SeeYou;
                if (self.SeeState != null)
                    ctx.SetState?.Invoke(self, self.SeeState);
                else
                    ctx.CallWander?.Invoke(self, 0);
                return;
            }

            self.Target = heard;
            if ((self.Flags & MfAmbush) != 0)
            {
                if (ctx.CheckSight?.Invoke(self, self.Target!) == true)
                    goto SeeYou;
            }
            else
                goto SeeYou;
        }

        if (ctx.LookForPlayers?.Invoke(self, (self.Flags4 & Mf4Lookallaround) != 0, null) != true)
            return;

        SeeYou:
        if (ReferenceEquals(self.Target, self.Goal))
        {
            if (self.Reactiontime > self.Maptime)
                self.Target = null;
        }
        else if (self.SeeSound.Length > 0)
        {
            var atten = ((self.Flags2 & Mf2Boss) != 0 || (self.Flags8 & Mf8Fullvolsee) != 0)
                ? AttnNone
                : AttnNorm;
            ctx.PlaySeeSound?.Invoke(self, 0, self.SeeSound, atten);
        }

        if (self.Target != null && self.SeeState != null)
            ctx.SetState?.Invoke(self, self.SeeState);
    }

    public static void Wander(Monster self, int chaseFlags, WanderContext ctx)
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

        if ((chaseFlags & ChfNodirectionturn) == 0 && self.Movedir < DiNodir)
        {
            self.YawDegrees = Math.Floor(self.YawDegrees / 45.0) * 45.0;
            var delta = self.Movedir * 45.0 - self.YawDegrees;
            if (delta < 0)
                self.YawDegrees -= 45;
            else if (delta > 0)
                self.YawDegrees += 45;
        }

        var blocked = ctx.SmartMove?.Invoke(self) == false;
        if ((--self.Movecount < 0 && (chaseFlags & ChfNorandomturn) == 0) ||
            (blocked && (chaseFlags & ChfStopifblocked) == 0))
        {
            ctx.NewChaseDir?.Invoke(self);
            self.Movecount += 5;
        }
    }
}
