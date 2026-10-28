namespace HCDE.Playsim.Tests;

public class VmPPSpriteLayersTests
{
    [Fact]
    public void P_SetPsprite_SkipsNullPlayer()
    {
        VmPPSpriteLayers.P_SetPsprite(new VmPPSpriteLayers.SetPspriteContext
        {
            Player = null,
            GetPsprite = _ => throw new InvalidOperationException(),
        });
    }

    [Fact]
    public void GetPsprite_ReturnsNullForCallerIdLayer()
    {
        var player = new VmPPSpriteLayers.PlayerModel { ReadyWeapon = new object() };
        var result = VmPPSpriteLayers.GetPsprite(new VmPPSpriteLayers.GetPspriteContext
        {
            Player = player,
            Layer = VmPPSpriteLayers.PspCallerId,
        });
        Assert.Null(result);
    }

    [Fact]
    public void GetPsprite_ResetsTargeterFlagsOnCallerChange()
    {
        var weapon = new object();
        var pspr = new VmPPSpriteLayers.PspriteModel { Caller = new object(), Flags = 0 };
        var player = new VmPPSpriteLayers.PlayerModel { ReadyWeapon = weapon };
        var created = VmPPSpriteLayers.GetPsprite(new VmPPSpriteLayers.GetPspriteContext
        {
            Player = player,
            Layer = VmPPSpriteLayers.PspWeapon,
            FindPsprite = _ => pspr,
            IsWeapon = _ => true,
            FloatVar = (_, name) => name == "WeaponScaleX" ? 2.0 : 3.0,
        });
        Assert.Same(pspr, created);
        Assert.Equal(VmPPSpriteLayers.PspfAddweapon | VmPPSpriteLayers.PspfAddbob
            | VmPPSpriteLayers.PspfCvarfast | VmPPSpriteLayers.PspfPowdouble
            | VmPPSpriteLayers.PspfPivotpercent, pspr.Flags);
        Assert.Equal(2.0, pspr.BaseScaleX);
        Assert.True(pspr.FirstTic);
    }

    [Fact]
    public void GetPsprite_StrifeHandsClearsStateAndSetsY()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel
        {
            Caller = null,
            State = new VmPPSpriteSetState.StateModel(),
        };
        var player = new VmPPSpriteLayers.PlayerModel { Mo = new object() };
        VmPPSpriteLayers.GetPsprite(new VmPPSpriteLayers.GetPspriteContext
        {
            Player = player,
            Layer = VmPPSpriteLayers.PspStrifehands,
            FindPsprite = _ => pspr,
        });
        Assert.Null(pspr.State);
        Assert.Equal(VmPPSpriteLayers.Weapontop, pspr.Y);
    }

    [Fact]
    public void GetRenderStyle_ForceAlphaOverridesOwner()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel
        {
            Flags = VmPPSpriteLayers.PspfForcealpha,
            Alpha = 0.25,
        };
        var owner = VmPPSpriteLayers.PspriteRenderStyle.Normal;
        var (style, alpha) = VmPPSpriteLayers.GetRenderStyle(
            pspr, owner, 0.9, new VmPPSpriteLayers.RenderStyleContext());
        Assert.Equal(0.25f, alpha);
        Assert.Equal(0, style.Flags & VmPPSpriteLayers.StylefAlpha1);
    }

    [Fact]
    public void GetRenderStyle_NormalWithLowAlphaBecomesTranslucent()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel
        {
            Flags = VmPPSpriteLayers.PspfAlpha,
            Alpha = 0.5,
            Renderstyle = new VmPPSpriteLayers.PspriteRenderStyle(
                ActorBlendOperation.Add, ActorAlphaFactor.One, ActorAlphaFactor.Zero, 0),
        };
        var owner = new VmPPSpriteLayers.PspriteRenderStyle(
            ActorBlendOperation.Add, ActorAlphaFactor.One, ActorAlphaFactor.Zero, 0);
        var (_, alpha) = VmPPSpriteLayers.GetRenderStyle(
            pspr, owner, 0.5, new VmPPSpriteLayers.RenderStyleContext());
        Assert.Equal(0.25f, alpha, 3);
    }

    [Fact]
    public void NewTick_SetsProcessPendingOnAllPsprites()
    {
        var a = new VmPPSpriteLayers.PspriteModel();
        var b = new VmPPSpriteLayers.PspriteModel { Next = null };
        a.Next = b;
        var player = new VmPPSpriteLayers.PlayerModel { Psprites = a };
        VmPPSpriteLayers.NewTick(new VmPPSpriteLayers.NewTickContext
        {
            MaxPlayers = 2,
            PlayerInGame = i => i == 0,
            GetPlayer = i => i == 0 ? player : null,
            ResetInterpolation = _ => { },
        });
        Assert.True(a.ProcessPending);
        Assert.True(b.ProcessPending);
    }
}
