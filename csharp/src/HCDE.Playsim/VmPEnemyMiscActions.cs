namespace HCDE.Playsim;

/// <summary>Misc monster actions from <c>p_enemy.cpp</c> (2255–2260, 2330–2381, 3155–3196, 3217–3269, 3476–3488).</summary>
public static class VmPEnemyMiscActions
{
    public const int Mf5Inconversation = 1;
    public const int MfShootable = 1;
    public const int MfAmbush = 4;
    public const int MfFriendly = 8;
    public const int MfStealth = 2;
    public const int Mf4Lookallaround = 1;
    public const int Mf4Standstill = 2;
    public const int Mf4Incombat = 4;
    public const int LevelNoallies = 1;

    public sealed class Actor
    {
        public int Flags { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int LevelFlags { get; set; }
        public int Threshold { get; set; }
        public int Health { get; set; }
        public int Visdir { get; set; }
        public bool Special1 { get; set; }
        public object? LastHeard { get; set; }
        public object? Target { get; set; }
        public object? SeeState { get; set; }
        public object? SpawnState { get; set; }
        public string PainSound { get; set; } = "";
        public bool IsPlayer { get; set; }
        public int PlayerHealth { get; set; }
        public string LastDamageType { get; set; } = "";
        public object? PlayerAttacker { get; set; }
        public object? Alternative { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
    }

    public sealed class Look2Context
    {
        public Func<Actor, Actor, bool>? CheckSight { get; init; }
        public Func<Actor, bool, object?, bool>? LookForPlayers { get; init; }
        public Action<Actor, object?>? SetState { get; init; }
        public Func<int, int>? RandomLook2 { get; init; }
    }

    public sealed class MonsterRailContext
    {
        public Func<Actor, double, object?, object?>? AimLineAttack { get; init; }
        public Action<Actor>? ShadowHandling { get; init; }
        public Action<Actor, int>? RailAttack { get; init; }
        public Func<Actor, Actor, double>? AngleTo { get; init; }
        public Func<Actor, Actor, double, double, double>? AngleToWithVel { get; init; }
        public Func<double, double, double>? VecToAngle { get; init; }
        public Func<Actor, int, int, int>? MissileDamage { get; init; }
    }

    public sealed class PainContext
    {
        public Func<string, int>? FindSound { get; init; }
        public Action<Actor, int, int, int, int>? PlaySound { get; init; }
        public Action<Actor, object?>? PlayerHurtRumble { get; init; }
    }

    public static void ClearLastHeard(Actor self) => self.LastHeard = null;

    public static void Look2(Actor self, Look2Context ctx)
    {
        if ((self.Flags5 & Mf5Inconversation) != 0)
            return;
        self.Threshold = 0;
        var targ = self.LastHeard as Actor;
        if (targ != null && targ.Health <= 0)
            targ = null;
        if (targ != null && (targ.Flags & MfShootable) != 0)
        {
            var hostile = (self.LevelFlags & LevelNoallies) != 0 ||
                (self.Flags & MfFriendly) != (targ.Flags & MfFriendly);
            if (hostile)
            {
                if ((self.Flags & MfAmbush) != 0 &&
                    ctx.CheckSight?.Invoke(self, targ) != true)
                    goto NoSee;
                self.Target = targ;
                self.Threshold = 10;
                if (self.SeeState != null)
                    ctx.SetState?.Invoke(self, self.SeeState);
                return;
            }
            if (ctx.LookForPlayers?.Invoke(self, (self.Flags4 & Mf4Lookallaround) != 0, null) != true)
                goto NoSee;
            if (self.SeeState != null)
                ctx.SetState?.Invoke(self, self.SeeState);
            self.Flags4 |= Mf4Incombat;
            return;
        }
        NoSee:
        var r = ctx.RandomLook2?.Invoke(255) ?? 255;
        if (r < 30 && self.SpawnState != null)
            ctx.SetState?.Invoke(self, self.SpawnState);
        if ((self.Flags4 & Mf4Standstill) == 0 && (ctx.RandomLook2?.Invoke(255) ?? 255) < 40 && self.SpawnState != null)
            ctx.SetState?.Invoke(self, self.SpawnState);
    }

    public static void MonsterRail(Actor self, MonsterRailContext ctx)
    {
        var target = self.Target as Actor;
        if (target == null)
            return;
        var savedPitch = self.Pitch;
        if ((self.Flags & MfStealth) != 0)
            self.Visdir = 1;
        self.Flags &= ~MfAmbush;
        self.Yaw = ctx.AngleTo?.Invoke(self, target) ?? 0;
        self.Pitch = ctx.AimLineAttack?.Invoke(self, self.Yaw, null) is double p ? p : self.Pitch;
        self.Yaw = ctx.AngleToWithVel?.Invoke(self, target, -3, -3) ?? self.Yaw;
        ctx.ShadowHandling?.Invoke(self);
        ctx.RailAttack?.Invoke(self, ctx.MissileDamage?.Invoke(self, 0, 1) ?? 0);
        self.Pitch = savedPitch;
    }

    public static void Pain(Actor self, PainContext ctx)
    {
        if (self.IsPlayer && self.Alternative == null)
        {
            string amount = self.PlayerHealth < 25 ? "*pain25" :
                self.PlayerHealth < 50 ? "*pain50" :
                self.PlayerHealth < 75 ? "*pain75" : "*pain100";
            var sfx = ctx.FindSound?.Invoke(amount) ?? -1;
            ctx.PlayerHurtRumble?.Invoke(self, self.PlayerAttacker);
            if (sfx >= 0)
                ctx.PlaySound?.Invoke(self, 0, 1, sfx, 1);
        }
        else if (self.PainSound.Length > 0)
            ctx.PlaySound?.Invoke(self, 0, 0, 0, 1);
    }

    public static void ClassBossHealth(Actor self, bool multiplayer, bool deathmatch)
    {
        if (multiplayer && !deathmatch && !self.Special1)
        {
            self.Health *= 5;
            self.Special1 = true;
        }
    }
}
