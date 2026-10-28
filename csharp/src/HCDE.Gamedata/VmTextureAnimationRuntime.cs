namespace HCDE.Gamedata;

public sealed class VmDoorAnimationEntry
{
    public string BaseTexture { get; set; } = "";
    public List<string> FrameTextures { get; } = [];
}

public sealed class VmStandaloneAnimationState
{
    public bool Ok { get; set; }
    public int AnimIndex { get; set; }
    public int CurFrame { get; set; }
    public double SwitchTic { get; set; }
    public string AnimType { get; set; } = "Forward";
}

public static class VmTextureAnimationRuntime
{
    public const double MsPerTic = 1000.0 / 35.0;

    public static void ApplyNoDecalFlagsToRange(IReadOnlyList<string> frameTextures, bool noDecals)
    {
        _ = noDecals;
        _ = frameTextures;
    }

    public static VmDoorAnimationEntry? FindAnimatedDoor(IReadOnlyList<VmDoorAnimationEntry> doors, string baseTexture)
    {
        return doors.FirstOrDefault(d => d.BaseTexture.Equals(baseTexture, StringComparison.OrdinalIgnoreCase));
    }

    public static bool InitStandaloneAnimation(
        VmAnimDefEntry anim,
        int animIndex,
        uint curTic,
        VmStandaloneAnimationState state,
        Random? random = null)
    {
        state.Ok = false;
        if (anim.PicFrames.Count == 0) return false;
        state.Ok = true;
        state.AnimIndex = animIndex;
        state.CurFrame = 0;
        state.SwitchTic = curTic;
        state.AnimType = anim.AnimationType.Equals("OscillateDown", StringComparison.OrdinalIgnoreCase)
            ? "OscillateUp"
            : anim.AnimationType;
        var speed = anim.PicFrames[0].SpeedMin;
        if (anim.PicFrames[0].SpeedRange != 0)
            speed += (uint)(random ?? Random.Shared).NextInt64(anim.PicFrames[0].SpeedRange);
        state.SwitchTic += speed / MsPerTic;
        return true;
    }

    public static string UpdateStandaloneAnimation(
        VmAnimDefEntry anim,
        VmStandaloneAnimationState state,
        double curTic,
        Random? random = null)
    {
        if (!state.Ok || anim.PicFrames.Count == 0) return "";
        if (state.SwitchTic > curTic)
            return FrameTexture(anim, state.CurFrame);
        var ticker = new VmAnimationState
        {
            CurFrame = state.CurFrame,
            NumFrames = anim.PicFrames.Count,
            AnimType = state.AnimType,
            SpeedMin = anim.PicFrames[state.CurFrame].SpeedMin,
            SpeedRange = anim.PicFrames[state.CurFrame].SpeedRange
        };
        while (state.SwitchTic <= curTic)
        {
            VmTextureAnimationTicker.AdvanceFrame(ticker);
            state.AnimType = ticker.AnimType;
            var speedFrame = ticker.CurFrame;
            var time = anim.PicFrames[speedFrame].SpeedMin;
            if (anim.PicFrames[speedFrame].SpeedRange != 0)
                time += (uint)(random ?? Random.Shared).NextInt64(anim.PicFrames[speedFrame].SpeedRange);
            state.SwitchTic += time / MsPerTic;
            state.CurFrame = ticker.CurFrame;
        }

        return FrameTexture(anim, state.CurFrame);
    }

    public static void ResetAnimationTimers(VmAnimDefEntry anim)
    {
        anim.DefinedMode = 0;
        if (anim.AnimationType.Equals("OscillateDown", StringComparison.OrdinalIgnoreCase))
            anim.AnimationType = "OscillateUp";
    }

    private static string FrameTexture(VmAnimDefEntry anim, int frame) =>
        frame < anim.PicFrames.Count ? anim.PicFrames[frame].FrameTexture : "";
}
