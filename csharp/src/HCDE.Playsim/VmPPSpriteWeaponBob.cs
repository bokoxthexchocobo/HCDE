namespace HCDE.Playsim;

/// <summary>Weapon bring-up VM hook and per-tic weapon bob (<c>p_pspr.cpp</c> 659–779).</summary>
public static class VmPPSpriteWeaponBob
{
    public const int MaxPlayers = VmPPSpriteLayers.MaxPlayers;
    public const int OfEuthanizeMe = VmPMobjTagDamageRestore.OfEuthanizeme;

    public static readonly object WpNoChange = new();

    public sealed class PlayerModel
    {
        public object? Mo { get; set; }
        public object? ReadyWeapon { get; set; }
        public object? PendingWeapon { get; set; }
        public int PlayerIndex { get; set; }
        public int BobTimer { get; set; }
        public Func<object?, string>? WeaponClassName { get; set; }
    }

    public sealed class InventoryModel
    {
        public InventoryModel? Inventory { get; set; }
        public int ObjectFlags { get; set; }
        public bool OverridesModifyBob { get; set; }
        public bool OverridesModifyBob3D { get; set; }
        public Func<(double X, double Y), double, (double X, double Y)>? ModifyBob { get; set; }
        public Func<(double T, double R), (double T, double R), double, ((double T, double R) T, (double T, double R) R)>?
            ModifyBob3D { get; set; }
    }

    public sealed class PlayerBobSlot
    {
        public bool Interpolation2D { get; private set; }
        public bool Interpolation3D { get; private set; }
        public (double X, double Y) Bob2D { get; private set; }
        public (double T, double R) Bob3DTranslation { get; private set; }
        public (double T, double R) Bob3DRotation { get; private set; }

        public void UpdateInterpolation(bool twoD)
        {
            Interpolation2D = twoD;
            Interpolation3D = !twoD;
        }

        public void ResetInterpolation()
        {
            Interpolation2D = false;
            Interpolation3D = false;
        }

        public void SetBob2D(int tic, (double X, double Y) bob)
        {
            Bob2D = bob;
            _ = tic;
        }

        public void SetBob3D(int tic, (double T, double R) translation, (double T, double R) rotation)
        {
            Bob3DTranslation = translation;
            Bob3DRotation = rotation;
            _ = tic;
        }
    }

    public sealed class BringUpContext
    {
        public PlayerModel Player { get; init; } = null!;
        public Action<object?>? VmBringUpWeapon { get; init; }
        public Action<string>? DebugTrace { get; init; }
    }

    public sealed class BobWeaponContext
    {
        public PlayerModel Player { get; init; } = null!;
        public PlayerBobSlot[] PlayerBob { get; init; } = Array.Empty<PlayerBobSlot>();
        public Func<object?, bool>? HasVirtualBobWeapon { get; init; }
        public Func<object?, double, (double X, double Y)>? CallBobWeapon { get; init; }
        public Func<object?, InventoryModel?>? FirstInventory { get; init; }
    }

    public sealed class BobWeapon3DContext
    {
        public PlayerModel Player { get; init; } = null!;
        public PlayerBobSlot[] PlayerBob { get; init; } = Array.Empty<PlayerBobSlot>();
        public Func<object?, bool>? HasVirtualBobWeapon3D { get; init; }
        public Func<object?, double, ((double T, double R) T, (double T, double R) R)>? CallBobWeapon3D { get; init; }
        public Func<object?, InventoryModel?>? FirstInventory { get; init; }
    }

    public static void BringUpWeapon(BringUpContext ctx)
    {
        var readyName = ctx.Player.ReadyWeapon != null
            ? ctx.Player.WeaponClassName?.Invoke(ctx.Player.ReadyWeapon) ?? "?"
            : "None";
        var pending = ctx.Player.PendingWeapon;
        var pendingName = pending switch
        {
            null => "None",
            _ when ReferenceEquals(pending, WpNoChange) => "WP_NOCHANGE",
            _ => ctx.Player.WeaponClassName?.Invoke(pending) ?? "?"
        };

        ctx.DebugTrace?.Invoke(
            $"playsim.psprite P_BringUpWeapon ENTER player={ctx.Player.PlayerIndex} ReadyWeapon={readyName} PendingWeapon={pendingName}");

        if (ctx.Player.Mo != null)
            ctx.VmBringUpWeapon?.Invoke(ctx.Player.Mo);

        ctx.DebugTrace?.Invoke($"playsim.psprite P_BringUpWeapon EXIT player={ctx.Player.PlayerIndex}");
    }

    public static double WeaponBobSmoothFrac(double ticFrac, bool smoothEnabled)
    {
        if (!smoothEnabled)
            return ticFrac;

        if (ticFrac <= 0.0)
            return 0.0;
        if (ticFrac >= 1.0)
            return 1.0;

        return ticFrac * ticFrac * (3.0 - 2.0 * ticFrac);
    }

    public static void BobWeapon(BobWeaponContext ctx)
    {
        var bob = ctx.PlayerBob[ctx.Player.PlayerIndex];
        if (ctx.Player.Mo != null && ctx.HasVirtualBobWeapon?.Invoke(ctx.Player.Mo) == true)
        {
            bob.UpdateInterpolation(twoD: true);
            var result = ctx.CallBobWeapon?.Invoke(ctx.Player.Mo, 1.0) ?? (0, 0);

            for (var inv = ctx.FirstInventory?.Invoke(ctx.Player.Mo);
                 inv != null && (inv.ObjectFlags & OfEuthanizeMe) == 0;
                 inv = inv.Inventory)
            {
                if (inv.OverridesModifyBob && inv.ModifyBob != null)
                    result = inv.ModifyBob(result, 1.0);
            }

            bob.SetBob2D(ctx.Player.BobTimer, result);
            return;
        }

        bob.ResetInterpolation();
    }

    public static void BobWeapon3D(BobWeapon3DContext ctx)
    {
        var bob = ctx.PlayerBob[ctx.Player.PlayerIndex];
        if (ctx.Player.Mo != null && ctx.HasVirtualBobWeapon3D?.Invoke(ctx.Player.Mo) == true)
        {
            bob.UpdateInterpolation(twoD: false);
            var (translation, rotation) = ctx.CallBobWeapon3D?.Invoke(ctx.Player.Mo, 1.0) ?? ((0, 0), (0, 0));

            for (var inv = ctx.FirstInventory?.Invoke(ctx.Player.Mo);
                 inv != null && (inv.ObjectFlags & OfEuthanizeMe) == 0;
                 inv = inv.Inventory)
            {
                if (inv.OverridesModifyBob3D && inv.ModifyBob3D != null)
                {
                    var modified = inv.ModifyBob3D(translation, rotation, 1.0);
                    translation = modified.T;
                    rotation = modified.R;
                }
            }

            bob.SetBob3D(ctx.Player.BobTimer, translation, rotation);
            return;
        }

        bob.ResetInterpolation();
    }
}
