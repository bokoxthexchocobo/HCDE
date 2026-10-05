namespace HCDE.Playsim;

public static class ActorSpecialActions
{
    internal static bool OnDeath(AuthoritySimulation sim, Actor thing, Actor? trigger)
    {
        if (thing.Special == 0 || thing.SpecialPickup && !thing.IsMonster || (thing.ActivationType & 64) != 0) return false;
        return ActivateSpecial(sim, thing, trigger, true);
    }

    public static bool ActivateSpecial(AuthoritySimulation sim, Actor thing, Actor? trigger, bool death = false)
    {
        if ((thing.ActivationType & 2) != 0) thing.Brain?.SetSpecialTarget(trigger);
        if ((thing.ActivationType & 4) != 0 && trigger != null) trigger.Brain?.SetSpecialTarget(thing);
        var result = false;
        if (!death && (thing.ActivationType & (256 | 512 | 1024)) != 0)
        {
            if ((thing.ActivationType & 1024) != 0 && (thing.ActivationType & (256 | 512)) == 0)
                thing.ActivationType |= 256;
            var activate = (thing.ActivationType & 256) != 0;
            thing.ActivationType &= ~(activate ? 256 : 512);
            if ((thing.ActivationType & 1024) != 0) thing.ActivationType |= activate ? 512 : 256;
            if (activate) thing.Activate(trigger); else thing.Deactivate(trigger);
            result = true;
        }
        if (thing.Special == 0) return result;
        if ((thing.ActivationType & 1) != 0 || death && sim.Level.ActivateOwnDeathSpecials && (thing.ActivationType & 128) == 0) trigger = thing;
        var special = thing.Special;
        var args = thing.SpecialArgs.ToArray();
        result = ThingStop.ExecuteSpecial(sim, special, trigger, args[0])
            ?? ThingThrust.ExecuteSpecial(sim, special, trigger, args[0], args[1], args[2], args[3])
            ?? ThingThrustZ.ExecuteSpecial(sim, special, trigger, args[0], args[1], args[2], args[3])
            ?? ThingSetSpecial.Execute(sim, special, trigger, args[0], args[1], args[2], args[3], args[4])
            ?? ThingChangeTid.ExecuteSpecial(sim, special, trigger, args[0], args[1])
            ?? ThingRemove.ExecuteSpecial(sim, special, trigger, args[0])
            ?? ThingRaise.Execute(sim, special, trigger, args[0], args[1])
            ?? ThingDamage.Execute(sim, special, trigger, args[0], args[1], args[2])
            ?? HealthActions.Execute(special, trigger, args[0], args[1])
            ?? ThingActivation.ExecuteSpecial(sim, special, trigger, args[0])
            ?? LightActions.ExecuteSpecial(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? SectorGravity.ExecuteSpecial(sim, special, args[0], args[1], args[2])
            ?? SectorDamage.ExecuteSpecial(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? LineSpecials.ExecuteSectorRotation(sim, special, args[0], args[1], args[2])
            ?? SectorTexturePanning.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? SectorTextureScale.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? SectorTextureAlignment.Execute(sim, special, args[0], args[1])
            ?? WallTextureOffset.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? WallTextureScale.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? WallScrollActions.Execute(sim, special, args[0], args[1], args[2], args[3], args[4])
            ?? GeometryHealthActions.Execute(sim, special, args[0], args[1], args[2])
            ?? LineSpecials.ExecuteExitSpecial(sim, special, trigger)
            ?? LineSpecials.ExecuteTeleportSpecial(sim, special, args[0], args[1], trigger, false)
            ?? LineSpecials.ExecuteScriptControl(sim, special, args[0], args[1], args[2], args[3], args[4], trigger, null, false)
            ?? LineSpecials.ExecuteDoorSpecial(sim, special, args[0], args[1], args[2], args[3], null)
            ?? LineSpecials.ExecuteFloorSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], null)
            ?? LineSpecials.ExecuteCeilingSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], null)
            ?? LineSpecials.ExecuteStairSpecial(sim, special, args[0], args[1], args[2], args[3], args[4], null)
            ?? false;
        if (death && !sim.Level.HexenHack || (thing.ActivationType & 32) != 0 && result) thing.Special = 0;
        return result;
    }
}
