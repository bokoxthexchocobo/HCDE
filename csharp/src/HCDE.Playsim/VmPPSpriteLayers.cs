namespace HCDE.Playsim;

public static class VmPPSpriteLayers
{
    public const int PspStrifehands = -1;
    public const int PspCallerId = 0;
    public const int PspWeapon = 1;
    public const int PspFlash = 1000;
    public const int PspTargetcenter = int.MaxValue - 2;

    public const int PspfAddweapon = 1 << 0;
    public const int PspfAddbob = 1 << 1;
    public const int PspfPowdouble = 1 << 2;
    public const int PspfCvarfast = 1 << 3;
    public const int PspfAlpha = 1 << 4;
    public const int PspfRenderstyle = 1 << 5;
    public const int PspfForcealpha = 1 << 7;
    public const int PspfForcestyle = 1 << 8;
    public const int PspfPivotpercent = 1 << 11;

    public const double Weapontop = 32.0;

    public const int StylefTransSoulsAlpha = 1;
    public const int StylefAlpha1 = 2;
    public const int StylefRedIsAlpha = 4;
    public const int StylefColorIsFixed = 8;

    public const int MaxPlayers = 64;

    public sealed class PspriteModel
    {
        public object? Caller { get; set; }
        public VmPPSpriteSetState.StateModel? State { get; set; }
        public int Flags { get; set; }
        public int Sprite { get; set; }
        public int Frame { get; set; }
        public int Tics { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public int WeaponState { get; set; }
        public bool ProcessPending { get; set; }
        public bool Euthanize { get; set; }
        public bool FirstTic { get; set; }
        public double BaseScaleX { get; set; } = 1;
        public double BaseScaleY { get; set; } = 1;
        public double Alpha { get; set; } = 1;
        public int Translation { get; set; }
        public PspriteRenderStyle Renderstyle { get; set; } = PspriteRenderStyle.Normal;
        public int RenderStyle { get; set; }
        public PspriteModel? Next { get; set; }
        public int Id { get; set; }
        public object? OwnerMo { get; set; }
        public bool CallerIsWeapon { get; set; }
        public int SvFastWeapons { get; set; }
    }

    public readonly record struct PspriteRenderStyle(
        ActorBlendOperation BlendOp,
        ActorAlphaFactor SrcAlpha,
        ActorAlphaFactor DestAlpha,
        int Flags)
    {
        public static PspriteRenderStyle Normal =>
            new(ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, StylefAlpha1);

        public static PspriteRenderStyle Translucent =>
            new(ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 0);

        public PspriteRenderStyle CheckFuzz(bool drawTrans, int drawFuzz)
        {
            if (BlendOp == ActorBlendOperation.FuzzOrAdd)
            {
                if (drawTrans && drawFuzz == 0)
                    return this with { BlendOp = ActorBlendOperation.Add };
            }
            else if (BlendOp == ActorBlendOperation.ReverseSubtract && drawTrans && drawFuzz == 0)
            {
                return this with { BlendOp = ActorBlendOperation.Subtract };
            }

            return this;
        }
    }

    public sealed class PlayerModel
    {
        public object? Mo { get; set; }
        public object? ReadyWeapon { get; set; }
        public PspriteModel? Psprites { get; set; }
        public int PlayerIndex { get; set; }
    }

    public sealed class GetPspriteContext
    {
        public PlayerModel Player { get; set; } = null!;
        public int Layer { get; set; }
        public Func<object?, object?>? FindInventoryTargeter { get; init; }
        public Func<int, PspriteModel?>? FindPsprite { get; init; }
        public Func<object?, int, PspriteModel?>? CreatePsprite { get; init; }
        public Func<object?, bool>? IsWeapon { get; init; }
        public Func<object?, string, double>? FloatVar { get; init; }
    }

    public sealed class SetPspriteContext
    {
        public PlayerModel? Player { get; init; }
        public int Layer { get; init; }
        public VmPPSpriteSetState.StateModel? State { get; init; }
        public bool Pending { get; init; }
        public Func<VmPPSpriteSetState.StateModel?, string>? StateName { get; init; }
        public Action<string>? DebugTrace { get; init; }
        public Func<int, PspriteModel?>? GetPsprite { get; init; }
    }

    public sealed class NewTickContext
    {
        public int MaxPlayers { get; init; } = VmPPSpriteLayers.MaxPlayers;
        public Func<int, bool>? PlayerInGame { get; init; }
        public Func<int, PlayerModel?>? GetPlayer { get; init; }
        public Action<PspriteModel>? ResetInterpolation { get; init; }
    }

    public sealed class RenderStyleContext
    {
        public bool DrawTrans { get; init; }
        public int DrawFuzz { get; init; }
    }

    public static void P_SetPsprite(SetPspriteContext ctx)
    {
        if (ctx.Player == null)
            return;

        var psp = ctx.GetPsprite?.Invoke(ctx.Layer);
        if (psp == null)
            return;

        var stateName = ctx.State != null
            ? ctx.StateName?.Invoke(ctx.State) ?? "?"
            : "<null>";
        ctx.DebugTrace?.Invoke(
            $"playsim.psprite P_SetPsprite layer={ctx.Layer} state={stateName} player={ctx.Player.PlayerIndex}");

        var setCtx = BuildSetStateContext(psp, ctx.State, ctx.Pending, ctx.StateName);
        VmPPSpriteSetState.SetState(setCtx, ctx.State, ctx.Pending);
        CopyBackFromSetState(psp, setCtx);
    }

    public static void SetPspriteAction(PlayerModel player, int id, VmPPSpriteSetState.StateModel? state, bool pending,
        Func<int, PspriteModel?> getPsprite)
    {
        P_SetPsprite(new SetPspriteContext
        {
            Player = player,
            Layer = id,
            State = state,
            Pending = pending,
            GetPsprite = getPsprite,
        });
    }

    public static PspriteModel? GetPsprite(GetPspriteContext ctx)
    {
        object? oldcaller = null;
        object? newcaller;

        if (ctx.Layer >= PspTargetcenter)
        {
            if (ctx.Player.Mo != null)
                newcaller = ctx.FindInventoryTargeter?.Invoke(ctx.Player.Mo);
            else
                newcaller = null;
        }
        else if (ctx.Layer == PspStrifehands)
            newcaller = ctx.Player.Mo;
        else
            newcaller = ctx.Player.ReadyWeapon;

        if (newcaller == null || ctx.Layer == PspCallerId)
            return null;

        var pspr = ctx.FindPsprite?.Invoke(ctx.Layer);
        if (pspr == null)
        {
            pspr = ctx.CreatePsprite?.Invoke(newcaller, ctx.Layer);
            if (pspr == null)
                return null;
        }
        else
        {
            oldcaller = pspr.Caller;
            if (newcaller != null && ctx.IsWeapon?.Invoke(newcaller) == true)
            {
                pspr.BaseScaleX = ctx.FloatVar?.Invoke(newcaller, "WeaponScaleX") ?? pspr.BaseScaleX;
                pspr.BaseScaleY = ctx.FloatVar?.Invoke(newcaller, "WeaponScaleY") ?? pspr.BaseScaleY;
            }
        }

        pspr.Caller = newcaller;

        if (!ReferenceEquals(newcaller, oldcaller))
        {
            if (ctx.Layer >= PspTargetcenter)
                pspr.Flags = PspfCvarfast | PspfPowdouble;
            else
                pspr.Flags = PspfAddweapon | PspfAddbob | PspfCvarfast | PspfPowdouble | PspfPivotpercent;

            if (ctx.Layer == PspStrifehands)
            {
                pspr.State = null;
                pspr.Y = Weapontop;
            }

            pspr.FirstTic = true;
        }

        return pspr;
    }

    public static PspriteModel? GetPspriteAction(PlayerModel player, int id, Func<GetPspriteContext> buildContext)
    {
        var ctx = buildContext();
        ctx.Player = player;
        ctx.Layer = id;
        return GetPsprite(ctx);
    }

    public static (PspriteRenderStyle Style, float Alpha) GetRenderStyle(
        PspriteModel pspr,
        PspriteRenderStyle ownerstyle,
        double owneralpha,
        RenderStyleContext fuzzCtx)
    {
        ownerstyle = ownerstyle.CheckFuzz(fuzzCtx.DrawTrans, fuzzCtx.DrawFuzz);
        var mystyle = pspr.Renderstyle.CheckFuzz(fuzzCtx.DrawTrans, fuzzCtx.DrawFuzz);
        PspriteRenderStyle returnstyle;
        double returnalpha;

        if ((pspr.Flags & PspfRenderstyle) != 0)
        {
            if ((pspr.Flags & PspfForcestyle) != 0)
                returnstyle = mystyle;
            else if (ownerstyle.BlendOp != ActorBlendOperation.Add)
                returnstyle = ownerstyle;
            else
            {
                returnstyle = mystyle;
                if (ownerstyle.DestAlpha == ActorAlphaFactor.One)
                    returnstyle = returnstyle with { DestAlpha = ActorAlphaFactor.One };
                if ((ownerstyle.Flags & (StylefColorIsFixed | StylefRedIsAlpha)) != 0)
                    returnstyle = returnstyle with { Flags = ownerstyle.Flags };
            }
        }
        else
            returnstyle = ownerstyle;

        if ((pspr.Flags & PspfForcealpha) != 0
            && returnstyle.BlendOp != ActorBlendOperation.Fuzz
            && returnstyle.BlendOp != ActorBlendOperation.Shadow)
        {
            returnalpha = pspr.Alpha;
            returnstyle = returnstyle with { Flags = returnstyle.Flags & ~(StylefAlpha1 | StylefTransSoulsAlpha) };
        }
        else if ((pspr.Flags & PspfAlpha) != 0)
        {
            if (returnstyle.BlendOp != ActorBlendOperation.Add)
                returnalpha = owneralpha;
            else
                returnalpha = owneralpha * pspr.Alpha;
        }
        else
            returnalpha = owneralpha;

        if (returnstyle.BlendOp == ActorBlendOperation.Add
            && returnstyle.SrcAlpha == ActorAlphaFactor.One
            && returnstyle.DestAlpha == ActorAlphaFactor.Zero
            && returnalpha < 1.0)
        {
            returnstyle = PspriteRenderStyle.Translucent;
            returnalpha = owneralpha * pspr.Alpha;
        }

        return (returnstyle, (float)Math.Clamp(returnalpha, 0.0, 1.0));
    }

    public static void NewTick(NewTickContext ctx)
    {
        for (var i = 0; i < ctx.MaxPlayers; i++)
        {
            if (ctx.PlayerInGame?.Invoke(i) != true)
                continue;

            var player = ctx.GetPlayer?.Invoke(i);
            if (player == null)
                continue;

            for (var pspr = player.Psprites; pspr != null; pspr = pspr.Next)
            {
                pspr.ProcessPending = true;
                ctx.ResetInterpolation?.Invoke(pspr);
            }
        }
    }

    private static VmPPSpriteSetState.SetStateContext BuildSetStateContext(
        PspriteModel psp,
        VmPPSpriteSetState.StateModel? state,
        bool pending,
        Func<VmPPSpriteSetState.StateModel?, string>? stateName)
    {
        return new VmPPSpriteSetState.SetStateContext
        {
            Id = psp.Id,
            Flags = psp.Flags,
            State = psp.State,
            Sprite = psp.Sprite,
            Frame = psp.Frame,
            Tics = psp.Tics,
            X = psp.X,
            Y = psp.Y,
            OldX = psp.OldX,
            OldY = psp.OldY,
            WeaponState = psp.WeaponState,
            ProcessPending = psp.ProcessPending,
            Euthanize = psp.Euthanize,
            OwnerMo = psp.OwnerMo,
            Caller = psp.Caller,
            CallerIsWeapon = psp.CallerIsWeapon,
            SvFastWeapons = psp.SvFastWeapons,
            StateName = stateName,
            Destroy = () => psp.State = null,
        };
    }

    private static void CopyBackFromSetState(PspriteModel psp, VmPPSpriteSetState.SetStateContext setCtx)
    {
        psp.State = setCtx.State;
        psp.Sprite = setCtx.Sprite;
        psp.Frame = setCtx.Frame;
        psp.Tics = setCtx.Tics;
        psp.X = setCtx.X;
        psp.Y = setCtx.Y;
        psp.OldX = setCtx.OldX;
        psp.OldY = setCtx.OldY;
        psp.WeaponState = setCtx.WeaponState;
        psp.ProcessPending = setCtx.ProcessPending;
        psp.Euthanize = setCtx.Euthanize;
    }
}
