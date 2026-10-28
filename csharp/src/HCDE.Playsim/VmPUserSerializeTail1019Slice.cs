namespace HCDE.Playsim;

/// <summary>
/// <c>player_t::Serialize</c> tail field keys and <c>PlayerInfo</c> <c>DEFINE_FIELD</c> export manifest from
/// <c>p_user.cpp</c> (**2545–2694**). Post-serialize / freeze gates remain on <see cref="VmPUserSerializeFreeze"/>.
/// </summary>
public static class VmPUserSerializeTail1019Slice
{
    public const string SafeposLegacyArcKey = "safepos";
    public const int MaxPlayersFragsArray = 64;

    public static IReadOnlyList<string> SerializeTailArcKeys { get; } = new[]
    {
        "chickenpeck", "jumptics", "respawntime", "airfinished", "turnticks", "oldbuttons",
        "hazardtype", "hazardinterval", "bot", "blendr", "blendg", "blendb", "blenda",
        "weaponstate", "logtext", "subtitletext", "subtitlecounter", "conversionnpc", "conversionpc",
        "conversionnpcangle", "conversionfacetalker", "frags", "psprites", "currentplayerclass",
        "crouchfactor", "crouching", "crouchdir", "crouchoffset", "crouchviewdelta",
        "original_cmd", "original_oldbuttons", "poisontype", "poisonpaintype", "timefreezer",
        "onground", "musinfoactor", "musinfotics", "soundclass", "angleoffsettargets", "lastdamagetype",
    };

    public static bool SerializeTailUsesFragsArray(string key) =>
        string.Equals(key, "frags", StringComparison.OrdinalIgnoreCase);

    public static bool SerializeTailUsesPsprites(string key) =>
        string.Equals(key, "psprites", StringComparison.OrdinalIgnoreCase);

    public static bool SafeposUsesLegacyKey(string arcKey) =>
        string.Equals(arcKey, SafeposLegacyArcKey, StringComparison.Ordinal);

    public static int SerializeTailArcKeyCount => SerializeTailArcKeys.Count;

    public static bool TryGetSerializeTailArcKey(int index, out string key)
    {
        if (index < 0 || index >= SerializeTailArcKeys.Count)
        {
            key = "";
            return false;
        }

        key = SerializeTailArcKeys[index];
        return true;
    }

    public static IReadOnlyList<string> PlayerInfoVmExportNames { get; } = new[]
    {
        "mo", "playerstate", "original_oldbuttons", "cls", "DesiredFOV", "FOV", "viewz", "viewheight",
        "deltaviewheight", "bob", "BobTimer", "Vel", "centering", "turnticks", "attackdown", "usedown",
        "oldbuttons", "health", "inventorytics", "CurrentPlayerClass", "frags", "fragcount", "lastkilltime",
        "multicount", "spreecount", "WeaponState", "ReadyWeapon", "PendingWeapon", "psprites", "cheats",
        "timefreezer", "refire", "inconsistent", "waiting", "killcount", "itemcount", "secretcount",
        "damagecount", "bonuscount", "hazardcount", "hazardinterval", "hazardtype", "poisoncount",
        "poisontype", "poisonpaintype", "poisoner", "attacker", "extralight", "fixedcolormap",
        "fixedlightlevel", "morphTics", "MorphedPlayerClass", "MorphStyle", "MorphExitFlash", "PremorphWeapon",
        "chickenPeck", "jumpTics", "onground", "respawn_time", "camera", "air_finished", "LastDamageType",
        "MUSINFOactor", "MUSINFOtics", "settings_controller", "crouching", "crouchdir", "Bot", "BlendR",
        "BlendG", "BlendB", "BlendA",
    };

    public static int PlayerInfoVmExportCount => PlayerInfoVmExportNames.Count;

    public static bool TryGetPlayerInfoVmExportName(int index, out string name)
    {
        if (index < 0 || index >= PlayerInfoVmExportNames.Count)
        {
            name = "";
            return false;
        }

        name = PlayerInfoVmExportNames[index];
        return true;
    }

    public static bool PlayerInfoExportUsesNamedAlias(string exportName) =>
        exportName is "inconsistent";
}
