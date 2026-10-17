namespace HCDE.Gamedata;

public enum DehackedRenderStyle
{
    None = 0, Normal = 1, Fuzzy = 2, SoulTrans = 3, OptFuzzy = 4,
    Translucent = 6, Add = 7, Shaded = 8,
}

public sealed class DehackedActor
{
    internal DehackedActor Copy()
    {
        var copy = (DehackedActor)MemberwiseClone();
        copy.SoundNameOverrides = new Dictionary<int, string>(SoundNameOverrides);
        return copy;
    }
    public Dictionary<int, string> SoundNameOverrides { get; private set; } = new();
    public int Index { get; init; }
    public string Name { get; init; } = "";
    public int Health { get; set; }
    public bool HealthPatched { get; set; }
    public double Speed { get; set; }
    public bool SpeedPatched { get; set; }
    public double? FastSpeed { get; set; }
    public double Radius { get; set; }
    public bool WidthPatched { get; set; }
    public double Height { get; set; }
    public bool HeightPatched { get; set; }
    public int? ProjectilePassHeightRaw { get; set; }
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
    public bool PainChancePatched { get; set; }
    public int? MeleeRangeRaw { get; set; }
    public double? MeleeThreshold { get; set; }
    public double? MaxTargetRange { get; set; }
    public double? MissileChanceMultiplier { get; set; }
    public byte? MinMissileChance { get; set; }
    public int DoomEdNum { get; set; }
    public int OriginalDoomEdNum { get; init; }
    public DehackedRenderStyle? RenderStyle { get; set; }
    public double? Alpha { get; set; }
    public bool? Invisible { get; set; }
    public uint Bits { get; set; }
    public bool BitsPatched { get; set; }
    public uint Bits2 { get; set; }
    public bool Bits2Patched { get; set; }
    public bool FullVolumeSounds { get; set; }
    public bool FullVolumeSoundsPatched { get; set; }
    public bool GravityPatched { get; set; }
    public double Gravity { get; set; } = 1;
    public bool BitsUseStealth { get; set; }
    public bool NoBlockMonsters { get; set; }
    public bool Touchy { get; set; }
    public int SeeSound { get; set; }
    public int AttackSound { get; set; }
    public int PainSound { get; set; }
    public int DeathSound { get; set; }
    public int ActiveSound { get; set; }
    public int RipSound { get; set; }
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
    public IReadOnlyDictionary<string, string> SoundLumpOverrides { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public bool DsdhackedSoundMode { get; init; }
    public IReadOnlyList<string> TentativeSoundNames { get; init; } = Array.Empty<string>();
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
        var soundLumps = baseline is null ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(baseline.SoundLumpOverrides, StringComparer.OrdinalIgnoreCase);
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
        var dsdhacked = baseline?.DsdhackedSoundMode ?? false;
        var tentativeSounds = new HashSet<string>(baseline?.TentativeSoundNames ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        if (lines.Count > 0 && lines[0].StartsWith("Patch File for DeHackEd v", StringComparison.Ordinal))
        {
            foreach (var line in lines.Skip(1).TakeWhile(line => line.Contains('=')))
            {
                var eq = line.IndexOf('=');
                if (line[..eq].Trim().Equals("Doom version", StringComparison.OrdinalIgnoreCase)
                    && ParseDecimalPrefix(line[(eq + 1)..]) == 2021) dsdhacked = true;
            }
        }

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

                ApplyThing(actor, body, errors, dsdhacked, tentativeSounds);
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
                ApplySoundNames(sounds, soundLumps, body, dsdhacked);
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
            SoundLumpOverrides = soundLumps,
            DsdhackedSoundMode = dsdhacked,
            TentativeSoundNames = tentativeSounds.OrderBy(name => name, StringComparer.Ordinal).ToArray(),
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

        // Sparse additions retain native InfoNames indices without inventing intervening classes.
        (int Index, string Name, int EditorNumber, int Height, int Radius, bool Solid)[] hanging = [
            (102, "BloodyTwitch", 49, 68, 16, true), (103, "Meat2", 50, 84, 16, true),
            (104, "Meat3", 51, 84, 16, true), (105, "Meat4", 52, 68, 16, true),
            (106, "Meat5", 53, 52, 16, true), (107, "NonsolidMeat2", 59, 84, 20, false),
            (108, "NonsolidMeat4", 60, 68, 20, false), (109, "NonsolidMeat3", 61, 84, 20, false),
            (110, "NonsolidMeat5", 62, 52, 20, false), (111, "NonsolidTwitch", 63, 68, 20, false),
            (129, "HangNoGuts", 73, 88, 16, true), (130, "HangBNoBrain", 74, 88, 16, true),
            (131, "HangTLookingDown", 75, 64, 16, true), (132, "HangTSkull", 76, 64, 16, true),
            (133, "HangTLookingUp", 77, 64, 16, true), (134, "HangTNoBrain", 78, 64, 16, true),
        ];
        (int Index, string Name, int EditorNumber, int Height, int Radius)[] props = [
            (80, "TechLamp", 85, 80, 16), (81, "TechLamp2", 86, 60, 16), (82, "Column", 2028, 48, 16),
            (83, "TallGreenColumn", 30, 52, 16), (84, "ShortGreenColumn", 31, 40, 16),
            (85, "TallRedColumn", 32, 52, 16), (86, "ShortRedColumn", 33, 40, 16),
            (87, "SkullColumn", 37, 40, 16), (88, "HeartColumn", 36, 40, 16),
            (89, "EvilEye", 41, 54, 16), (90, "FloatingSkull", 42, 26, 16), (91, "TorchTree", 43, 56, 16),
            (92, "BlueTorch", 44, 68, 16), (93, "GreenTorch", 45, 68, 16), (94, "RedTorch", 46, 68, 16),
            (95, "ShortBlueTorch", 55, 37, 16), (96, "ShortGreenTorch", 56, 37, 16), (97, "ShortRedTorch", 57, 37, 16),
            (98, "Stalagtite", 47, 40, 16), (99, "TechPillar", 48, 128, 16),
            (100, "Candlestick", 34, 14, 20), (101, "Candelabra", 35, 60, 16),
            (121, "HeadsOnAStick", 28, 64, 16), (123, "HeadOnAStick", 27, 56, 16),
            (124, "HeadCandles", 29, 42, 16), (125, "DeadStick", 25, 64, 16),
            (126, "LiveStick", 26, 64, 16), (127, "BigTree", 54, 108, 32),
            (128, "BurningBarrel", 70, 32, 16),
        ];
        (int Index, string Name, int EditorNumber)[] bodies = [
            (112, "DeadCacodemon", 22), (113, "DeadMarine", 15), (114, "DeadZombieMan", 18),
            (115, "DeadDemon", 21), (116, "DeadLostSoul", 23), (117, "DeadDoomImp", 20),
            (118, "DeadShotgunGuy", 19), (119, "GibbedMarine", 10), (120, "GibbedMarineExtra", 12),
            (122, "Gibs", 24),
        ];
        (int Index, string Name, int EditorNumber, int Height)[] debris = [
            (135, "ColonGibs", 79, 4), (136, "SmallBloodPool", 80, 1), (137, "BrainStem", 81, 4),
        ];
        (int Index, string Name, int EditorNumber, bool CountItem)[] pickups = [
            (44, "GreenArmor", 2018, false), (45, "BlueArmor", 2019, false),
            (46, "HealthBonus", 2014, true), (47, "ArmorBonus", 2015, true),
            (48, "BlueCard", 5, false), (49, "RedCard", 13, false), (50, "YellowCard", 6, false),
            (51, "YellowSkull", 39, false), (52, "RedSkull", 38, false), (53, "BlueSkull", 40, false),
            (54, "Stimpack", 2011, false), (55, "Medikit", 2012, false),
            (56, "Soulsphere", 2013, true), (58, "Berserk", 2023, true),
            (60, "RadSuit", 2025, false), (63, "Megasphere", 83, true),
            (64, "Clip", 2007, false), (65, "ClipBox", 2048, false),
            (66, "RocketAmmo", 2010, false), (67, "RocketBox", 2046, false),
            (68, "Cell", 2047, false), (69, "CellPack", 17, false),
            (70, "Shell", 2008, false), (71, "ShellBox", 2049, false), (72, "Backpack", 8, false),
            (73, "BFG9000", 2006, false), (74, "Chaingun", 2002, false), (75, "Chainsaw", 2005, false),
            (76, "RocketLauncher", 2003, false), (77, "PlasmaRifle", 2004, false),
            (78, "Shotgun", 2001, false), (79, "SuperShotgun", 82, false),
        ];
        return actors.Concat(hanging.Select(value => new DehackedActor {
            Index = value.Index, Name = value.Name, DoomEdNum = value.EditorNumber, OriginalDoomEdNum = value.EditorNumber,
            Health = 1000, Radius = value.Radius, Height = value.Height, Bits = 256u | 512u | (value.Solid ? 2u : 0u),
        })).Concat(props.Select(value => new DehackedActor {
            Index = value.Index, Name = value.Name, DoomEdNum = value.EditorNumber, OriginalDoomEdNum = value.EditorNumber,
            Health = 1000, Radius = value.Radius, Height = value.Height,
            Bits = value.EditorNumber == 34 ? 0u : 2u, ProjectilePassHeightRaw = -16 * 65536,
        })).Concat(bodies.Select(value => new DehackedActor {
            Index = value.Index, Name = value.Name, DoomEdNum = value.EditorNumber, OriginalDoomEdNum = value.EditorNumber,
            Health = 1000, Radius = 20, Height = 16, ProjectilePassHeightRaw = 0,
        })).Concat(debris.Select(value => new DehackedActor {
            Index = value.Index, Name = value.Name, DoomEdNum = value.EditorNumber, OriginalDoomEdNum = value.EditorNumber,
            Health = 1000, Radius = 20, Height = value.Height, Bits = 16, ProjectilePassHeightRaw = 0,
        })).Concat(pickups.Select(value => new DehackedActor {
            Index = value.Index, Name = value.Name, DoomEdNum = value.EditorNumber, OriginalDoomEdNum = value.EditorNumber,
            Health = 1000, Radius = 20, ReactionTime = 8,
            Height = value.EditorNumber == 2025 ? 46 : value.EditorNumber == 8 ? 26 : value.EditorNumber == 2006 ? 20 : 16,
            Bits = 1u | (value.CountItem ? 0x00800000u : 0u) | (value.Index is >= 48 and <= 53 ? 0x02000000u : 0u),
        })).OrderBy(actor => actor.Index).ToArray();
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

    private static void ApplyThing(DehackedActor actor, IReadOnlyList<(string Key, string Value)> body, List<string> errors,
        bool dsdhacked, HashSet<string> tentativeSounds)
    {
        actor.Patched = true;
        var oldShadow = actor.BitsPatched ? (actor.Bits & 0x40000) != 0 : actor.OriginalDoomEdNum == 58;
        var hadStyle = false;
        var hadTranslucency = false;
        var hadPhysicalHeight = false;
        var hadHeight = false;
        var hadProjectilePassHeight = false;
        foreach (var (key, value) in body)
        {
            if (key.Equals("Render Style", StringComparison.OrdinalIgnoreCase))
            {
                var name = value.Trim();
                if (name.StartsWith("STYLE_", StringComparison.OrdinalIgnoreCase) && name.Length > 6 && char.IsLetter(name[6]) &&
                    Enum.TryParse<DehackedRenderStyle>(name[6..], true, out var style) && Enum.IsDefined(style))
                {
                    actor.RenderStyle = style;
                    hadStyle = true;
                }
                else errors.Add($"Thing {actor.Index}: unknown render style '{value}'.");
                continue;
            }
            var numeric = ParseDecimalPrefix(value);
            if (numeric < int.MinValue || numeric > uint.MaxValue)
            {
                errors.Add($"Thing {actor.Index}: bad numeric constant {value} for {key}.");
                continue;
            }
            if (key.Equals("Translucency", StringComparison.OrdinalIgnoreCase))
            {
                actor.Alpha = (double)numeric / 65536;
                actor.RenderStyle = DehackedRenderStyle.Translucent;
                hadStyle = hadTranslucency = true;
            }
            else if (key.Equals("Hit points", StringComparison.OrdinalIgnoreCase))
            {
                actor.Health = unchecked((int)numeric);
                actor.HealthPatched = true;
            }
            else if (key.Equals("Reaction time", StringComparison.OrdinalIgnoreCase))
            {
                actor.ReactionTime = unchecked((int)numeric);
                actor.ReactionTimePatched = true;
            }
            else if (key.Equals("Pain chance", StringComparison.OrdinalIgnoreCase))
            {
                actor.PainChance = unchecked((short)numeric);
                actor.PainChancePatched = true;
            }
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
                hadHeight = true;
                if (!hadPhysicalHeight)
                {
                    actor.Height = (double)numeric / 65536.0;
                    actor.HeightPatched = true;
                }
                if (!hadProjectilePassHeight) actor.ProjectilePassHeightRaw = 0;
            }
            else if (key.Equals("Physical height", StringComparison.OrdinalIgnoreCase))
            {
                hadPhysicalHeight = true;
                if (numeric < int.MinValue || numeric > int.MaxValue)
                    errors.Add($"Thing {actor.Index}: Physical height {value} exceeds the represented fixed-point range.");
                else
                {
                    actor.Height = numeric / 65536.0;
                    actor.HeightPatched = true;
                }
            }
            else if (key.Equals("Projectile pass height", StringComparison.OrdinalIgnoreCase))
            {
                hadProjectilePassHeight = true;
                if (numeric < int.MinValue || numeric > int.MaxValue)
                    errors.Add($"Thing {actor.Index}: Projectile pass height {value} exceeds the represented fixed-point range.");
                else actor.ProjectilePassHeightRaw = (int)numeric;
            }
            else if (key.Equals("Width", StringComparison.OrdinalIgnoreCase))
            {
                actor.Radius = (double)numeric / 65536.0;
                actor.WidthPatched = true;
            }
            else if (key.Equals("Speed", StringComparison.OrdinalIgnoreCase))
            {
                var speed = (double)numeric;
                speed = Math.Abs(speed) >= 256 ? speed / 65536 : speed;
                if (actor.Index == 1 && (speed < int.MinValue / 65536.0 || speed > int.MaxValue / 65536.0))
                    errors.Add($"Thing {actor.Index}: Speed {value} exceeds the represented fixed-point range.");
                else
                {
                    actor.Speed = speed;
                    actor.SpeedPatched = true;
                }
            }
            else if (key.Equals("Fast speed", StringComparison.OrdinalIgnoreCase))
            {
                var speed = numeric >= 256 ? numeric / 65536.0 : numeric;
                if (speed > int.MaxValue / 65536.0)
                    errors.Add($"Thing {actor.Index}: Fast speed {value} exceeds the represented fixed-point range.");
                else actor.FastSpeed = speed;
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
            else if (key.Equals("Melee threshold", StringComparison.OrdinalIgnoreCase))
                actor.MeleeThreshold = numeric / 65536.0;
            else if (key.Equals("Max target range", StringComparison.OrdinalIgnoreCase))
                actor.MaxTargetRange = numeric / 65536.0;
            else if (key.Equals("Missile chance multiplier", StringComparison.OrdinalIgnoreCase))
                actor.MissileChanceMultiplier = numeric / 65536.0;
            else if (key.Equals("Min missile chance", StringComparison.OrdinalIgnoreCase))
            {
                var chance = numeric / 65536;
                if (chance is < 0 or > 255)
                    errors.Add($"Thing {actor.Index}: Min missile chance {value} exceeds the represented byte range.");
                else actor.MinMissileChance = (byte)chance;
            }
            else if (key.Equals("Melee range", StringComparison.OrdinalIgnoreCase))
            {
                var adjusted = numeric - 20L * 65536;
                if (adjusted < int.MinValue || adjusted > int.MaxValue)
                    errors.Add($"Thing {actor.Index}: Melee range {value} exceeds the represented fixed-point range.");
                else actor.MeleeRangeRaw = (int)adjusted;
            }
            else if (key.Equals("Bits", StringComparison.OrdinalIgnoreCase))
            {
                actor.FullVolumeSounds = false;
                actor.FullVolumeSoundsPatched = true;
                uint bits = 0;
                var changed = false;
                var useStealth = false;
                var changed2 = false;
                uint bits2 = 0;
                uint renderBits = 0;
                var changedRender = false;
                foreach (var token in value.Split([',', '+', '|', ' ', '\t', '\f', '\r'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (token.All(character => character is >= '0' and <= '9' or '-'))
                    {
                        bits |= unchecked((uint)ParseDecimalPrefix(token));
                        changed = true;
                    }
                    else if (token.Equals("FULLVOLSOUNDS", StringComparison.OrdinalIgnoreCase))
                        actor.FullVolumeSounds = true;
                    else if (token.Equals("DONTDRAW", StringComparison.OrdinalIgnoreCase))
                    {
                        renderBits |= 16;
                        changedRender = true;
                    }
                    else if (token.Equals("TRANSLUC25", StringComparison.OrdinalIgnoreCase) ||
                        token.Equals("TRANSLUC50", StringComparison.OrdinalIgnoreCase) ||
                        token.Equals("TRANSLUC75", StringComparison.OrdinalIgnoreCase))
                    {
                        renderBits |= token.Equals("TRANSLUC25", StringComparison.OrdinalIgnoreCase) ? 2u :
                            token.Equals("TRANSLUC50", StringComparison.OrdinalIgnoreCase) ? 4u : 8u;
                        changedRender = true;
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
                    if ((bits & 0x10000000) != 0)
                    {
                        actor.Touchy = true;
                        bits &= ~0x10000000u;
                    }
                    if ((bits & 0x80000000u) != 0)
                    {
                        bits &= ~0x80000000u;
                        renderBits |= 2;
                        changedRender = true;
                    }
                    actor.Bits = bits;
                    actor.BitsPatched = true;
                    actor.BitsUseStealth = useStealth;
                    if (!useStealth && (bits & 0x40000000) != 0) actor.NoBlockMonsters = true;
                }
                if (changedRender)
                {
                    if ((renderBits & 7) != 0)
                    {
                        actor.Alpha = (renderBits & 1) != 0 ? 0.25 : (renderBits & 2) != 0 ? 0.5 : 0.75;
                        actor.RenderStyle = DehackedRenderStyle.Translucent;
                        hadTranslucency = true;
                    }
                    actor.Invisible = (renderBits & 8) != 0;
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
                AssignSound(actor, key, unchecked((int)numeric), value, dsdhacked, tentativeSounds);
            else if (key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase))
                AssignState(actor, key, unchecked((int)numeric), errors);
        }
        if (hadPhysicalHeight != hadProjectilePassHeight)
            errors.Add($"Thing {actor.Index}: Physical height and Projectile pass height must be specified together in the same Thing block.");
        if ((actor.Bits & 256) != 0 && !hadHeight && !hadPhysicalHeight
            && DehackedOriginalHeights.Find(actor.Index) is { } originalHeight)
        {
            actor.Height = originalHeight;
            actor.HeightPatched = true;
            actor.ProjectilePassHeightRaw = 0;
        }
        var shadow = actor.BitsPatched ? (actor.Bits & 0x40000) != 0 : actor.OriginalDoomEdNum == 58;
        if (shadow != oldShadow)
        {
            if (!hadStyle) actor.RenderStyle = shadow ? DehackedRenderStyle.OptFuzzy : DehackedRenderStyle.Normal;
            if (shadow && !hadTranslucency) actor.Alpha = 0.5;
        }
    }

    private static void AssignSound(DehackedActor actor, string key, int id, string value,
        bool dsdhacked, HashSet<string> tentativeSounds)
    {
        var selector = key.StartsWith("Alert", StringComparison.OrdinalIgnoreCase) ? 0
            : key.StartsWith("Attack", StringComparison.OrdinalIgnoreCase) ? 1
            : key.StartsWith("Pain", StringComparison.OrdinalIgnoreCase) ? 2
            : key.StartsWith("Death", StringComparison.OrdinalIgnoreCase) ? 3
            : key.StartsWith("Action", StringComparison.OrdinalIgnoreCase) ? 4
            : key.StartsWith("Rip", StringComparison.OrdinalIgnoreCase) ? 11 : -1;
        var text = value.Trim();
        var numericStart = text.Length > 0 && (char.IsAsciiDigit(text[0]) ||
            text.Length > 1 && text[0] is '+' or '-' && char.IsAsciiDigit(text[1]));
        if (selector >= 0)
        {
            if (!numericStart && text.Length != 0)
                actor.SoundNameOverrides[selector] = text.Equals("None", StringComparison.OrdinalIgnoreCase) ? "" : text;
            else if (numericStart)
            {
                var sound = DehackedSoundCatalog.Resolve(id, dsdhacked);
                actor.SoundNameOverrides[selector] = sound;
                if (sound.StartsWith("~dsdhacked/#", StringComparison.Ordinal)) tentativeSounds.Add(sound);
            }
            else actor.SoundNameOverrides[selector] = "";
        }
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
        else if (key.StartsWith("Rip", StringComparison.OrdinalIgnoreCase))
            actor.RipSound = id;
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

    private static void ApplySoundNames(Dictionary<int, DehackedSound> sounds, Dictionary<string, string> soundLumps,
        IReadOnlyList<(string Key, string Value)> body, bool dsdhacked)
    {
        foreach (var (key, value) in body)
        {
            var index = unchecked((int)ParseDecimalPrefix(key));
            var logicalName = DehackedSoundCatalog.Resolve(index, dsdhacked);
            if (logicalName.Length == 0) continue;
            // Native ReplaceSoundName prefixes the supplied suffix unconditionally.
            soundLumps[logicalName] = "ds" + value.Trim();
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
            ["NOSECTOR"] = 8,
            ["SPAWNCEILING"] = 256, ["NOGRAVITY"] = 512, ["DROPOFF"] = 1024,
            ["PICKUP"] = 2048, ["FLOAT"] = 16384, ["DROPPED"] = 131072,
            ["NOCLIP"] = 4096,
            ["JUSTHIT"] = 64, ["JUSTATTACKED"] = 128,
            ["INFLOAT"] = 0x00200000,
            ["NOTDMATCH"] = 0x02000000,
            ["CORPSE"] = 0x00100000,
            ["SHADOW"] = 262144, ["TRANSLUCENT"] = 0x80000000u,
            ["NOBLOOD"] = 524288,
            ["COUNTKILL"] = 4194304,
            ["TOUCHY"] = 0x10000000,
            ["FRIEND"] = 0x40000000, ["STEALTH"] = 0x40000000,
        };

    private static readonly IReadOnlyDictionary<string, uint> SupportedThingBits2 =
        new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            ["LOGRAV"] = 1, ["NOTELEPORT"] = 0x80, ["CANSLIDE"] = 0x400, ["INVULNERABLE"] = 0x08000000,
            ["DORMANT"] = 0x10000000, ["PASSMOBJ"] = 0x1000, ["FLOATBOB"] = 0x00080000,
            ["SEEKERMISSILE"] = 0x40000000,
            ["REFLECTIVE"] = 0x80000000u,
            ["BOSS"] = 0x00008000,
            ["THRUGHOST"] = 0x00004000,
            ["NONSHOOTABLE"] = 0x04000000,
            ["CANTLEAVEFLOORPIC"] = 0x02000000,
            ["RIP"] = 0x00000100,
            ["PUSHABLE"] = 0x00000200, ["CANNOTPUSH"] = 0x00002000,
            ["TELESTOMP"] = 0x00040000,
            ["FLOORCLIP"] = 0x00000020,
            ["FLY"] = 0x00000010,
            ["SPAWNFLOAT"] = 0x00000040,
            ["BLASTED"] = 0x00000008,
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
