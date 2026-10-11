namespace HCDE.Playsim.Tests;

public class PlayerPainFlashTests
{
    private class ColoredPlayer : PlayerPawn
    {
        private static readonly PlayerPainFlash[] Flashes =
        [
            new(typeof(ColoredPlayer), "Fire", 0xffff8000u),
            new(typeof(ChildPlayer), "Fire", 0xff00ff00u),
            new(typeof(OtherPlayer), "Fire", 0xff0000ffu),
            new(typeof(ColoredPlayer), "Ice", 0),
            new(typeof(ColoredPlayer), "Poison", 0x0000ff00u),
        ];
        public override uint DamageFade => 0xffff00ffu;
        protected override IReadOnlyList<PlayerPainFlash> PainFlashes => Flashes;
    }
    private sealed class ChildPlayer : ColoredPlayer { }
    private sealed class OtherPlayer : PlayerPawn { }

    [Theory]
    [InlineData("Fire", 0xffff8000u)]
    [InlineData("fIrE", 0xffff8000u)]
    [InlineData("Ice", 0xffff00ffu)]
    [InlineData("Unknown", 0xffff00ffu)]
    [InlineData("Poison", 0x0000ff00u)]
    public void SelectsMatchingClassAndTypeOrDamageFade(string type, uint color)
    {
        var player = new ColoredPlayer { NoPain = true };
        ActorDamage.Apply(player, 1, damageType: type);
        Assert.Equal(color, player.GetPainFlash());
    }

    [Fact]
    public void ChildRegistrationWinsAndIncomingTypeIgnoresPainTypeOverride()
    {
        var player = new ChildPlayer { NoPain = true };
        ActorDamage.Apply(player, 1, damageType: "Fire", inflictor: new Actor { PainType = "Ice" });
        Assert.Equal(0xff00ff00u, player.GetPainFlash());
    }

    [Fact]
    public void DefaultAndUndamagedClassUseTheirDamageFade()
    {
        Assert.Equal(0xffff0000u, new PlayerPawn().GetPainFlash());
        Assert.Equal(0xffff00ffu, new ColoredPlayer().GetPainFlash());
    }
}
