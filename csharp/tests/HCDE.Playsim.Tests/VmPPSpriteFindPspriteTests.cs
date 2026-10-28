namespace HCDE.Playsim.Tests;

public class VmPPSpriteFindPspriteTests
{
    [Fact]
    public void FindPsprite_ReturnsNullForLayerZero()
    {
        var player = new VmPPSpriteFindPsprite.PlayerModel
        {
            Psprites = new VmPPSpriteFindPsprite.PspriteModel { Id = 1 },
        };
        Assert.Null(VmPPSpriteFindPsprite.FindPsprite(player, 0));
    }

    [Fact]
    public void FindPsprite_WalksLinkedList()
    {
        var target = new VmPPSpriteFindPsprite.PspriteModel { Id = 5 };
        var player = new VmPPSpriteFindPsprite.PlayerModel
        {
            Psprites = new VmPPSpriteFindPsprite.PspriteModel
            {
                Id = 1,
                Next = new VmPPSpriteFindPsprite.PspriteModel { Id = 3, Next = target },
            },
        };

        Assert.Same(target, VmPPSpriteFindPsprite.FindPsprite(player, 5));
        Assert.Null(VmPPSpriteFindPsprite.FindPsprite(player, 9));
    }
}
