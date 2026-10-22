namespace HCDE.Playsim;

public static class VmPPSpriteSerializeLifecycle
{
    public static readonly string[] SerializeFieldOrder =
    [
        "next", "caller", "owner", "flags", "state", "tics", "translation", "sprite", "frame", "id",
        "x", "y", "oldx", "oldy", "alpha", "pivot", "scale", "rotation", "halign", "valign",
        "renderstyle_", "baseScale"
    ];

    public sealed class PspriteLifecycleModel
    {
        public PspriteLifecycleModel? Next { get; set; }
        public object? Caller { get; set; }
        public PlayerPspriteOwner? Owner { get; set; }
        public int Flags { get; set; }
        public object? State { get; set; }
        public int Tics { get; set; }
        public int Translation { get; set; }
        public int Sprite { get; set; }
        public int Frame { get; set; }
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public double Alpha { get; set; } = 1;
        public object? Pivot { get; set; }
        public object? Scale { get; set; }
        public object? Rotation { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public int Renderstyle { get; set; }
        public object? BaseScale { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class PlayerPspriteOwner
    {
        public PspriteLifecycleModel? Psprites { get; set; }
    }

    public static void SerializePsprite(PspriteLifecycleModel pspr, Action<string, object?> writeField)
    {
        foreach (var key in SerializeFieldOrder)
        {
            var value = key switch
            {
                "next" => pspr.Next,
                "caller" => pspr.Caller,
                "owner" => pspr.Owner,
                "flags" => pspr.Flags,
                "state" => pspr.State,
                "tics" => pspr.Tics,
                "translation" => pspr.Translation,
                "sprite" => pspr.Sprite,
                "frame" => pspr.Frame,
                "id" => pspr.Id,
                "x" => pspr.X,
                "y" => pspr.Y,
                "oldx" => pspr.OldX,
                "oldy" => pspr.OldY,
                "alpha" => pspr.Alpha,
                "pivot" => pspr.Pivot,
                "scale" => pspr.Scale,
                "rotation" => pspr.Rotation,
                "halign" => pspr.HAlign,
                "valign" => pspr.VAlign,
                "renderstyle_" => pspr.Renderstyle,
                "baseScale" => pspr.BaseScale,
                _ => null
            };
            writeField(key, value);
        }
    }

    public static void DestroyPsprites(PlayerPspriteOwner player, Action<PspriteLifecycleModel>? destroy)
    {
        var pspr = player.Psprites;
        player.Psprites = null;
        while (pspr != null)
        {
            var next = pspr.Next;
            pspr.Next = null;
            destroy?.Invoke(pspr);
            pspr.Destroyed = true;
            pspr = next;
        }
    }

    public sealed class SafeFlashContext
    {
        public Func<object?, object?, bool>? OwnsState { get; init; }
        public Func<object?, int, bool>? OwnsStateAtOffset { get; init; }
        public Func<object?, int>? GetDehIndex { get; init; }
        public Func<int, object?>? LookupDehExtState { get; init; }
        public Action<object?, int, object?, bool>? SetPsprite { get; init; }
        public Func<object?, bool>? IsWeaponClass { get; init; }
        public object? WeaponBaseClass { get; init; }
        public Func<object?, object?>? GetParentClass { get; init; }
    }

    public static void SetSafeFlash(
        SafeFlashContext ctx,
        object? weapon,
        object? player,
        object? flashstate,
        int index)
    {
        if (flashstate != null && weapon != null)
        {
            var cls = weapon;
            while (cls != null && !ReferenceEquals(cls, ctx.WeaponBaseClass))
            {
                if (ctx.OwnsState?.Invoke(cls, flashstate) == true)
                {
                    if (ctx.OwnsStateAtOffset?.Invoke(cls, index) == true)
                    {
                        ctx.SetPsprite?.Invoke(player, VmPPSpriteLayers.PspFlash, OffsetState(flashstate, index), true);
                        return;
                    }

                    if ((ctx.GetDehIndex?.Invoke(flashstate) ?? -1) < 0)
                    {
                        ctx.SetPsprite?.Invoke(player, VmPPSpriteLayers.PspFlash, flashstate, true);
                        return;
                    }

                    break;
                }

                cls = ctx.GetParentClass?.Invoke(cls);
            }

            var dehIndex = ctx.GetDehIndex?.Invoke(flashstate) ?? -1;
            if (dehIndex >= 0)
            {
                var target = ctx.LookupDehExtState?.Invoke(dehIndex + index);
                if (target != null)
                {
                    ctx.SetPsprite?.Invoke(player, VmPPSpriteLayers.PspFlash, target, true);
                    return;
                }
            }

            index = 0;
        }

        ctx.SetPsprite?.Invoke(player, VmPPSpriteLayers.PspFlash, OffsetState(flashstate, index), true);
    }

    public static void OnDestroy(PspriteLifecycleModel pspr, Action<PspriteLifecycleModel, PspriteLifecycleModel?>? writeBarrier)
    {
        var owner = pspr.Owner;
        if (owner?.Psprites == null)
            return;

        if (owner.Psprites != pspr)
        {
            var prev = owner.Psprites;
            while (prev != null && prev.Next != pspr)
                prev = prev.Next;

            if (prev != null && prev.Next == pspr)
            {
                prev.Next = pspr.Next;
                writeBarrier?.Invoke(prev, pspr.Next);
            }
        }
        else
        {
            owner.Psprites = pspr.Next;
            writeBarrier?.Invoke(pspr, pspr.Next);
        }
    }

    public static float GetYAdjust(
        PspriteLifecycleModel pspr,
        bool fullscreen,
        Func<object?, bool>? callerIsWeapon,
        Func<object?, string, double>? floatVar,
        Func<double, float>? statusBarDisplacement)
    {
        var weapon = pspr.Caller;
        if (weapon != null && callerIsWeapon?.Invoke(weapon) == true)
        {
            var yAd = floatVar?.Invoke(weapon, "YAdjust") ?? 0;
            if (yAd != 0)
                return fullscreen ? (float)yAd : statusBarDisplacement?.Invoke(yAd) ?? (float)yAd;
        }

        return 0;
    }

    private static object? OffsetState(object? flashstate, int index)
    {
        if (flashstate is StateSlot[] states && index >= 0 && index < states.Length)
            return states[index];
        if (flashstate is IList<object?> list && index >= 0 && index < list.Count)
            return list[index];
        return flashstate;
    }

    public sealed class StateSlot
    {
        public int DehIndex { get; set; } = -1;
    }
}
