namespace HCDE.Playsim;

/// <summary>Weapon-ready button flags and `DPSprite` layer insert from <c>p_pspr.cpp</c> (54–102, 163–219).</summary>
public static class VmPPSprWeaponReady
{
    public const double LowerSpeed = 6.0;
    public const int SvFastWeaponsDefault = 0;
    [Flags]
    public enum WrfOptions
    {
        NoBob = 1,
        NoSwitch = 1 << 1,
        NoPrimary = 1 << 2,
        NoSecondary = 1 << 3,
        NoFire = NoPrimary | NoSecondary,
        AllowReload = 1 << 4,
        AllowZoom = 1 << 5,
        DisableSwitch = 1 << 6,
        AllowUser1 = 1 << 7,
        AllowUser2 = 1 << 8,
        AllowUser3 = 1 << 9,
        AllowUser4 = 1 << 10,
    }

    public readonly record struct GenericButton(int ReadyFlag, int StateFlag, int ButtonFlag, string StateName);

    public static readonly GenericButton[] ButtonChecks =
    {
        new((int)WrfOptions.AllowZoom, 1, 1 << 0, "Zoom"),
        new((int)WrfOptions.AllowReload, 2, 1 << 1, "Reload"),
        new((int)WrfOptions.AllowUser1, 4, 1 << 2, "User1"),
        new((int)WrfOptions.AllowUser2, 8, 1 << 3, "User2"),
        new((int)WrfOptions.AllowUser3, 16, 1 << 4, "User3"),
        new((int)WrfOptions.AllowUser4, 32, 1 << 5, "User4"),
    };

    public const int PspfAddWeapon = 1;
    public const int PspfAddBob = 2;
    public const int PspfPowDouble = 4;
    public const int PspfCvarFast = 8;
    public const int PspfPivotPercent = 128;
    public const int PspfFlip = 256;
    public const int PspfMirror = 512;
    public const int PspfPlayerTranslated = 1024;
    public const int PspfInterpolate = 2048;

    public static int DefaultWeaponPspriteFlags =>
        PspfAddWeapon | PspfAddBob | PspfPowDouble | PspfCvarFast | PspfPivotPercent;

    public sealed class PspriteNode
    {
        public int Id { get; init; }
        public PspriteNode? Next { get; set; }
        public object? Caller { get; set; }
        public int Flags { get; set; }
        public bool Destroyed { get; private set; }
        public void Destroy() => Destroyed = true;
    }

    public sealed class PlayerPsprites
    {
        public PspriteNode? Head { get; set; }
    }

    public static PspriteNode InsertPspriteLayer(
        PlayerPsprites owner,
        object caller,
        int id,
        Func<object, bool> isWeapon,
        Func<object, bool> isPlayerPawn)
    {
        var node = new PspriteNode { Id = id, Caller = caller };
        PspriteNode? prev = null;
        var next = owner.Head;
        while (next != null && next.Id < id)
        {
            prev = next;
            next = next.Next;
        }
        node.Next = next;
        if (prev == null)
            owner.Head = node;
        else
            prev.Next = node;
        if (next != null && next.Id == id && id != 0)
            next.Destroy();
        if (isWeapon(caller) || isPlayerPawn(caller))
            node.Flags = PspfAddWeapon | PspfAddBob | PspfPowDouble | PspfCvarFast | PspfPivotPercent;
        return node;
    }

    public static PspriteNode? FindPsprite(PlayerPsprites owner, int layer)
    {
        if (layer == 0)
            return null;
        var pspr = owner.Head;
        while (pspr != null)
        {
            if (pspr.Id == layer)
                break;
            pspr = pspr.Next;
        }
        return pspr;
    }
}
