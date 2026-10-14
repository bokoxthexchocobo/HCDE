namespace HCDE.Playsim;

/// <summary>Native A_Light and fixed weapon-light actions from actor.zs.</summary>
public static class ActorWeaponLightActions
{
    public static void Light(Actor actor, int extraLight)
    {
        if (actor is PlayerPawn player) player.ExtraLight = Math.Clamp(extraLight, -20, 20);
    }

    public static void Light0(Actor actor) => Light(actor, 0);
    public static void Light1(Actor actor) => Light(actor, 1);
    public static void Light2(Actor actor) => Light(actor, 2);
    public static void LightInverse(Actor actor)
    {
        if (actor is PlayerPawn player) player.ExtraLight = int.MinValue;
    }
}
