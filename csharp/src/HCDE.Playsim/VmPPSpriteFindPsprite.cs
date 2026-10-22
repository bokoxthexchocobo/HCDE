namespace HCDE.Playsim;

/// <summary>Player psprite layer lookup (<c>p_pspr.cpp</c> 227–249).</summary>
public static class VmPPSpriteFindPsprite
{
    public sealed class PspriteModel
    {
        public int Id { get; set; }
        public PspriteModel? Next { get; set; }
    }

    public sealed class PlayerModel
    {
        public PspriteModel? Psprites { get; set; }
    }

    public static PspriteModel? FindPsprite(PlayerModel player, int layer)
    {
        if (layer == 0)
            return null;

        for (var pspr = player.Psprites; pspr != null; pspr = pspr.Next)
        {
            if (pspr.Id == layer)
                return pspr;
        }

        return null;
    }

    public static PspriteModel? FindPspriteAction(PlayerModel player, int id) => FindPsprite(player, id);
}
