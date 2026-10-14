namespace HCDE.Playsim;

[Flags]
public enum SimulationSoundFlags
{
    ListenerZ = 8,
    MaybeLocal = 16,
    Ui = 32,
    NoPause = 64,
    Force = 65536,
    Loop = 256,
    NoStop = 4096,
    Overlap = 8192,
    Local = 16384,
    Singular = 131072,
    NoRumble = 524288,
}

public readonly record struct SimulationSoundOrigin(float X, float Y, float Z, int SectorIndex);

public sealed record SimulationSoundRequest(uint ActorId, string Sound, int Channel, int Flags,
    float Volume, bool Loop, float Attenuation, bool Local, bool Stop = false, bool ChangeVolume = false,
    int? StopChannelMax = null, float? Pitch = null, float StartTime = 0, bool ChangePitch = false,
    bool OnlyIfChannelFree = false, SimulationSoundOrigin? DetachedOrigin = null,
    bool OnlyIfSoundNotPlaying = false, bool NonPositional = false, bool CheckAllSoundChannels = false,
    bool PickupSound = false, bool ConsolePlayerOnly = false);
