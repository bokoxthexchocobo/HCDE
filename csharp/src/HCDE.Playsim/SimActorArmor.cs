namespace HCDE.Playsim;

public readonly record struct SimActorArmor(int Amount, int SavePercent, int MaxAbsorb, int MaxFullAbsorb, int AbsorbCount)
{
    internal static SimActorArmor? Capture(Actor actor)
    {
        if (actor is PlayerPawn) return null;
        var value = new SimActorArmor(actor.Armor, actor.ArmorSavePercent, actor.MaxAbsorb, actor.MaxFullAbsorb, actor.AbsorbCount);
        return value == default ? null : value;
    }
    internal void Restore(Actor actor)
    {
        actor.Armor = Amount; actor.ArmorSavePercent = SavePercent;
        actor.MaxAbsorb = MaxAbsorb; actor.MaxFullAbsorb = MaxFullAbsorb; actor.AbsorbCount = AbsorbCount;
    }
}
