namespace HCDE.Playsim;

/// <summary>Drop/blend/countdown/burst/respawn actions from <c>p_actionfunctions.cpp</c> (1989–2211).</summary>
public static class VmPActionDropBurstRespawn
{
    public const int RsfFog = 1;
    public const int RsfKeeptarget = 2;
    public const int RsfTelefrag = 4;
    public const int MfMissile = 1;
    public const int MfShootable = 2;
    public const int Mf4Bossdeath = 4;

    public static void DropInventory(Func<object?>? findInventory, Action<object?, int>? drop, int amount)
    {
        var inv = findInventory?.Invoke();
        if (inv != null)
            drop?.Invoke(inv, amount);
    }

    public static void SetBlend(Action<double, double, double, double, double, double, double, double>? createFader,
        int colorA, int colorB, double alpha, double alpha2, int tics)
    {
        if (colorA == unchecked((int)0xFFFFFFFF)) colorA = 0;
        if (colorB == unchecked((int)0xFFFFFFFF)) colorB = 0;
        createFader?.Invoke(0, 0, 0, alpha, 0, 0, 0, alpha2);
    }

    public sealed class CountdownActor
    {
        public int[] Args { get; } = new int[5];
        public int Flags { get; set; }
        public object? DeathState { get; set; }
    }

    public sealed class CountdownContext
    {
        public Action? ExplodeMissile { get; init; }
        public Action? ForcedDamage { get; init; }
        public Action<object?>? SetState { get; init; }
        public Func<object?>? FindDeathState { get; init; }
    }

    public static void CountdownArg(CountdownContext ctx, CountdownActor actor, int cnt, object? state)
    {
        if (cnt < 0 || cnt >= 5)
            return;
        if (actor.Args[cnt]-- != 0)
            return;
        if ((actor.Flags & MfMissile) != 0)
            ctx.ExplodeMissile?.Invoke();
        else if ((actor.Flags & MfShootable) != 0)
            ctx.ForcedDamage?.Invoke();
        else
            ctx.SetState?.Invoke(state ?? actor.DeathState ?? ctx.FindDeathState?.Invoke());
    }

    public sealed class BurstContext
    {
        public double Radius { get; init; }
        public double Height { get; init; }
        public Func<int, int>? BurstRandom { get; init; }
        public Func<double, double, double, object?>? SpawnChunk { get; init; }
        public Action<object?>? CopyFriendliness { get; init; }
        public Action? BossDeath { get; init; }
        public Action? Unblock { get; init; }
        public Action? DestroySelf { get; init; }
        public int Flags4 { get; init; }
    }

    public static int Burst(BurstContext ctx)
    {
        var numChunks = Math.Max(4, (int)(ctx.Radius * ctx.Height / 32));
        var extra = ctx.BurstRandom?.Invoke(numChunks / 4) ?? 0;
        var total = Math.Max(24, numChunks + extra);
        var spawned = 0;
        for (var i = total; i >= 0; i--)
        {
            var mo = ctx.SpawnChunk?.Invoke(0, 0, 0);
            if (mo == null)
                continue;
            spawned++;
            ctx.CopyFriendliness?.Invoke(mo);
        }

        if ((ctx.Flags4 & Mf4Bossdeath) != 0)
            ctx.BossDeath?.Invoke();
        ctx.Unblock?.Invoke();
        ctx.DestroySelf?.Invoke();
        return spawned;
    }

    public sealed class RespawnActor
    {
        public int Health { get; set; }
        public int DefaultHealth { get; init; }
        public int Flags { get; set; }
        public object? Target { get; set; }
        public object? SpawnState { get; init; }
        public bool CountsAsKill { get; init; }
    }

    public sealed class RespawnContext
    {
        public Func<bool>? CheckPosition { get; init; }
        public Func<bool>? TeleportMove { get; init; }
        public Action? RestoreSpecialPosition { get; init; }
        public Action<RespawnActor>? CopyDefaults { get; init; }
        public Action<RespawnActor, object?>? SetState { get; init; }
        public Action? SpawnFog { get; init; }
        public Action? IncrementMonsters { get; init; }
    }

    public static bool Respawn(RespawnContext ctx, RespawnActor self, int flags)
    {
        ctx.RestoreSpecialPosition?.Invoke();
        var ok = (flags & RsfTelefrag) != 0
            ? ctx.TeleportMove?.Invoke() == true
            : ctx.CheckPosition?.Invoke() == true;
        if (!ok)
        {
            self.Flags &= ~1;
            return false;
        }

        self.Health = self.DefaultHealth;
        if ((flags & RsfKeeptarget) == 0)
            self.Target = null;
        ctx.CopyDefaults?.Invoke(self);
        ctx.SetState?.Invoke(self, self.SpawnState);
        if ((flags & RsfFog) != 0)
            ctx.SpawnFog?.Invoke();
        if (self.CountsAsKill)
            ctx.IncrementMonsters?.Invoke();
        return true;
    }
}
