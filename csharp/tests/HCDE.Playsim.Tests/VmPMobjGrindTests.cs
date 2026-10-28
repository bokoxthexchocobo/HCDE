namespace HCDE.Playsim.Tests;

public class VmPMobjGrindTests
{
    [Fact]
    public void Grind_CorpseDontCrunch_Shrinks()
    {
        var actor = new VmPMobjGrind.GrindActor
        {
            Flags = VmPMobjGrind.MfCorpse,
            Health = 0,
            DontCrunchCorpses = true,
        };
        Assert.False(VmPMobjGrind.Grind(new(), actor));
        Assert.True(actor.VelZeroed);
        Assert.Equal(0, actor.Height);
        Assert.Equal(0, actor.Radius);
    }

    [Fact]
    public void Grind_Nonsolid_ReturnsFalse()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = 0 };
        Assert.False(VmPMobjGrind.Grind(new(), actor));
    }

    [Fact]
    public void Grind_ItemsFlag_PassedThroughContext()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = 0 };
        Assert.False(VmPMobjGrind.Grind(new() { Items = true }, actor));
    }

    [Fact]
    public void CallGrind_UsesVirtualWhenPresent()
    {
        var actor = new VmPMobjGrind.GrindActor { Flags = VmPMobjGrind.MfSolid | VmPMobjGrind.MfShootable };
        Assert.True(VmPMobjGrind.CallGrind(new(), actor, hasVirtual: true, vmReturn: 1));
    }

    [Fact]
    public void Grind_GenericCrushState_SkipsZeroFrameSprite()
    {
        var crush = new object();
        var actor = new VmPMobjGrind.GrindActor
        {
            Flags = VmPMobjGrind.MfCorpse,
            Health = 0,
            Height = 32,
            GenericCrushState = crush,
            BloodTranslation = 7,
        };
        var ctx = new VmPMobjGrind.GrindContext
        {
            SpriteNumFrames = _ => 0,
        };
        Assert.False(VmPMobjGrind.Grind(ctx, actor));
        Assert.Equal(0, actor.Height);
        Assert.NotEqual(0, actor.Flags3 & VmPMobjGrind.Mf3Dontgib);
    }

    [Fact]
    public void Grind_TouchyArmed_DamagesAndKeepsChecking()
    {
        var actor = new VmPMobjGrind.GrindActor
        {
            Flags6 = VmPMobjGrind.Mf6Touchy | VmPMobjGrind.Mf6Armed,
            Health = 50,
        };
        var damaged = false;
        var ctx = new VmPMobjGrind.GrindContext
        {
            DamageMobj = (_, _, _, dmg, _, _) => damaged = dmg == 50,
        };
        Assert.True(VmPMobjGrind.Grind(ctx, actor));
        Assert.True(damaged);
        Assert.Equal(0, actor.Flags6 & VmPMobjGrind.Mf6Armed);
    }
}
