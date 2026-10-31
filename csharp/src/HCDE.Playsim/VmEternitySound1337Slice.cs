using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Eternity spatial-audio facade from <c>s_sound_eternity.cpp</c> (1-331).
/// Chunk 1337. The renderer records source and listener parameters and
/// stays silent: starts return null and streams return null.
/// </summary>
public sealed class VmEternitySound1337
{
    public enum InactiveState
    {
        Active = 0,
        Complete = 1,
        Mute = 2,
    }

    public bool RegistrationAttempted { get; private set; }
    public bool RendererConstructed { get; private set; }
    public int DiagnosticTriggers { get; private set; }
    public Renderer? Current { get; private set; }
    public List<string> Printed { get; } = new();

    public Renderer Create()
    {
        RegistrationAttempted = true;
        Current = new Renderer(this);
        Print("snd_backend=eternity selected: using silent Eternity spatial facade (mixer not vendored yet).\n");
        return Current;
    }

    public Renderer CreateStub()
    {
        return Create();
    }

    public void Probe()
    {
        DiagnosticTriggers++;
        if (Current != null)
            Current.StartSound(1f, 1f, 0);
        Print("Eternity spatial audio diagnostic trigger submitted.\n");
    }

    public void Status(string? backend)
    {
        var selected = backend != null && AsciiEqual(backend, "eternity");
        Print("Eternity spatial audio backend: " + (selected ? "selected" : "not selected") + "\n");
        var shown = backend != null && backend.Length > 0 ? backend : "<empty>";
        Print("  snd_backend = " + shown + "\n");
        Print("  registration attempted = " + (RegistrationAttempted ? "yes" : "no") + "\n");
        Print("  renderer constructed = " + (RendererConstructed ? "yes" : "no") + "\n");
        Print("  implementation = silent facade (Eternity mixer not vendored yet)\n");
        Print("  diagnostic triggers = " + DiagnosticTriggers + "\n");
        if (Current != null)
        {
            var spatial = Current;
            Print("  loaded sounds = " + spatial.LoadedSounds + "\n");
            Print("  2D starts = " + spatial.Count2D + ", 3D starts = " + spatial.Count3D + "\n");
            Print("  listener updates = " + spatial.ListenerUpdates + ", 3D param updates = " + spatial.ParamUpdates3D + ", mix ticks = " + spatial.MixTicks + "\n");
            Print(
                "  last spatial submit: pos=(" + F2(spatial.LastPosX) + " " + F2(spatial.LastPosY) + " " + F2(spatial.LastPosZ)
                + ") vel=(" + F2(spatial.LastVelX) + " " + F2(spatial.LastVelY) + " " + F2(spatial.LastVelZ)
                + ") vol=" + F3(spatial.LastVolume) + " pitch=" + F3(spatial.LastPitch)
                + " channel=" + spatial.LastChannel + " flags=" + spatial.LastChannelFlags
                + " priority=" + spatial.LastPriority + " area=" + (spatial.LastAreaSound ? 1 : 0) + "\n");
        }

        Print("  boundary = client-local audio backend; no snapshots, saves, demos, or authority state\n");
        Print("  note: Eternity facade is silent; use snd_backend=openal for full audio with snd_env_reverb support\n");
    }

    internal void MarkConstructed()
    {
        RendererConstructed = true;
    }

    internal void Print(string line)
    {
        Printed.Add(line);
    }

    static bool AsciiEqual(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (var i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }

        return true;
    }

    static char Fold(char c)
    {
        return c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
    }

    static string F2(float value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    static string F3(float value) => value.ToString("0.000", CultureInfo.InvariantCulture);

    public sealed class Renderer
    {
        readonly VmEternitySound1337 host;

        public Renderer(VmEternitySound1337 host)
        {
            this.host = host;
            host.MarkConstructed();
        }

        public int Count2D { get; private set; }
        public int Count3D { get; private set; }
        public int ListenerUpdates { get; private set; }
        public int ParamUpdates3D { get; private set; }
        public float LastVolume { get; private set; }
        public float LastPitch { get; private set; }
        public float LastDistanceScale { get; private set; }
        public float LastPosX { get; private set; }
        public float LastPosY { get; private set; }
        public float LastPosZ { get; private set; }
        public float LastVelX { get; private set; }
        public float LastVelY { get; private set; }
        public float LastVelZ { get; private set; }
        public int LastChannel { get; private set; } = -1;
        public int LastChannelFlags { get; private set; }
        public int LastPriority { get; private set; }
        public bool LastAreaSound { get; private set; }
        public int LoadedSounds { get; private set; }
        public int StreamRequests { get; private set; }
        public int MixTicks { get; private set; }
        public float SfxVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 1f;
        public bool SyncActive { get; private set; }
        public bool SfxPaused { get; private set; }
        public InactiveState Inactive { get; private set; } = InactiveState.Active;

        public bool IsNull()
        {
            return false;
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = volume;
        }

        public void UpdateMusicParams()
        {
        }

        public object? LoadSound()
        {
            LoadedSounds++;
            return null;
        }

        public object? LoadSoundRaw()
        {
            LoadedSounds++;
            return null;
        }

        public void UnloadSound()
        {
        }

        public uint GetMsLength()
        {
            return 250;
        }

        public uint GetSampleLength()
        {
            return 0;
        }

        public float GetOutputRate()
        {
            return 44100f;
        }

        public object? CreateStream()
        {
            StreamRequests++;
            return null;
        }

        public object? StartSound(float vol, float pitch, int chanflags)
        {
            Count2D++;
            LastVolume = vol;
            LastPitch = pitch;
            LastChannelFlags = chanflags;
            return null;
        }

        public object? StartSound3D(
            float vol,
            float distscale,
            float pitch,
            int priority,
            double posX,
            double posY,
            double posZ,
            double velX,
            double velY,
            double velZ,
            int channum,
            int chanflags)
        {
            Count3D++;
            LastVolume = vol;
            LastPitch = pitch;
            LastDistanceScale = distscale;
            LastPosX = (float)posX;
            LastPosY = (float)posY;
            LastPosZ = (float)posZ;
            LastVelX = (float)velX;
            LastVelY = (float)velY;
            LastVelZ = (float)velZ;
            LastChannel = channum;
            LastChannelFlags = chanflags;
            LastPriority = priority;
            return null;
        }

        public void StopChannel()
        {
        }

        public void ChannelVolume(float volume)
        {
            LastVolume = volume;
        }

        public void ChannelPitch(float pitch)
        {
            LastPitch = pitch;
        }

        public void MarkStartTime()
        {
        }

        public uint GetPosition()
        {
            return 0;
        }

        public float GetAudibility()
        {
            return 0f;
        }

        public void Sync(bool sync)
        {
            SyncActive = sync;
        }

        public void SetSfxPaused(bool paused)
        {
            SfxPaused = paused;
        }

        public void SetInactive(InactiveState inactive)
        {
            Inactive = inactive;
        }

        public void UpdateSoundParams3D(bool areasound, double posX, double posY, double posZ, double velX, double velY, double velZ)
        {
            ParamUpdates3D++;
            LastAreaSound = areasound;
            LastPosX = (float)posX;
            LastPosY = (float)posY;
            LastPosZ = (float)posZ;
            LastVelX = (float)velX;
            LastVelY = (float)velY;
            LastVelZ = (float)velZ;
        }

        public void UpdateListener()
        {
            ListenerUpdates++;
        }

        public void UpdateSounds()
        {
            MixTicks++;
        }

        public bool IsValid()
        {
            return true;
        }

        public void PrintStatus()
        {
            host.Print("Eternity spatial audio facade active (silent mixer placeholder).\n");
            host.Print(
                "  loaded=" + LoadedSounds + " 2d-starts=" + Count2D + " 3d-starts=" + Count3D
                + " listener-updates=" + ListenerUpdates + " param-updates=" + ParamUpdates3D
                + " mix-ticks=" + MixTicks + "\n");
        }

        public void PrintDriversList()
        {
            host.Print("Eternity spatial audio facade has no platform drivers yet.\n");
        }

        public string GatherStats()
        {
            return "Eternity facade: loaded=" + LoadedSounds + " 2d=" + Count2D + " 3d=" + Count3D
                + " listener=" + ListenerUpdates + " params=" + ParamUpdates3D
                + " streams=" + StreamRequests + " mix=" + MixTicks;
        }
    }
}
