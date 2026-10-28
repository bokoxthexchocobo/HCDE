namespace HCDE.Playsim;

/// <summary>
/// `DoSectorDamage`, `P_SectorDamage`, `P_GiveSecret`, and `P_ActorOnSpecialFlat` from <c>p_spec.cpp</c> (509–677, 629–637).
/// </summary>
public static class VmPSpecSectorDamageRun
{
    public const int MfShootable = 1;
    public const int DamageNonplayers = 1;
    public const int DamagePlayers = 2;
    public const int DamageInAir = 4;
    public const int DamageNoArmor = 8;
    public const int DamageSubclassesProtect = 16;

    public sealed class Actor
    {
        public int Flags { get; set; }
        public object? Player { get; set; }
        public double Top { get; set; }
        public Func<double, bool>? IsAtZ { get; set; }
        public Func<double, bool>? IsAbove { get; set; }
        public int WaterLevel { get; set; }
        public object? Next { get; set; }
        public IEnumerable<object>? Inventory { get; set; }
    }

    public sealed class Sector
    {
        public int LastDamage { get; set; }
        public Func<Actor, double>? FloorZ { get; set; }
        public Func<Actor, double>? CeilingZ { get; set; }
        public List<Actor>? Things { get; set; }
        public List<Sector>? Attached { get; set; }
    }

    public sealed class DamageContext
    {
        public Action<Actor, int, object?, int>? Damage { get; init; }
        public Func<object?, bool, bool>? FindInventory { get; init; }
        public object? ProtectClass { get; init; }
        public int MapTime { get; init; }
    }

    public static void DoSectorDamage(Actor actor, Sector? sec, int amount, object? type, DamageContext ctx, int flags)
    {
        if ((actor.Flags & MfShootable) == 0)
            return;
        if ((flags & DamageNonplayers) == 0 && actor.Player == null)
            return;
        if ((flags & DamagePlayers) == 0 && actor.Player != null)
            return;
        if ((flags & DamageInAir) == 0 && sec != null &&
            actor.IsAtZ?.Invoke(sec.FloorZ?.Invoke(actor) ?? 0) != true && actor.WaterLevel == 0)
            return;
        if (ctx.ProtectClass != null && ctx.FindInventory?.Invoke(ctx.ProtectClass, (flags & DamageSubclassesProtect) != 0) == true)
            return;
        var dflags = (flags & DamageNoArmor) != 0 ? 1 : 0;
        ctx.Damage?.Invoke(actor, amount, type, dflags);
    }

    public static void SectorDamage(IEnumerable<Sector> tagged, int amount, object? type, DamageContext ctx, int flags)
    {
        foreach (var sec in tagged)
        {
            sec.LastDamage = ctx.MapTime;
            if (sec.Things != null)
            {
                foreach (var actor in sec.Things)
                    DoSectorDamage(actor, sec, amount, type, ctx, flags);
            }
            if (sec.Attached == null)
                continue;
            foreach (var sec2 in sec.Attached)
            {
                sec2.LastDamage = ctx.MapTime;
                if (sec2.Things == null)
                    continue;
                foreach (var actor in sec2.Things)
                {
                    var z1 = sec.FloorZ?.Invoke(actor) ?? 0;
                    var z2 = sec.CeilingZ?.Invoke(actor) ?? 0;
                    if (z2 < z1)
                        (z1, z2) = (z2, z1);
                    if (actor.Top <= z1)
                        continue;
                    if ((flags & DamageInAir) != 0 || actor.IsAbove?.Invoke(z2) != true)
                        DoSectorDamage(actor, null, amount, type, ctx, flags | DamageInAir);
                }
            }
        }
    }

    public static int GiveSecret(object? actor, bool printMessage, bool playSound, int sectorNum, GiveSecretContext ctx)
    {
        if (actor == null)
            return 0;
        ctx.IncrementPlayerSecret?.Invoke(actor);
        var retval = 1;
        retval = ctx.OnGiveSecret?.Invoke(actor, printMessage, playSound) ?? retval;
        if (retval != 0 && ctx.ShowSecretMessage && ctx.LocalView?.Invoke(actor) == true)
        {
            if (printMessage)
            {
                ctx.MidPrint?.Invoke();
                if (ctx.ShowSecretSector && sectorNum >= 0)
                    ctx.PrintSector?.Invoke(sectorNum);
            }
            if (playSound)
                ctx.PlaySecretSound?.Invoke();
        }
        ctx.IncrementFoundSecrets?.Invoke();
        return retval;
    }

    public sealed class GiveSecretContext
    {
        public Action<object?>? IncrementPlayerSecret { get; init; }
        public Func<object?, bool, bool, int>? OnGiveSecret { get; init; }
        public bool ShowSecretMessage { get; init; }
        public Func<object?, bool>? LocalView { get; init; }
        public Action? MidPrint { get; init; }
        public bool ShowSecretSector { get; init; }
        public Action<int>? PrintSector { get; init; }
        public Action? PlaySecretSound { get; init; }
        public Action? IncrementFoundSecrets { get; init; }
    }

    public static void ActorOnSpecialFlat(Actor victim, int floorType, TerrainFlatContext ctx)
    {
        if (ctx.DamageAmount?.Invoke(floorType) == 0)
            return;
        var mask = ctx.DamageTimeMask?.Invoke(floorType) ?? 0;
        if (ctx.LevelTime % (mask + 1) != 0)
            return;
        object? ironfeet = null;
        if (ctx.AllowProtection?.Invoke(floorType) == true)
            ironfeet = ctx.FindIronFeet?.Invoke(victim);
        var damage = 0;
        if (ironfeet == null)
            damage = ctx.ApplyTerrainDamage?.Invoke(victim, floorType) ?? 0;
        if (damage > 0 && ctx.SplashIndex?.Invoke(floorType) != -1)
            ctx.PlaySplash?.Invoke(victim, floorType);
    }

    public sealed class TerrainFlatContext
    {
        public int LevelTime { get; init; }
        public Func<int, int>? DamageAmount { get; init; }
        public Func<int, int>? DamageTimeMask { get; init; }
        public Func<int, bool>? AllowProtection { get; init; }
        public Func<Actor, object?>? FindIronFeet { get; init; }
        public Func<Actor, int, int>? ApplyTerrainDamage { get; init; }
        public Func<int, int>? SplashIndex { get; init; }
        public Action<Actor, int>? PlaySplash { get; init; }
    }
}
