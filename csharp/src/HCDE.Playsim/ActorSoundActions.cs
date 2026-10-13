namespace HCDE.Playsim;

public static class ActorSoundActions
{
    public static void CrushPain(Actor actor)
    {
        if (actor.Invulnerable || actor.Dormant
            || !actor.ActorSounds.TryGetValue(ActorSoundType.CrushPain, out var sound)
            || string.IsNullOrEmpty(sound) || sound.Equals("None", StringComparison.OrdinalIgnoreCase)) return;
        StartSoundCore(actor, sound, 2, 0, 1, 1, 0, 0, onlyIfSoundNotPlaying: true, checkAllSoundChannels: true);
    }

    public static void StartGlobalSound(AuthoritySimulation sim, string sound, int channel = 4,
        SimulationSoundFlags flags = 0, float volume = 1, float attenuation = 1, float pitch = 0, float startTime = 0)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(sound);
        if (!float.IsFinite(volume)) throw new ArgumentOutOfRangeException(nameof(volume));
        if (!float.IsFinite(attenuation)) throw new ArgumentOutOfRangeException(nameof(attenuation));
        if (!float.IsFinite(pitch)) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (!float.IsFinite(startTime)) throw new ArgumentOutOfRangeException(nameof(startTime));
        if (startTime < 0 && (flags & SimulationSoundFlags.Loop) != 0)
            throw new NotSupportedException("Negative loop start offsets are not yet supported.");
        if (sound.Length == 0) return;
        sim.QueueSoundRequest(new SimulationSoundRequest(0, sound, channel, (int)flags, volume, false,
            attenuation, false, Pitch: pitch > 0 ? pitch : null, StartTime: Math.Max(0, startTime),
            NonPositional: true), immediate: true);
    }

    public static void StartSoundAt(AuthoritySimulation sim, double x, double y, double z, string sound,
        int channel = 4, SimulationSoundFlags flags = 0, float volume = 1, float attenuation = 1,
        float pitch = 0, float startTime = 0)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(sound);
        if (!float.IsFinite((float)x)) throw new ArgumentOutOfRangeException(nameof(x));
        if (!float.IsFinite((float)y)) throw new ArgumentOutOfRangeException(nameof(y));
        if (!float.IsFinite((float)z)) throw new ArgumentOutOfRangeException(nameof(z));
        if (!float.IsFinite(volume)) throw new ArgumentOutOfRangeException(nameof(volume));
        if (!float.IsFinite(attenuation)) throw new ArgumentOutOfRangeException(nameof(attenuation));
        if (!float.IsFinite(pitch)) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (!float.IsFinite(startTime)) throw new ArgumentOutOfRangeException(nameof(startTime));
        if (startTime < 0 && (flags & SimulationSoundFlags.Loop) != 0)
            throw new NotSupportedException("Negative loop start offsets are not yet supported.");
        if (sound.Length == 0) return;
        sim.QueueSoundRequest(new SimulationSoundRequest(0, sound, channel, (int)flags, volume, false,
            attenuation, false, Pitch: pitch > 0 ? pitch : null, StartTime: Math.Max(0, startTime),
            DetachedOrigin: new SimulationSoundOrigin((float)x, (float)z, (float)y, -1)), immediate: true);
    }

    public static void PlaySpawnSound(Actor missile, Actor? spawner = null, bool? spawnSoundSource = null)
    {
        ArgumentNullException.ThrowIfNull(missile);
        if (missile.Destroyed || !missile.ActorSounds.TryGetValue(ActorSoundType.See, out var sound)
            || string.IsNullOrEmpty(sound)) return;
        var sim = missile.Simulation ?? throw new InvalidOperationException("Spawn sound requires an attached simulation.");
        if (!(spawnSoundSource ?? missile.SpawnSoundSource))
        {
            StartSound(missile, sound, channel: 2);
            return;
        }
        if (spawner is not null)
        {
            if (spawner.Simulation != sim)
                throw new InvalidOperationException("Spawn sound source must belong to the missile simulation.");
            StartSound(spawner, sound, channel: 1);
            return;
        }
        var sector = ActorPhysics.SectorAt(sim.Level, missile.X.ToDouble(), missile.Y.ToDouble());
        if ((uint)sector < (uint)sim.Level.Sectors.Count && sim.Level.Sectors[sector].Silent) return;
        var position = missile.SoundPos();
        sim.QueueSoundRequest(new SimulationSoundRequest(0, sound, 1, 0, 1, false, 1, false,
            DetachedOrigin: new SimulationSoundOrigin(position.X, position.Y, position.Z, sector)), immediate: true);
    }

    private static bool IsSoundRegistered(HCDE.MapLoader.PlayLevel level, string name) =>
        level.SoundDefinitions.Keys.Concat(level.SoundAliases.Keys).Concat(level.RandomSoundGroups.Keys)
            .Concat(level.SoundSettings.Keys).Any(key => key.Equals(name, StringComparison.OrdinalIgnoreCase));
    public static void DefaultDeathSound(Actor actor)
    {
        if (actor is PlayerPawn) PlayerScream(actor);
        else Scream(actor);
    }

    public static void PlayerScream(Actor actor)
    {
        if (actor.Destroyed || actor.Simulation is not { } sim) return;
        var channel = 2;
        var sound = actor.ActorSounds.GetValueOrDefault(ActorSoundType.Death);
        if (string.IsNullOrEmpty(sound))
        {
            sound = "*death";
            if (actor is PlayerPawn player)
            {
                bool Defined(string name) => IsSoundRegistered(sim.Level, name);
                string? Variant(string name)
                {
                    var type = string.IsNullOrEmpty(player.LastDamageType)
                        || player.LastDamageType.Equals("None", StringComparison.OrdinalIgnoreCase)
                        ? "None" : player.LastDamageType;
                    if (Defined(name + "-" + type)) return name + "-" + type;
                    return Defined(name) ? name : null;
                }
                string? selected = null;
                if (sim.FallingDeathSplatEnabled && actor.VelocityZ.ToDouble() <= -39)
                {
                    selected = Defined("*splat") ? "*splat" : null;
                    channel = 4;
                }
                if (selected is null && actor.DeathDamageAmount < 10) selected = Variant("*wimpydeath");
                if (selected is null && actor.Health <= -50)
                {
                    if (actor.Health > -100) selected = Variant("*crazydeath");
                    selected ??= Variant("*xdeath");
                    if (selected is null) { selected = Variant("*gibbed"); channel = 4; }
                }
                sound = selected ?? Variant("*death") ?? "*death";
            }
        }
        if (channel != 2)
            foreach (var slot in new[] { 0, 3, 4, 5, 6, 7 }) StopSound(actor, slot);
        StartSound(actor, sound, channel, SimulationSoundFlags.NoRumble);
    }

    public static void SoundPitch(Actor actor, int channel, float pitch, string? sound = null)
    {
        if (!float.IsFinite(pitch)) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Changing actor sound pitch requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound ?? "", channel, 0, 0, false, 0, false,
            Pitch: pitch, ChangePitch: true), immediate: true);
    }

    private static int LegacySoundSlot(string slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        return slot.ToUpperInvariant() switch
        {
            "WEAPON" => 1, "VOICE" => 2, "ITEM" => 3, "BODY" => 4,
            "SOUNDSLOT5" => 5, "SOUNDSLOT6" => 6, "SOUNDSLOT7" => 7, _ => 0,
        };
    }

    public static void PlaySoundEx(Actor actor, string sound, string slot, bool looping = false, int attenuation = 0)
    {
        var channel = LegacySoundSlot(slot);
        var distanceScale = attenuation switch { -1 => 3f, 1 or 2 => 0f, _ => 1f };
        StartSoundCore(actor, sound, channel, looping ? SimulationSoundFlags.Loop : 0,
            1, distanceScale, 0, 0, onlyIfSoundNotPlaying: looping);
    }

    public static void StopSoundEx(Actor actor, string slot)
    {
        var channel = LegacySoundSlot(slot);
        if (channel != 0) StopSound(actor, channel);
    }

    public static void PlaySound(Actor actor, string sound = "weapons/pistol", int channel = 4,
        float volume = 1, bool looping = false, float attenuation = 1, bool local = false, float pitch = 0)
    {
        if (looping) channel |= (int)(SimulationSoundFlags.Loop | SimulationSoundFlags.NoStop);
        if (local) channel |= (int)SimulationSoundFlags.Local;
        StartSound(actor, sound, channel & 7, (SimulationSoundFlags)(channel & ~7), volume, attenuation, pitch);
    }

    public static void StartSound(Actor actor, string sound, int channel = 4, SimulationSoundFlags flags = 0,
        float volume = 1, float attenuation = 1, float pitch = 0, float startTime = 0)
        => StartSoundCore(actor, sound, channel, flags, volume, attenuation, pitch, startTime);

    public static void StartSoundIfNotSame(Actor actor, string sound, string checkAgainst, int channel = 4,
        SimulationSoundFlags flags = 0, float volume = 1, float attenuation = 1, float pitch = 0, float startTime = 0)
    {
        ArgumentNullException.ThrowIfNull(checkAgainst);
        ArgumentNullException.ThrowIfNull(sound);
        if (sound.Equals(checkAgainst, StringComparison.OrdinalIgnoreCase)) return;
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Sound equivalence requires an attached simulation.");
        string? Identity(string name)
        {
            if (name.Length == 0 || name.Equals("None", StringComparison.OrdinalIgnoreCase)) return null;
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (visited.Add(name))
            {
                var random = sim.Level.RandomSoundGroups.Keys.FirstOrDefault(key => key.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (random is not null) return random;
                var alias = sim.Level.SoundAliases.FirstOrDefault(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (alias.Key is not null) { name = alias.Value; continue; }
                return sim.Level.SoundDefinitions.Keys.Concat(sim.Level.SoundSettings.Keys)
                    .FirstOrDefault(key => key.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            throw new InvalidOperationException("Sound equivalence encountered an alias cycle.");
        }
        var identity = Identity(sound); var checkIdentity = Identity(checkAgainst);
        if (string.Equals(identity, checkIdentity, StringComparison.OrdinalIgnoreCase)) return;
        StartSoundCore(actor, sound, channel, flags, volume, attenuation, pitch, startTime);
    }

    private static void StartSoundCore(Actor actor, string sound, int channel, SimulationSoundFlags flags,
        float volume, float attenuation, float pitch, float startTime, bool onlyIfSoundNotPlaying = false,
        bool checkAllSoundChannels = false)
    {
        ArgumentNullException.ThrowIfNull(sound);
        if (!float.IsFinite(volume)) throw new ArgumentOutOfRangeException(nameof(volume));
        if (!float.IsFinite(attenuation)) throw new ArgumentOutOfRangeException(nameof(attenuation));
        if (!float.IsFinite(pitch)) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (!float.IsFinite(startTime)) throw new ArgumentOutOfRangeException(nameof(startTime));
        if (startTime < 0 && (flags & SimulationSoundFlags.Loop) != 0)
            throw new NotSupportedException("Negative loop start offsets are not yet supported.");
        startTime = Math.Max(0, startTime);
        if (actor.Destroyed || sound.Length == 0) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Starting actor sounds requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, channel, (int)flags, volume,
            false, attenuation, false, Pitch: pitch > 0 ? pitch : null, StartTime: startTime,
            OnlyIfSoundNotPlaying: onlyIfSoundNotPlaying, CheckAllSoundChannels: checkAllSoundChannels), immediate: true);
    }

    public static void PlayWeaponSound(Actor actor, string sound, bool fullVolume = false)
    {
        StartSound(actor, sound, 1, attenuation: fullVolume ? 0 : 1);
    }

    public static void SoundVolume(Actor actor, int channel, float volume)
    {
        if (!float.IsFinite(volume)) throw new ArgumentOutOfRangeException(nameof(volume), "Sound volume must be finite.");
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Changing actor sound volume requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, "", channel, 0, Math.Clamp(volume, 0, 1),
            false, 0, false, ChangeVolume: true), immediate: true);
    }

    public static void StopAllSounds(Actor actor) => StopSounds(actor, 0, 0);

    public static void StopSounds(Actor actor, int channelMin, int channelMax)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Stopping actor sounds requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, "", channelMin, 0, 0, false, 0, false,
            Stop: true, StopChannelMax: channelMax), immediate: true);
    }

    public static void StopSound(Actor actor) => StopSound(actor, 2);

    public static void StopSound(Actor actor, int channel, string? sound = null)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Stopping actor sounds requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound ?? "", channel, 0, 0, false, 0, false, Stop: true), immediate: true);
    }

    public static void Howl(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Howl, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Howl sound requires an attached simulation.");
        // Native Howl leaves an already playing copy on the body channel alone.
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 4, 0,
            1, false, 1, false, OnlyIfSoundNotPlaying: true), immediate: true);
    }

    public static void PlayPushSound(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Push, out var sound) || string.IsNullOrEmpty(sound)) return;
        StartSoundCore(actor, sound, 4, 0, 1, 1, 0, 0, onlyIfSoundNotPlaying: true);
    }

    /// <summary>Configured A_Look sight sound emission; target acquisition remains separate.</summary>
    public static void SeeSound(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.See, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("See sound requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, 0,
            1, false, actor.Boss || actor.FullVolumeSee ? 0 : 1, false), immediate: true);
    }

    /// <summary>A_XScream logical sound selection. Player skin substitution remains separate.</summary>
    public static void XScream(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Extreme death sound requires an attached simulation.");
        var sound = actor is PlayerPawn ? "*gibbed" : "misc/gibbed";
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, (int)SimulationSoundFlags.NoRumble,
            1, false, 1, false), immediate: true);
    }

    public static void ActiveSound(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Active, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Active sound requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, (int)SimulationSoundFlags.NoRumble,
            1, false, 1, false), immediate: true);
    }

    public static void PlayActiveSound(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Active, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Active sound requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, 0, 1, false,
            actor.FullVolumeActive ? 0 : 1.001f, false, OnlyIfChannelFree: true), immediate: true);
    }

    /// <summary>A_Scream with boss and FullvolDeath attenuation.</summary>
    public static void Scream(Actor actor)
    {
        if (actor.Destroyed) return;
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Death, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Death sound requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, (int)SimulationSoundFlags.NoRumble,
            1, false, actor.Boss || actor.FullVolumeDeath ? 0 : 1, false), immediate: true);
    }

    /// <summary>A_Pain logical selection. Player skin resolution remains separate.</summary>
    public static void Pain(Actor actor)
    {
        if (actor.Destroyed) return;
        if (actor is PlayerPawn)
        {
            // Standalone players support damage calculations without a world or audio queue.
            if (actor.Simulation is not { } simulation) return;
            var pain = actor.Health < 25 ? "*pain25" : actor.Health < 50 ? "*pain50"
                : actor.Health < 75 ? "*pain75" : "*pain100";
            if ((actor.PainDamageTypeContext ?? ((PlayerPawn)actor).LastDamageType) is { Length: > 0 } type && !type.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                var specific = pain + "-" + type;
                var generic = "*pain-" + type;
                var level = simulation.Level;
                bool IsDefined(string name) => IsSoundRegistered(level, name);
                if (IsDefined(specific)) pain = specific;
                else if (IsDefined(generic)) pain = generic;
            }
            simulation.QueueSoundRequest(new SimulationSoundRequest(actor.Id, pain, 2, (int)SimulationSoundFlags.NoRumble,
                1, false, 1, false), immediate: true);
            return;
        }
        if (!actor.ActorSounds.TryGetValue(ActorSoundType.Pain, out var sound) || string.IsNullOrEmpty(sound)) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Pain sound requires an attached simulation.");
        sim.QueueSoundRequest(new SimulationSoundRequest(actor.Id, sound, 2, 0, 1, false, 1, false), immediate: true);
    }
}
