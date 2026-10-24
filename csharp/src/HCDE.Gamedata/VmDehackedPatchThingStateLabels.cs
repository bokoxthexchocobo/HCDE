namespace HCDE.Gamedata;

/// <summary><c>PatchThing</c> frame/sound label routing from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingStateLabels
{
    public static bool TryMapFrameLabel(string key, int stateNum, IDictionary<string, int> labels)
    {
        if (!key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase)) return false;
        if (key.StartsWith("Initial", StringComparison.OrdinalIgnoreCase))
        {
            labels["Spawn"] = stateNum;
            return true;
        }

        if (key.StartsWith("First moving", StringComparison.OrdinalIgnoreCase))
        {
            labels["See"] = stateNum;
            return true;
        }

        if (key.StartsWith("Injury", StringComparison.OrdinalIgnoreCase))
        {
            labels["Pain"] = stateNum;
            return true;
        }

        if (key.StartsWith("Close attack", StringComparison.OrdinalIgnoreCase))
        {
            labels["Melee"] = stateNum;
            return true;
        }

        if (key.StartsWith("Far attack", StringComparison.OrdinalIgnoreCase))
        {
            labels["Missile"] = stateNum;
            return true;
        }

        if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase))
        {
            labels["Death"] = stateNum;
            return true;
        }

        if (key.StartsWith("Exploding", StringComparison.OrdinalIgnoreCase))
        {
            labels["XDeath"] = stateNum;
            return true;
        }

        if (key.StartsWith("Respawn", StringComparison.OrdinalIgnoreCase))
        {
            labels["Raise"] = stateNum;
            return true;
        }

        return false;
    }

    public static bool TryMapSoundLabel(string key, int soundId, IDictionary<string, int> sounds)
    {
        if (!key.EndsWith(" sound", StringComparison.OrdinalIgnoreCase)) return false;
        if (key.StartsWith("Alert", StringComparison.OrdinalIgnoreCase)) { sounds["See"] = soundId; return true; }
        if (key.StartsWith("Attack", StringComparison.OrdinalIgnoreCase)) { sounds["Attack"] = soundId; return true; }
        if (key.StartsWith("Pain", StringComparison.OrdinalIgnoreCase)) { sounds["Pain"] = soundId; return true; }
        if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase)) { sounds["Death"] = soundId; return true; }
        if (key.StartsWith("Action", StringComparison.OrdinalIgnoreCase)) { sounds["Active"] = soundId; return true; }
        if (key.StartsWith("Rip", StringComparison.OrdinalIgnoreCase)) { sounds["Rip"] = soundId; return true; }
        return false;
    }
}
