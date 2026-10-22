namespace HCDE.Playsim;

public static class VmPPSpriteCreatePsprite
{
    public const int PspfAddweapon = VmPPSpriteLayers.PspfAddweapon;
    public const int PspfAddbob = VmPPSpriteLayers.PspfAddbob;
    public const int PspfPowdouble = VmPPSpriteLayers.PspfPowdouble;
    public const int PspfCvarfast = VmPPSpriteLayers.PspfCvarfast;
    public const int PspfPivotpercent = VmPPSpriteLayers.PspfPivotpercent;

    public sealed class PlayerOwner
    {
        public VmPPSpriteLayers.PspriteModel? Psprites { get; set; }
    }

    public static VmPPSpriteLayers.PspriteModel Construct(
        PlayerOwner owner,
        object? caller,
        int id,
        Func<PlayerOwner, object?, int, VmPPSpriteLayers.PspriteModel>? allocate,
        Func<object?, bool>? isWeaponOrPawn,
        Action<VmPPSpriteLayers.PspriteModel>? destroyDuplicate)
    {
        var pspr = allocate?.Invoke(owner, caller, id)
            ?? throw new InvalidOperationException("allocate required");

        pspr.Caller = caller;
        pspr.Id = id;
        pspr.BaseScaleX = 1.0;
        pspr.BaseScaleY = 1.2;
        pspr.Alpha = 1;

        VmPPSpriteLayers.PspriteModel? prev = null;
        var next = owner.Psprites;
        while (next != null && next.Id < id)
        {
            prev = next;
            next = next.Next;
        }

        pspr.Next = next;
        if (prev == null)
            owner.Psprites = pspr;
        else
            prev.Next = pspr;

        if (next != null && next.Id == id && id != 0)
            destroyDuplicate?.Invoke(next);

        if (caller != null && isWeaponOrPawn?.Invoke(caller) == true)
            pspr.Flags = PspfAddweapon | PspfAddbob | PspfPowdouble | PspfCvarfast | PspfPivotpercent;

        return pspr;
    }

    public static VmPPSpriteLayers.PspriteModel? CreatePsprite(
        PlayerOwner owner,
        object? caller,
        int layer,
        Func<PlayerOwner, object?, int, VmPPSpriteLayers.PspriteModel>? allocate,
        Func<object?, bool>? isWeapon,
        Func<object?, string, double>? floatVar,
        Func<object?, bool>? isWeaponOrPawn,
        Action<VmPPSpriteLayers.PspriteModel>? destroyDuplicate)
    {
        var pspr = Construct(owner, caller, layer, allocate, isWeaponOrPawn, destroyDuplicate);
        if (caller != null && isWeapon?.Invoke(caller) == true)
        {
            pspr.BaseScaleX = floatVar?.Invoke(caller, "WeaponScaleX") ?? pspr.BaseScaleX;
            pspr.BaseScaleY = floatVar?.Invoke(caller, "WeaponScaleY") ?? pspr.BaseScaleY;
        }

        return pspr;
    }
}
