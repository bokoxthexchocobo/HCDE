namespace HCDE.Gamedata;

public sealed class DehackedActor
{
    internal DehackedActor Copy() => (DehackedActor)MemberwiseClone();
    public int Index { get; init; }
    public string Name { get; init; } = "";
    public int Health { get; set; }
    public double Speed { get; set; }
    public double Radius { get; set; }
    public bool WidthPatched { get; set; }
    public double Height { get; set; }
    public bool HeightPatched { get; set; }
    public int MissileDamage { get; set; }
    public bool MissileDamagePatched { get; set; }
    public int ReactionTime { get; set; }
    public bool ReactionTimePatched { get; set; }
    public int Mass { get; set; }
    public bool MassPatched { get; set; }
    public int PainChance { get; set; }
    public int DoomEdNum { get; set; }
    public int OriginalDoomEdNum { get; init; }
    public uint Bits { get; set; }
    public bool BitsPatched { get; set; }
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
        ];
        var actors = new DehackedActor[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            actors[i] = new DehackedActor
            {
                Index = i + 1,
                Name = names[i],
                Health = names[i] == "Player" ? 100 : 30,
                DoomEdNum = i switch { 0 => 1, 1 => 3004, 2 => 9, 3 => 64, 5 => 66, 8 => 67, 10 => 65, 11 => 3001, _ => -1 },
                OriginalDoomEdNum = i switch { 0 => 1, 1 => 3004, 2 => 9, 3 => 64, 5 => 66, 8 => 67, 10 => 65, 11 => 3001, _ => -1 },
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
            if (key.Equals("Hit points", StringComparison.OrdinalIgnoreCase))
                actor.Health = ParseInt(value);
            else if (key.Equals("Reaction time", StringComparison.OrdinalIgnoreCase))
            {
                actor.ReactionTime = ParseInt(value);
                actor.ReactionTimePatched = true;
            }
            else if (key.Equals("Pain chance", StringComparison.OrdinalIgnoreCase))
                actor.PainChance = unchecked((short)ParseInt(value));
            else if (key.Equals("Height", StringComparison.OrdinalIgnoreCase))
            {
                actor.Height = ParseInt(value) / 65536.0;
                actor.HeightPatched = true;
            }
            else if (key.Equals("Width", StringComparison.OrdinalIgnoreCase))
            {
                actor.Radius = ParseInt(value) / 65536.0;
                actor.WidthPatched = true;
            }
            else if (key.Equals("Speed", StringComparison.OrdinalIgnoreCase))
            {
                var speed = (double)ParseInt(value);
                actor.Speed = Math.Abs(speed) >= 256 ? speed / 65536 : speed;
            }
            else if (key.Equals("Missile damage", StringComparison.OrdinalIgnoreCase))
            {
                actor.MissileDamage = ParseInt(value);
                actor.MissileDamagePatched = true;
            }
            else if (key.Equals("Mass", StringComparison.OrdinalIgnoreCase))
            {
                actor.Mass = ParseInt(value);
                actor.MassPatched = true;
            }
            else if (key.Equals("Bits", StringComparison.OrdinalIgnoreCase))
            {
                actor.Bits = (uint)ParseLong(value);
                actor.BitsPatched = true;
            }
            else if (key.Equals("ID #", StringComparison.OrdinalIgnoreCase))
                actor.DoomEdNum = ParseInt(value);
            else if (key.EndsWith(" sound", StringComparison.OrdinalIgnoreCase))
                AssignSound(actor, key, value);
            else if (key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase))
                AssignState(actor, key, ParseInt(value), errors);
        }
    }

    private static void AssignSound(DehackedActor actor, string key, string value)
    {
        var id = ParseInt(value);
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
            var number = ParseInt(value);
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

    private static int ParseInt(string value) => (int)ParseLong(value);

    private static long ParseLong(string value)
    {
        var trimmed = value.Trim();
        return long.TryParse(trimmed, out var number) ? number : 0;
    }
}
