namespace HCDE.Playsim;

/// <summary>Hosted state and seams for <c>SoundEngine::StartSound</c> (<c>s_sound.cpp</c> 398–639).</summary>
public sealed class StartSoundContext
{
    public const int SourceNone = 0;
    public const int SourceUnattached = 2;
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int ChanfOverlap = 4;
    public const int ChanAuto = -1;

    public int Type { get; set; }
    public object? Source { get; set; }
    public bool HasPoint { get; set; }
    public int Channel { get; set; }
    public int ChanFlags { get; set; }
    public int SoundId { get; set; }
    public int OrgSoundId { get; set; }
    public float Volume { get; set; }
    public float Attenuation { get; set; }
    public bool ForcedRolloffPresent { get; set; }
    public float ForcedRolloffMin { get; set; }
    public float SfxVolume { get; set; }
    public float SfxAttenuation { get; set; }
    public bool SfxSingular { get; set; }
    public bool RequestSingular { get; set; }
    public int NearLimit { get; set; }
    public float LimitRange { get; set; }
    public float DefPitch { get; set; }
    public float DefPitchMax { get; set; }
    public float RolloffMinDistance { get; set; }
    public float GlobalRolloffMin { get; set; }
    public float Spitch { get; set; }
    public float StartTime { get; set; }
    public bool NoSfx { get; set; }
    public bool BlockNewSounds { get; set; }
    public bool SoundPaused { get; set; }
    public bool SourceIsListener { get; set; }
    public int SfxEmptyLump { get; set; }
    public int LoadedLump { get; set; }
    public bool LimitReached { get; set; }
    public bool SingularAlreadyPlaying { get; set; }
    public int? ResultChannelId { get; set; }

    public Func<bool>? SfxHasLink { get; set; }
    /// <summary>Resolve one link step; updates sound id and limit fields. False aborts <c>StartSound</c>.</summary>
    public Func<bool>? ResolveNextSfxLink { get; set; }

    public Func<int, bool>? IsValidSoundId { get; init; }
    public Func<bool>? SoundEnabled { get; init; }
    public Func<bool>? ValidatePosVel { get; init; }
    public Func<bool>? CheckSingular { get; init; }
    public Func<bool>? CheckSoundLimit { get; init; }
    public Func<int, int>? LoadSoundLump { get; init; }
    public Func<bool>? IsChannelUsed { get; init; }
    public Func<int, object?>? StartSound2D { get; init; }
    public Func<object?>? StartSound3D { get; init; }
    public Func<object?, object?>? GetChannel { get; init; }
    public Action<object?>? MarkStartTime { get; init; }
    public Action<int>? StopMatchingChannel { get; init; }
    public Action<string>? JoyRumble { get; init; }
    public Func<float, float, float, float>? CalcPitch { get; init; }
    public Func<int, float>? GetMsLength { get; init; }
    public Func<int, string>? GetSoundName { get; init; }
}
