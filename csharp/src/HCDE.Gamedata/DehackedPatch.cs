namespace HCDE.Gamedata;

public sealed class DehackedActor
{
    internal DehackedActor Copy() => (DehackedActor)MemberwiseClone();
    public int Index { get; init; }
    public string Name { get; init; } = "";
    public int Health { get; set; }
    public double Speed { get; set; }
    public bool SpeedPatched { get; set; }
    public double Radius { get; set; }
    public bool WidthPatched { get; set; }
    public double Height { get; set; }
    public bool HeightPatched { get; set; }
    public int MissileDamage { get; set; }
    public bool MissileDamagePatched { get; set; }
    public int ReactionTime { get; set; }
    public int InfightingGroup { get; set; }
    public bool InfightingGroupPatched { get; set; }
    public int ProjectileGroup { get; set; }
    public bool ProjectileGroupPatched { get; set; }
    public int SplashGroup { get; set; }
    public bool SplashGroupPatched { get; set; }
    public bool ReactionTimePatched { get; set; }
    public int Mass { get; set; }
    public bool MassPatched { get; set; }
    public int PainChance { get; set; }
    public int DoomEdNum { get; set; }
    public int OriginalDoomEdNum { get; init; }
    public uint Bits { get; set; }
    public bool BitsPatched { get; set; }
    public uint Bits2 { get; set; }
    public bool Bits2Patched { get; set; }
    public bool GravityPatched { get; set; }
    public double Gravity { get; set; } = 1;
    public bool BitsUseStealth { get; set; }
    public bool NoBlockMonsters { get; set; }
    public int SeeSound { get; set; }
    public int AttackSound { get; set; }
    public int PainSound { get; set; }
    public int DeathSound { get; set; }
    public int ActiveSound { get; set; }
    public int SpawnState { get; set; }
    public int SeeState { get; set; }
    public int PainState { get; set; }
    public int MeleeState { get; set; }
    public int MissileState { get; set; }
    public int DeathState { get; set; }
    public int XDeathState { get; set; }
    public int RaiseState { get; set; }
    public bool Patched { get; set; }
}

public sealed class DehackedState
{
    internal DehackedState Copy() => (DehackedState)MemberwiseClone();
    public const int DehackedFlag = 64;
    public const int FullBrightFlag = 16;

    public int Index { get; init; }
    public int Tics { get; set; }
    public int Sprite { get; set; }
    public int Frame { get; set; }
    public int NextState { get; set; }
    public int Misc1 { get; set; }
    public int Misc2 { get; set; }
    public int StateFlags { get; set; }
    public bool FullBright => (StateFlags & FullBrightFlag) != 0;
    public bool Patched => (StateFlags & DehackedFlag) != 0;
}

public sealed class DehackedSound
{
    internal DehackedSound Copy() => (DehackedSound)MemberwiseClone();
    public int Index { get; init; }
    public string Name { get; set; } = "";
}

public sealed class DehackedPatchResult
{
    public bool HealthBonusCapPatched { get; init; }
    public bool NoAutofreeze { get; init; }
    public int MaxHealth { get; init; } = 100;
    public int InitialHealth { get; init; } = 100;
    public int InitialBullets { get; init; } = 50;
    public int MaxArmor { get; init; } = 200;
    public int GreenArmorClass { get; init; } = 1;
    public int BlueArmorClass { get; init; } = 2;
    public int MaxSoulsphere { get; init; } = 200;
    public int SoulsphereHealth { get; init; } = 100;
    public int MegasphereHealth { get; init; } = 200;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<DehackedActor> Actors { get; init; } = Array.Empty<DehackedActor>();
    public IReadOnlyList<DehackedState> States { get; init; } = Array.Empty<DehackedState>();
    public IReadOnlyList<DehackedSound> Sounds { get; init; } = Array.Empty<DehackedSound>();
}

/// <summary>
/// Thing, frame, and sound patches from a DEHACKED lump.
/// Thing and frame fields follow <c>PatchThing</c> / <c>PatchFrame</c>.
/// Classic <c>Sound</c> blocks are consumed and ignored, matching current <c>PatchSound</c>.
/// BEX <c>[SOUNDS]</c> renames follow <c>PatchSoundNames</c>.
/// </summary>
public static class DehackedPatch
{
    public static DehackedPatchResult Apply(string text, DehackedPatchResult? baseline = null)
    {
        var actors = (baseline?.Actors ?? CreateVanillaActors()).ToDictionary(actor => actor.Index, actor => actor.Copy());
        var states = (baseline?.States ?? CreateVanillaStates(1024)).ToDictionary(state => state.Index, state => state.Copy());
        var sounds = (baseline?.Sounds ?? CreateVanillaSounds()).ToDictionary(sound => sound.Index, sound => sound.Copy());
        var errors = new List<string>();
        var maxHealth = baseline?.MaxHealth ?? 100;
        var initialHealth = baseline?.InitialHealth ?? 100;
        var initialBullets = baseline?.InitialBullets ?? 50;
        var noAutofreeze = baseline?.NoAutofreeze ?? false;
        var maxArmor = baseline?.MaxArmor ?? 200;
        var greenArmorClass = baseline?.GreenArmorClass ?? 1;
        var blueArmorClass = baseline?.BlueArmorClass ?? 2;
        var healthBonusCapPatched = baseline?.HealthBonusCapPatched ?? false;
        var maxSoulsphere = baseline?.MaxSoulsphere ?? 200;
        var soulsphereHealth = baseline?.SoulsphereHealth ?? 100;
        var megasphereHealth = baseline?.MegasphereHealth ?? 200;
        var lines = SplitLines(text);

        for (var i = 0; i < lines.Count; i++)
        {
            var header = lines[i];
            if (header.Length == 0 || header[0] == '#')
                continue;
            if (header.Contains('='))
                continue;

            if (TryHeader(header, "Thing", out var thingIndex))
            {
                var body = ReadBody(lines, ref i);
                if (!actors.TryGetValue(thingIndex, out var actor))
                {
                    errors.Add($"Thing {thingIndex} out of range or invalid.");
                    continue;
                }

                ApplyThing(actor, body, errors);
            }
            else if (TryHeader(header, "Frame", out var frameIndex))
            {
                var body = ReadBody(lines, ref i);
                if (!states.TryGetValue(frameIndex, out var state))
                {
                    errors.Add($"Frame {frameIndex} out of range");
                    continue;
                }

                ApplyFrame(state, body, errors);
            }
            else if (TryHeader(header, "Misc", out _))
            {
                // Native PatchMisc changes HealthBonus's default even without a Max Health entry.
                healthBonusCapPatched = true;
                foreach (var (key, value) in ReadBody(lines, ref i))
                {
                    var setting = key.ToUpperInvariant();
                    if (setting is not ("NO AUTOFREEZE" or "INITIAL HEALTH" or "INITIAL BULLETS" or "MAX HEALTH" or "MAX ARMOR" or "GREEN ARMOR CLASS" or "BLUE ARMOR CLASS" or "MAX SOULSPHERE" or "SOULSPHERE HEALTH" or "MEGASPHERE HEALTH")) continue;
                    if (!int.TryParse(value, out var parsed))
                    {
                        errors.Add($"Misc {key} is not an integer.");
                        continue;
                    }
                    switch (setting)
                    {
                        case "MAX HEALTH": maxHealth = parsed; break;
                        case "INITIAL HEALTH": initialHealth = parsed; break;
                        case "INITIAL BULLETS": initialBullets = parsed; break;
                        case "NO AUTOFREEZE": noAutofreeze = parsed != 0; break;
                        case "MAX ARMOR": maxArmor = parsed; break;
                        case "GREEN ARMOR CLASS": greenArmorClass = parsed; break;
                        case "BLUE ARMOR CLASS": blueArmorClass = parsed; break;
                        case "MAX SOULSPHERE": maxSoulsphere = parsed; break;
                        case "SOULSPHERE HEALTH": soulsphereHealth = parsed; break;
                        case "MEGASPHERE HEALTH": megasphereHealth = parsed; break;
                    }
                }
                if (actors.TryGetValue(1, out var player))
                {
                    player.Health = initialHealth;
                    player.Patched = true;
                }
            }
            else if (TryHeader(header, "Sound", out _))
            {
                ReadBody(lines, ref i);
            }
            else if (header.Equals("[SOUNDS]", StringComparison.OrdinalIgnoreCase))
            {
                var body = ReadBody(lines, ref i);
                ApplySoundNames(sounds, body);
            }
        }

        return new DehackedPatchResult
        {
            Errors = errors,
            MaxHealth = maxHealth,
            InitialHealth = initialHealth,
            InitialBullets = initialBullets,
            NoAutofreeze = noAutofreeze,
            MaxArmor = maxArmor,
            GreenArmorClass = greenArmorClass,
            BlueArmorClass = blueArmorClass,
            HealthBonusCapPatched = healthBonusCapPatched,
            MaxSoulsphere = maxSoulsphere,
            SoulsphereHealth = soulsphereHealth,
            MegasphereHealth = megasphereHealth,
            Actors = actors.Values.OrderBy(actor => actor.Index).ToArray(),
            States = states.Values.OrderBy(state => state.Index).ToArray(),
            Sounds = sounds.Values.OrderBy(sound => sound.Index).ToArray(),
        };
    }

    public static IReadOnlyList<DehackedActor> CreateVanillaActors()
    {
        // 1-based Dehacked thing numbers follow the Doom mobjinfo / InfoNames order.
        string[] names =
        [
            "Player", "Zombieman", "ShotgunGuy", "Archvile", "ArchvileFire",
            "Revenant", "RevenantTracer", "Smoke", "Mancubus", "FatShot",
            "Chaingunner", "Imp",
            "Demon", "Spectre", "Cacodemon", "BaronOfHell", "BaronBall",
            "HellKnight", "LostSoul", "SpiderMastermind", "Arachnotron",
            "Cyberdemon", "PainElemental", "WolfensteinSS",
            "CommanderKeen", "BossBrain", "BossEye", "BossTarget", "SpawnShot", "SpawnFire", "ExplosiveBarrel",
            "DoomImpBall", "CacodemonBall", "Rocket", "PlasmaBall", "BFGBall", "ArachnotronPlasma",
        ];
        var actors = new DehackedActor[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            actors[i] = new DehackedActor
            {
                Index = i + 1,
                Name = names[i],
                Health = names[i] == "Player" ? 100 : 30,
                DoomEdNum = i switch { 0 => 1, 1 => 3004, 2 => 9, 3 => 64, 5 => 66, 8 => 67, 10 => 65, 11 => 3001,
                    12 => 3002, 13 => 58, 14 => 3005, 15 => 3003, 17 => 69,
                    18 => 3006, 19 => 7, 20 => 68, 21 => 16, 22 => 71, 23 => 84, _ => -1 },
                OriginalDoomEdNum = i switch { 0 => 1, 1 => 3004, 2 => 9, 3 => 64, 5 => 66, 8 => 67, 10 => 65, 11 => 3001,
                    12 => 3002, 13 => 58, 14 => 3005, 15 => 3003, 17 => 69,
                    18 => 3006, 19 => 7, 20 => 68, 21 => 16, 22 => 71, 23 => 84, _ => -1 },
            };
            if (DoomActorCatalog.Find(actors[i].DoomEdNum) is { } definition)
            {
                actors[i].Health = definition.Health;
                actors[i].Radius = definition.Radius;
                actors[i].Height = definition.Height;
                actors[i].Speed = definition.Speed;
                actors[i].PainChance = definition.PainChance;
            }
        }

        return actors;
    }

    public static IReadOnlyList<DehackedState> CreateVanillaStates(int count)
    {
        var states = new DehackedState[count];
        for (var i = 0; i < count; i++)
        {
            var tics = i switch
            {
                47 => 5,
                48 => 4,
                _ => 0,
            };
            states[i] = new DehackedState
            {
                Index = i,
                Tics = tics,
                NextState = i + 1 < count ? i + 1 : 0,
            };
        }

        return states;
    }

    public static IReadOnlyList<DehackedSound> CreateVanillaSounds(int count = 128) =>
        Enumerable.Range(1, count).Select(index => new DehackedSound { Index = index, Name = $"SFX{index}" }).ToArray();

    private static void ApplyThing(DehackedActor actor, IReadOnlyList<(string Key, string Value)> body, List<string> errors)
    {
        actor.Patched = true;
        foreach (var (key, value) in body)
        {
            var numeric = ParseDecimalPrefix(value);
            if (numeric < int.MinValue || numeric > uint.MaxValue)
            {
                errors.Add($"Thing {actor.Index}: bad numeric constant {value} for {key}.");
                continue;
            }
            if (key.Equals("Hit points", StringComparison.OrdinalIgnoreCase))
                actor.Health = unchecked((int)numeric);
            else if (key.Equals("Reaction time", StringComparison.OrdinalIgnoreCase))
            {
                actor.ReactionTime = unchecked((int)numeric);
                actor.ReactionTimePatched = true;
            }
            else if (key.Equals("Pain chance", StringComparison.OrdinalIgnoreCase))
                actor.PainChance = unchecked((short)numeric);
            else if (key.Equals("Infighting group", StringComparison.OrdinalIgnoreCase))
            {
                actor.InfightingGroup = numeric < 0 ? 0 : unchecked((int)numeric);
                actor.InfightingGroupPatched = true;
            }
            else if (key.Equals("Projectile group", StringComparison.OrdinalIgnoreCase))
            {
                actor.ProjectileGroup = numeric < 0 ? -1 : unchecked((int)numeric);
                actor.ProjectileGroupPatched = true;
            }
            else if (key.Equals("Splash group", StringComparison.OrdinalIgnoreCase))
            {
                actor.SplashGroup = numeric < 0 ? 0 : unchecked((int)numeric);
                actor.SplashGroupPatched = true;
            }
            else if (key.Equals("Height", StringComparison.OrdinalIgnoreCase))
            {
                actor.Height = (double)numeric / 65536.0;
                actor.HeightPatched = true;
            }
            else if (key.Equals("Width", StringComparison.OrdinalIgnoreCase))
            {
                actor.Radius = (double)numeric / 65536.0;
                actor.WidthPatched = true;
            }
            else if (key.Equals("Speed", StringComparison.OrdinalIgnoreCase))
            {
                var speed = (double)numeric;
                actor.Speed = Math.Abs(speed) >= 256 ? speed / 65536 : speed;
                actor.SpeedPatched = true;
            }
            else if (key.Equals("Missile damage", StringComparison.OrdinalIgnoreCase))
            {
                actor.MissileDamage = unchecked((int)numeric);
                actor.MissileDamagePatched = true;
            }
            else if (key.Equals("Mass", StringComparison.OrdinalIgnoreCase))
            {
                actor.Mass = unchecked((int)numeric);
                actor.MassPatched = true;
            }
            else if (key.Equals("Bits", StringComparison.OrdinalIgnoreCase))
            {
                uint bits = 0;
                var changed = false;
                var useStealth = false;
                var changed2 = false;
                uint bits2 = 0;
                foreach (var token in value.Split([',', '+', '|', ' ', '\t', '\f', '\r'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (token.All(character => character is >= '0' and <= '9' or '-'))
                    {
                        bits |= unchecked((uint)ParseDecimalPrefix(token));
                        changed = true;
                    }
                    else if (SupportedThingBits2.TryGetValue(token, out var namedBits2))
                    {
                        bits2 |= namedBits2;
                        changed2 = true;
                    }
                    else if (SupportedThingBits.TryGetValue(token, out var namedBits))
                    {
                        bits |= namedBits;
                        changed = true;
                        useStealth |= token.Equals("STEALTH", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                        errors.Add($"Thing {actor.Index}: unsupported bit mnemonic '{token}'.");
                }
                if (changed)
                {
                    actor.Bits = bits;
                    actor.BitsPatched = true;
                    actor.BitsUseStealth = useStealth;
                    if (!useStealth && (bits & 0x40000000) != 0) actor.NoBlockMonsters = true;
                }
                if (changed2)
                {
                    if ((bits2 & 1) != 0)
                    {
                        actor.Gravity = 0.25;
                        actor.GravityPatched = true;
                    }
                    actor.Bits2 = bits2 & ~1u;
                    actor.Bits2Patched = true;
                }
            }
            else if (key.Equals("ID #", StringComparison.OrdinalIgnoreCase))
                actor.DoomEdNum = unchecked((int)numeric);
            else if (key.EndsWith(" sound", StringComparison.OrdinalIgnoreCase))
                AssignSound(actor, key, unchecked((int)numeric));
            else if (key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase))
                AssignState(actor, key, unchecked((int)numeric), errors);
        }
    }

    private static void AssignSound(DehackedActor actor, string key, int id)
    {
        if (key.StartsWith("Alert", StringComparison.OrdinalIgnoreCase))
            actor.SeeSound = id;
        else if (key.StartsWith("Attack", StringComparison.OrdinalIgnoreCase))
            actor.AttackSound = id;
        else if (key.StartsWith("Pain", StringComparison.OrdinalIgnoreCase))
            actor.PainSound = id;
        else if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase))
            actor.DeathSound = id;
        else if (key.StartsWith("Action", StringComparison.OrdinalIgnoreCase))
            actor.ActiveSound = id;
    }

    private static void AssignState(DehackedActor actor, string key, int state, List<string> errors)
    {
        if (key.StartsWith("Initial", StringComparison.OrdinalIgnoreCase))
            actor.SpawnState = state;
        else if (key.StartsWith("First moving", StringComparison.OrdinalIgnoreCase))
            actor.SeeState = state;
        else if (key.StartsWith("Injury", StringComparison.OrdinalIgnoreCase))
            actor.PainState = state;
        else if (key.StartsWith("Close attack", StringComparison.OrdinalIgnoreCase))
        {
            if (actor.Index != 1)
                actor.MeleeState = state;
        }
        else if (key.StartsWith("Far attack", StringComparison.OrdinalIgnoreCase))
        {
            if (actor.Index != 1)
                actor.MissileState = state;
        }
        else if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase))
            actor.DeathState = state;
        else if (key.StartsWith("Exploding", StringComparison.OrdinalIgnoreCase))
            actor.XDeathState = state;
        else if (key.StartsWith("Respawn", StringComparison.OrdinalIgnoreCase))
            actor.RaiseState = state;
        else
            errors.Add($"Thing {actor.Index}: unknown state field '{key}'.");
    }

    private static void ApplyFrame(DehackedState state, IReadOnlyList<(string Key, string Value)> body, List<string> errors)
    {
        var tics = state.Tics;
        var misc1 = state.Misc1;
        var misc2 = state.Misc2;
        var frame = state.Frame | (state.FullBright ? 0x8000 : 0);
        var next = state.NextState;
        var sprite = state.Sprite;
        foreach (var (key, value) in body)
        {
            var number = unchecked((int)ParseDecimalPrefix(value));
            if (key.Equals("Duration", StringComparison.OrdinalIgnoreCase))
                tics = Math.Clamp(number, -1, short.MaxValue);
            else if (key.Equals("Unknown 1", StringComparison.OrdinalIgnoreCase))
                misc1 = number;
            else if (key.Equals("Unknown 2", StringComparison.OrdinalIgnoreCase))
                misc2 = number;
            else if (key.Equals("Sprite number", StringComparison.OrdinalIgnoreCase))
                sprite = number;
            else if (key.Equals("Next frame", StringComparison.OrdinalIgnoreCase))
                next = number;
            else if (key.Equals("Sprite subnumber", StringComparison.OrdinalIgnoreCase))
                frame = number;
        }

        if ((uint)(frame & 0x7fff) > 63)
            errors.Add($"Frame {state.Index}: Subnumber must be in range [0,63]");

        state.Tics = tics;
        state.Misc1 = misc1;
        state.Misc2 = misc2;
        state.Sprite = sprite;
        state.NextState = next;
        state.Frame = frame & 0x3f;
        state.StateFlags |= DehackedState.DehackedFlag;
        if ((frame & 0x8000) != 0)
            state.StateFlags |= DehackedState.FullBrightFlag;
        else
            state.StateFlags &= ~DehackedState.FullBrightFlag;
    }

    private static void ApplySoundNames(Dictionary<int, DehackedSound> sounds, IReadOnlyList<(string Key, string Value)> body)
    {
        foreach (var (key, value) in body)
        {
            if (!int.TryParse(key, out var index))
                continue;
            if (!sounds.TryGetValue(index, out var sound))
            {
                sound = new DehackedSound { Index = index };
                sounds.Add(index, sound);
            }

            sound.Name = value.Trim();
        }
    }

    private static bool TryHeader(string line, string kind, out int index)
    {
        index = 0;
        if (!line.StartsWith(kind, StringComparison.OrdinalIgnoreCase))
            return false;
        if (line.Length == kind.Length || !char.IsWhiteSpace(line[kind.Length]))
            return false;

        var rest = line[(kind.Length + 1)..].Trim();
        var space = rest.IndexOf(' ');
        var number = space >= 0 ? rest[..space] : rest;
        return int.TryParse(number, out index);
    }

    private static List<(string Key, string Value)> ReadBody(List<string> lines, ref int index)
    {
        var body = new List<(string Key, string Value)>();
        while (index + 1 < lines.Count)
        {
            var next = lines[index + 1];
            var eq = next.IndexOf('=');
            if (eq <= 0)
                break;
            index++;
            body.Add((next[..eq].Trim(), next[(eq + 1)..].Trim()));
        }

        return body;
    }

    private static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        foreach (var raw in (text ?? string.Empty).Split('\n'))
        {
            var line = raw.Trim().TrimEnd('\r');
            if (line.Length == 0 || line[0] == '#')
                continue;
            lines.Add(line);
        }

        return lines;
    }

    // First flag-set names whose gameplay effects are represented by ActorSpawner.
    // Values follow wadsrc/static/dehsupp.txt; extended sets and remapped bits remain separate work.
    private static readonly IReadOnlyDictionary<string, uint> SupportedThingBits =
        new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["SPECIAL"] = 1, ["SOLID"] = 2, ["SHOOTABLE"] = 4, ["AMBUSH"] = 32,
            ["NOBLOCKMAP"] = 16,
            ["SPAWNCEILING"] = 256, ["NOGRAVITY"] = 512, ["DROPOFF"] = 1024,
            ["PICKUP"] = 2048, ["FLOAT"] = 16384, ["DROPPED"] = 131072,
            ["COUNTKILL"] = 4194304,
            ["FRIEND"] = 0x40000000, ["STEALTH"] = 0x40000000,
        };

    private static readonly IReadOnlyDictionary<string, uint> SupportedThingBits2 =
        new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["LOGRAV"] = 1, ["NOTELEPORT"] = 0x80, ["CANSLIDE"] = 0x400, ["INVULNERABLE"] = 0x08000000,
            ["DORMANT"] = 0x10000000,
        };

    // Thing/Frame use decimal strtoll/atoll: stop at the first non-digit; no digits means zero.
    private static long ParseDecimalPrefix(string value)
    {
        var token = value.TrimStart();
        var end = token.Length > 0 && token[0] is '+' or '-' ? 1 : 0;
        var digits = end;
        while (end < token.Length && token[end] is >= '0' and <= '9') end++;
        if (end == digits) return 0;
        var number = System.Numerics.BigInteger.Parse(token[..end], System.Globalization.CultureInfo.InvariantCulture);
        return number > long.MaxValue ? long.MaxValue : number < long.MinValue ? long.MinValue : (long)number;
    }

    private static int ParseInt(string value) => (int)ParseLong(value);

    private static long ParseLong(string value)
    {
        var trimmed = value.Trim();
        return long.TryParse(trimmed, out var number) ? number : 0;
    }
}
