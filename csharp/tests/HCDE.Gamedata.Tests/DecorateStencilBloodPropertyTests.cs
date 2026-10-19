namespace HCDE.Gamedata.Tests;

public class DecorateStencilBloodPropertyTests
{
    [Fact]
    public void RouterAppliesStencilBloodAndSpriteAngle()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty(
            "stencilcolor", "Actor", ["255", ",", "0", ",", "0"], def, out _));
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty(
            "bloodcolor", "Actor", ["128", ",", "0", ",", "0"], def, out _));
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty(
            "bloodtype", "Actor", ["Blood", "Splat", "Axe"], def, out _));
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty(
            "spriteangle", "Actor", ["90"], def, out _));
        Assert.Equal("Blood", def.BloodType);
        Assert.Equal("Splat", def.BloodType2);
        Assert.Equal("Axe", def.BloodType3);
        Assert.Equal(90, def.SpriteAngle);
    }
}
