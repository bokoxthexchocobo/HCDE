namespace HCDE.Gamedata;

/// <summary><c>AdvanceFrame</c>, <c>SetSwitchTime</c>, <c>UpdateAnimations</c> from <c>animations.cpp</c>.</summary>
public sealed class VmAnimationState
{
    public int CurFrame { get; set; }
    public string AnimType { get; set; } = "Forward";
    public ulong SwitchTimeMs { get; set; }
    public int NumFrames { get; set; }
    public uint SpeedMin { get; set; }
    public uint SpeedRange { get; set; }
}

public static class VmTextureAnimationTicker
{
    public static ulong ComputeSwitchTime(ulong nowMs, VmAnimationState anim, Random? random = null)
    {
        var rng = random ?? Random.Shared;
        var next = nowMs + anim.SpeedMin;
        if (anim.SpeedRange != 0)
            next += (ulong)rng.NextInt64(anim.SpeedRange);
        return next;
    }

    public static void AdvanceFrame(VmAnimationState anim)
    {
        switch (anim.AnimType)
        {
            case "Backward":
                anim.CurFrame = anim.CurFrame == 0 ? anim.NumFrames - 1 : anim.CurFrame - 1;
                break;
            case "Random":
                if (anim.NumFrames > 1)
                {
                    var rnd = Random.Shared.Next(anim.NumFrames - 1);
                    if (rnd == anim.CurFrame) rnd++;
                    anim.CurFrame = rnd % anim.NumFrames;
                }

                break;
            case "OscillateUp":
                anim.CurFrame++;
                if (anim.CurFrame >= anim.NumFrames - 1)
                    anim.AnimType = "OscillateDown";
                break;
            case "OscillateDown":
                anim.CurFrame--;
                if (anim.CurFrame <= 0)
                    anim.AnimType = "OscillateUp";
                break;
            default:
                anim.CurFrame = (anim.CurFrame + 1) % Math.Max(anim.NumFrames, 1);
                break;
        }
    }

    public static void UpdateUntil(ulong nowMs, VmAnimationState anim)
    {
        if (anim.SwitchTimeMs == 0)
        {
            anim.SwitchTimeMs = ComputeSwitchTime(nowMs, anim);
            return;
        }

        while (anim.SwitchTimeMs <= nowMs)
        {
            AdvanceFrame(anim);
            anim.SwitchTimeMs = ComputeSwitchTime(nowMs, anim);
        }
    }
}
