namespace HCDE.Playsim;

/// <summary>Decal animation thinkers from <c>a_decalfx.cpp</c> (39–236).</summary>
public static class VmADecalFx
{
    public const int StyleColorIsFixed = 1 << 3;

    public sealed class DecalRef
    {
        public double Alpha { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Z { get; set; }
        public int AlphaColor { get; set; }
        public int RenderStyleFlags { get; set; }
        public Action Expired { get; init; } = () => { };
        public Action Destroy { get; init; } = () => { };
        public Action<int> SetShadeRgb { get; init; } = _ => { };
        public Action<int> SetShadeColor { get; init; } = _ => { };
    }

    public class ThinkerState
    {
        public DecalRef? TheDecal { get; set; }
        public Action DestroySelf { get; init; } = () => { };
    }

    public sealed class FaderState : ThinkerState
    {
        public int TimeToStartDecay { get; set; }
        public int TimeToEndDecay { get; set; }
        public int StartTrans { get; set; } = -1;
    }

    public sealed class StretcherState : ThinkerState
    {
        public int TimeToStart { get; set; }
        public int TimeToStop { get; set; }
        public double GoalX { get; set; }
        public double StartX { get; set; }
        public bool StretchX { get; set; }
        public double GoalY { get; set; }
        public double StartY { get; set; }
        public bool StretchY { get; set; }
        public bool Started { get; set; }
    }

    public sealed class SliderState : ThinkerState
    {
        public int TimeToStart { get; set; }
        public int TimeToStop { get; set; }
        public double DistY { get; set; }
        public double StartY { get; set; }
        public bool Started { get; set; }
    }

    public sealed class ColorerState : ThinkerState
    {
        public int TimeToStartDecay { get; set; }
        public int TimeToEndDecay { get; set; }
        public RgbColor StartColor { get; set; } = new(255, 0, 0, 0);
        public RgbColor GoalColor { get; set; } = new();
    }

    public readonly struct RgbColor
    {
        public RgbColor(int a, int r, int g, int b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public int A { get; }
        public int R { get; }
        public int G { get; }
        public int B { get; }
    }

    public static int Scale(int value, int numerator, int denominator)
    {
        if (denominator == 0)
            return 0;
        return value * numerator / denominator;
    }

    public static void TickFader(FaderState state, int mapTime, bool frozen)
    {
        if (state.TheDecal == null)
        {
            state.DestroySelf();
            return;
        }

        if (mapTime < state.TimeToStartDecay || frozen)
            return;

        if (mapTime >= state.TimeToEndDecay)
        {
            state.TheDecal.Expired();
            state.TheDecal.Destroy();
            state.DestroySelf();
            return;
        }

        if (state.StartTrans == -1)
            state.StartTrans = (int)state.TheDecal.Alpha;

        var distanceToEnd = state.TimeToEndDecay - mapTime;
        var fadeDistance = state.TimeToEndDecay - state.TimeToStartDecay;
        state.TheDecal.Alpha = state.StartTrans * distanceToEnd / fadeDistance;
    }

    public static void TickStretcher(StretcherState state, int mapTime, bool frozen)
    {
        if (state.TheDecal == null)
        {
            state.DestroySelf();
            return;
        }

        if (mapTime < state.TimeToStart || frozen)
            return;

        if (mapTime >= state.TimeToStop)
        {
            if (state.StretchX)
                state.TheDecal.ScaleX = state.GoalX;
            if (state.StretchY)
                state.TheDecal.ScaleY = state.GoalY;
            state.DestroySelf();
            return;
        }

        if (!state.Started)
        {
            state.Started = true;
            state.StartX = state.TheDecal.ScaleX;
            state.StartY = state.TheDecal.ScaleY;
        }

        var distance = mapTime - state.TimeToStart;
        var maxDistance = state.TimeToStop - state.TimeToStart;
        if (state.StretchX)
            state.TheDecal.ScaleX = state.StartX + (state.GoalX - state.StartX) * distance / maxDistance;
        if (state.StretchY)
            state.TheDecal.ScaleY = state.StartY + (state.GoalY - state.StartY) * distance / maxDistance;
    }

    public static void TickSlider(SliderState state, int mapTime, bool frozen)
    {
        if (state.TheDecal == null)
        {
            state.DestroySelf();
            return;
        }

        if (mapTime < state.TimeToStart || frozen)
            return;

        if (!state.Started)
        {
            state.Started = true;
            state.StartY = state.TheDecal.Z;
        }

        if (mapTime >= state.TimeToStop)
        {
            state.TheDecal.Z = state.StartY + state.DistY;
            state.DestroySelf();
            return;
        }

        var distance = mapTime - state.TimeToStart;
        var maxDistance = state.TimeToStop - state.TimeToStart;
        state.TheDecal.Z = state.StartY + state.DistY * distance / maxDistance;
    }

    public static void TickColorer(ColorerState state, int mapTime, bool frozen)
    {
        if (state.TheDecal == null || (state.TheDecal.RenderStyleFlags & StyleColorIsFixed) == 0)
        {
            state.DestroySelf();
            return;
        }

        if (mapTime < state.TimeToStartDecay || frozen)
            return;

        if (mapTime >= state.TimeToEndDecay)
        {
            state.TheDecal.SetShadeColor(state.GoalColor.R | (state.GoalColor.G << 8) | (state.GoalColor.B << 16));
            state.DestroySelf();
            return;
        }

        var start = state.StartColor;
        if (start.A == 255)
        {
            var sampled = state.TheDecal.AlphaColor & 0xffffff;
            start = new RgbColor(0, sampled & 0xff, (sampled >> 8) & 0xff, (sampled >> 16) & 0xff);
            var goalPacked = state.GoalColor.R | (state.GoalColor.G << 8) | (state.GoalColor.B << 16);
            if ((sampled & 0xffffff) == goalPacked)
            {
                state.DestroySelf();
                return;
            }

            state.StartColor = start;
        }

        if ((mapTime & 0) != 0)
            return;

        var distance = mapTime - state.TimeToStartDecay;
        var maxDistance = state.TimeToEndDecay - state.TimeToStartDecay;
        var r = start.R + Scale(state.GoalColor.R - start.R, distance, maxDistance);
        var g = start.G + Scale(state.GoalColor.G - start.G, distance, maxDistance);
        var b = start.B + Scale(state.GoalColor.B - start.B, distance, maxDistance);
        state.TheDecal.SetShadeRgb(r | (g << 8) | (b << 16));
    }
}
