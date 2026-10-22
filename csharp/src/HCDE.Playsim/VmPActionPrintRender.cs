namespace HCDE.Playsim;

/// <summary>Print/log/render-style actions from <c>p_actionfunctions.cpp</c> (1381–1668).</summary>
public static class VmPActionPrintRender
{
    public const int FtfRemove = 1;
    public const int FtfClamp = 2;
    public const int StyleTranslucent = 0;
    public const int StyleFuzzy = 2;
    public const int StyleAdd = 1;

    public static void Recoil(Action<double>? thrust, double yawPlus180, double xyVel) =>
        thrust?.Invoke(xyVel);

    public sealed class PrintContext
    {
        public bool LocalView { get; init; }
        public bool TargetLocalView { get; init; }
        public string Text { get; init; } = "";
        public double Time { get; init; }
        public Action<string, double>? MidPrint { get; init; }
    }

    public static void Print(PrintContext ctx)
    {
        if (!ctx.LocalView && !ctx.TargetLocalView)
            return;
        ctx.MidPrint?.Invoke(ctx.Text, ctx.Time);
    }

    public static void PrintBold(PrintContext ctx) => Print(ctx with { });

    public sealed class LogContext
    {
        public bool LocalOnly { get; init; }
        public bool LocalView { get; init; }
        public Action<string>? Write { get; init; }
    }

    public static void Log(LogContext ctx, string text)
    {
        if (ctx.LocalOnly && !ctx.LocalView)
            return;
        ctx.Write?.Invoke(text);
    }

    public static void LogInt(LogContext ctx, int num) => Log(ctx, num.ToString());

    public static void LogFloat(LogContext ctx, double num) => Log(ctx, num.ToString());

    public sealed class RenderActor
    {
        public double Alpha { get; set; }
        public int RenderStyle { get; set; }
        public int StyleFlags { get; set; }
    }

    public static void SetTranslucent(RenderActor actor, double alpha, int mode)
    {
        actor.StyleFlags &= ~1;
        actor.Alpha = Math.Clamp(alpha, 0, 1);
        actor.RenderStyle = mode == 0 ? StyleTranslucent : mode == 2 ? StyleFuzzy : StyleAdd;
    }

    public static void SetRenderStyle(RenderActor actor, double alpha, int mode)
    {
        actor.Alpha = Math.Clamp(alpha, 0, 1);
        actor.RenderStyle = mode;
    }

    public static void FadeIn(RenderActor actor, double reduce, int flags, Action? remove)
    {
        if (reduce == 0) reduce = 0.1;
        actor.StyleFlags &= ~1;
        actor.Alpha += reduce;
        if (actor.Alpha >= 1)
        {
            if ((flags & FtfClamp) != 0)
                actor.Alpha = 1;
            if ((flags & FtfRemove) != 0)
                remove?.Invoke();
        }
    }

    public static void FadeOut(RenderActor actor, double reduce, int flags, Action? remove)
    {
        if (reduce == 0) reduce = 0.1;
        actor.StyleFlags &= ~1;
        actor.Alpha -= reduce;
        if (actor.Alpha <= 0)
        {
            if ((flags & FtfClamp) != 0)
                actor.Alpha = 0;
            if ((flags & FtfRemove) != 0)
                remove?.Invoke();
        }
    }

    public static void FadeTo(RenderActor actor, double target, double amount, int flags, Action? remove)
    {
        actor.StyleFlags &= ~1;
        if (actor.Alpha > target)
            actor.Alpha = Math.Max(target, actor.Alpha - amount);
        else if (actor.Alpha < target)
            actor.Alpha = Math.Min(target, actor.Alpha + amount);
        if ((flags & FtfClamp) != 0)
            actor.Alpha = Math.Clamp(actor.Alpha, 0, 1);
        if (actor.Alpha == target && (flags & FtfRemove) != 0)
            remove?.Invoke();
    }
}
