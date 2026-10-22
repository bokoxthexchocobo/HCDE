namespace HCDE.Playsim;

public static class VmPPSpriteCreatePsprite
{
    public static VmPPSpriteLayers.PspriteModel? CreatePsprite(
        object? player,
        object? caller,
        int layer,
        Func<object?, object?, int, VmPPSpriteLayers.PspriteModel?>? factory,
        Func<object?, bool>? isWeapon,
        Func<object?, string, double>? floatVar)
    {
        var pspr = factory?.Invoke(player, caller, layer);
        if (pspr == null)
            return null;
        if (caller != null && isWeapon?.Invoke(caller) == true)
        {
            pspr.BaseScaleX = floatVar?.Invoke(caller, "WeaponScaleX") ?? pspr.BaseScaleX;
            pspr.BaseScaleY = floatVar?.Invoke(caller, "WeaponScaleY") ?? pspr.BaseScaleY;
        }
        return pspr;
    }
}
