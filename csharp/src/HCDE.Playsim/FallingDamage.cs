using HCDE.Gamedata;

namespace HCDE.Playsim;

public static class FallingDamage
{
    public static void ApplyMonster(Actor actor)
    {
        if (actor.Destroyed || !actor.IsMonster || actor.VelocityZ.ToDouble() >= -23) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Monster falling damage requires an attached simulation.");
        if (!sim.Level.MonsterFallingDamage && !actor.FallDamage) return;
        if ((uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count && sim.Level.Sectors[actor.SectorIndex].NoFallingDamage) return;
        var velocity = Math.Abs(actor.VelocityZ.ToDouble());
        var damage = !sim.Level.ProperMonsterFallingDamage || velocity > 35
            ? ActorDamage.TelefragDamage : (int)((velocity - 23) * 6);
        ActorDamage.Apply(actor, damage, damageType: "Falling");
    }

    public static void Apply(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Falling damage requires an attached simulation.");
        if ((uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count && sim.Level.Sectors[actor.SectorIndex].NoFallingDamage) return;
        var velocity = Math.Abs(actor.VelocityZ.ToDouble());
        int damage;
        switch (sim.EffectiveFallingDamage)
        {
            case FallingDamageStyle.None: return;
            case FallingDamageStyle.Hexen:
                if (velocity <= 23) return;
                if (velocity >= 63) damage = ActorDamage.TelefragDamage;
                else
                {
                    var scaled = velocity * (16.0 / 23);
                    damage = (int)(scaled * scaled / 10 - 24);
                    if (actor.VelocityZ.ToDouble() > -39 && damage > actor.Health && actor.Health != 1)
                        damage = actor.Health - 1;
                }
                break;
            case FallingDamageStyle.ZDoom:
                if (velocity <= 19) return;
                damage = velocity >= 84 ? ActorDamage.TelefragDamage : Math.Max(1, (int)((velocity * velocity * (11.0 / 128) - 30) / 2));
                break;
            case FallingDamageStyle.Strife:
                if (velocity <= 20) return;
                damage = (int)(velocity / (25000.0 / 65536));
                break;
            default: throw new InvalidOperationException("Invalid falling damage style.");
        }
        if (actor is PlayerPawn player)
        {
            ActorSoundActions.StartSound(actor, "*land", 0);
            SoundPropagation.Alert(sim, actor, actor, splash: true);
            if (damage >= ActorDamage.TelefragDamage && (player.GodMode || player.Buddha || player.PowerBuddhaTics > 0))
                damage = ActorDamage.TelefragDamage - 1;
        }
        ActorDamage.Apply(actor, damage, damageType: "Falling");
    }
}
