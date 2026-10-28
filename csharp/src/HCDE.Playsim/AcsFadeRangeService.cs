namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::DoFadeRange</c> screen blend for ACS fade opcodes.</summary>
public static class AcsFadeRangeService
{
    public sealed class BlendState
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
        public float? PendingFromR { get; set; }
        public float? PendingFromG { get; set; }
        public float? PendingFromB { get; set; }
        public float? PendingFromA { get; set; }
        public float? PendingToR { get; set; }
        public float? PendingToG { get; set; }
        public float? PendingToB { get; set; }
        public float? PendingToA { get; set; }
        public float PendingSeconds { get; set; }
    }

    public static void Apply(
        BlendState blend,
        int r1,
        int g1,
        int b1,
        int a1,
        int r2,
        int g2,
        int b2,
        int a2,
        int timeAcs)
    {
        var fadingFrom = a1 >= 0;
        var seconds = timeAcs / 65536f;
        var toR = r2 / 255f;
        var toG = g2 / 255f;
        var toB = b2 / 255f;
        var toA = a2 / 65536f;

        if (seconds <= 0f)
        {
            blend.R = toR;
            blend.G = toG;
            blend.B = toB;
            blend.A = toA;
            ClearPending(blend);
            return;
        }

        float fromR, fromG, fromB, fromA;
        if (fadingFrom)
        {
            fromR = r1 / 255f;
            fromG = g1 / 255f;
            fromB = b1 / 255f;
            fromA = a1 / 65536f;
        }
        else if (blend.A <= 0f)
        {
            fromR = toR;
            fromG = toG;
            fromB = toB;
            fromA = 0f;
        }
        else
        {
            fromR = blend.R;
            fromG = blend.G;
            fromB = blend.B;
            fromA = blend.A;
        }

        blend.PendingFromR = fromR;
        blend.PendingFromG = fromG;
        blend.PendingFromB = fromB;
        blend.PendingFromA = fromA;
        blend.PendingToR = toR;
        blend.PendingToG = toG;
        blend.PendingToB = toB;
        blend.PendingToA = toA;
        blend.PendingSeconds = seconds;
    }

    private static void ClearPending(BlendState blend)
    {
        blend.PendingFromR = blend.PendingFromG = blend.PendingFromB = blend.PendingFromA = null;
        blend.PendingToR = blend.PendingToG = blend.PendingToB = blend.PendingToA = null;
        blend.PendingSeconds = 0f;
    }
}
