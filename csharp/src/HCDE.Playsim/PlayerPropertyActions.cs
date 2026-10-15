namespace HCDE.Playsim;

/// <summary>Represented native SetPlayerProperty action properties.</summary>
public static class PlayerPropertyActions
{
    public const int SetPlayerProperty = 191;
    public const int Frozen = 0;
    public const int NoTarget = 1;
    public const int InstantWeaponSwitch = 2;
    public const int Fly = 3;
    public const int TotallyFrozen = 4;
    public const int Strength = 6;
    public const int RadiationSuit = 8;
    public const int Buddha = 16;
    public const int Buddha2 = 17;
    public const int Frightening = 18;
    public const int NoClip = 19;
    public const int NoClip2 = 20;
    public const int GodMode = 21;
    public const int GodMode2 = 22;

    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator,
        int who, int value, int property)
    {
        if (special != SetPlayerProperty) return null;
        if (property is not (Frozen or NoTarget or InstantWeaponSwitch or Fly or TotallyFrozen or Strength or RadiationSuit or Buddha or Buddha2 or Frightening or NoClip or NoClip2 or GodMode or GodMode2))
            return false;
        if (who == 0)
        {
            if (activator is not PlayerPawn { Destroyed: false } player) return false;
            if (property == Fly) player.FlyCheat = value != 0;
            Set(player, property, value != 0);
        }
        else
        {
            foreach (var player in sim.Players.Where(p => !p.Destroyed))
            {
                Set(player, property, value != 0);
                if (property is Frozen or TotallyFrozen && sim.Compat.HasFlag(CompatSurface.LinkFrozenProperties))
                    player.Frozen = player.TotallyFrozen = value != 0;
            }
        }
        return property != Fly;
    }

    private static void Set(PlayerPawn player, int property, bool enabled)
    {
        switch (property)
        {
            case Frozen: player.Frozen = enabled; break;
            case TotallyFrozen: player.TotallyFrozen = enabled; break;
            case NoTarget: player.NoTargetCheat = enabled; break;
            case InstantWeaponSwitch: player.InstantWeaponSwitch = enabled; break;
            case Fly: player.Fly = player.NoGravity = enabled; break;
            case Strength:
                if (enabled) AcsPlayerInventory.Give(player, "PowerStrength", 1);
                else AcsPlayerInventory.Take(player, "PowerStrength", 1);
                break;
            case RadiationSuit:
                if (enabled) AcsPlayerInventory.Give(player, "PowerIronFeet", 1);
                else AcsPlayerInventory.Take(player, "PowerIronFeet", 1);
                break;
            case Buddha: player.Buddha = enabled; break;
            case Buddha2: player.Buddha2 = enabled; break;
            case Frightening: player.Frightening = enabled; break;
            case NoClip: player.NoClip = enabled; break;
            case NoClip2:
                player.NoClip = player.NoClip2 = enabled;
                if (enabled) player.NoGravity = true;
                else if (!player.Fly) player.NoGravity = false;
                break;
            case GodMode: player.GodMode = enabled; break;
            case GodMode2: player.GodMode2 = enabled; break;
        }
    }
}
