namespace HCDE.Playsim;

internal static class HealthActions
{
    internal const int HealThing = 248;
    internal const int DamageThing = 73;

    internal static bool? Execute(int special, Actor? activator, int amount, int maximum)
    {
        if (special is not (HealThing or DamageThing)) return null;
        if (activator is null) return false;
        if (special == DamageThing)
        {
            if (amount < 0)
            {
                if (activator is PlayerPawn) activator.GiveBody((int)Math.Min(-(long)amount, int.MaxValue));
                else activator.Health = (int)Math.Min((long)activator.Health - amount, activator.ResurrectionHealth);
            }
            else ActorDamage.Apply(activator, amount == 0 ? ActorDamage.TelefragDamage : amount,
                damageType: DamageType(maximum));
            return true;
        }
        if (maximum == 0 || activator is not PlayerPawn)
        {
            activator.GiveBody(amount);
            return true;
        }
        if (maximum == 1) maximum = activator.Simulation?.DehackedMaxSoulsphere ?? 200;
        if (activator.Health < maximum)
        {
            var health = Math.Clamp((long)activator.Health + amount, int.MinValue, int.MaxValue);
            activator.Health = (int)(maximum > 0 ? Math.Min(health, maximum) : health);
        }
        return true;
    }

    internal static string? DamageType(int mod) => mod switch
    {
        9 => "BFGSplash", 12 => "Drowning", 13 => "Slime", 14 => "Fire",
        15 => "Crush", 16 => "Telefrag", 17 => "Falling", 18 => "Suicide",
        20 => "Exit", 22 => "Melee", 23 => "Railgun", 24 => "Ice",
        25 => "Disintegrate", 26 => "Poison", 27 => "Electric", 1000 => "Massacre",
        _ => null,
    };
}
