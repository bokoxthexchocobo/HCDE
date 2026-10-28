namespace HCDE.Playsim;

public static class VmPPSpriteClearBulletSetup
{
    public const int AlfIgnorenoautoaim = 1 << 0;

    public sealed class ClearPlayerModel
    {
        public VmPPSpriteLayers.PspriteModel? Psprites { get; set; }
        public Action<VmPPSpriteLayers.PspriteModel>? SetStateNull { get; set; }
    }

    public sealed class SetupContext
    {
        public VmPPSpriteWeaponBob.PlayerModel Player { get; init; } = null!;
        public Action? DestroyPsprites { get; init; }
    }

    public sealed class LineTargetScratch
    {
        public object? Linetarget { get; set; }
    }

    public sealed class BulletSlopeContext
    {
        public Func<object?, double, LineTargetScratch, int, double>? AimLineAttackDegrees { get; init; }
        public Func<object?, bool>? IsFreelookAllowed { get; init; }
        public Func<object?, double>? GetAimDist { get; init; }
    }

    public static int ClearOverlays(ClearPlayerModel? player, int start, int stop, bool safety)
    {
        if (player == null)
            return 0;

        if (start == 0 && stop == 0)
        {
            start = int.MinValue;
            stop = safety ? VmPPSpriteLayers.PspTargetcenter - 1 : int.MaxValue;
        }

        uint count = 0;
        for (var pspr = player.Psprites; pspr != null; pspr = pspr.Next)
        {
            var id = pspr.Id;
            if (id < start || id == 0)
                continue;
            if (id > stop)
                break;

            if (safety)
            {
                if (id >= VmPPSpriteLayers.PspTargetcenter)
                    break;
                if (id == VmPPSpriteLayers.PspStrifehands
                    || id == VmPPSpriteLayers.PspWeapon
                    || id == VmPPSpriteLayers.PspFlash)
                    continue;
            }

            player.SetStateNull?.Invoke(pspr);
            count++;
        }

        return (int)count;
    }

    public static double BulletSlope(
        BulletSlopeContext ctx,
        object? mo,
        LineTargetScratch? lineTarget,
        int aimflags)
    {
        ReadOnlySpan<double> angdiff = [-5.625, 5.625, 0];
        aimflags &= ~AlfIgnorenoautoaim;
        var scratch = lineTarget ?? new LineTargetScratch();
        var target = scratch;

        var i = 2;
        double pitch;
        do
        {
            var an = angdiff[i];
            pitch = ctx.AimLineAttackDegrees?.Invoke(mo, an, target, aimflags) ?? 0;
            if (mo != null
                && ctx.IsFreelookAllowed?.Invoke(mo) == true
                && (ctx.GetAimDist?.Invoke(mo) ?? 1) <= 0.5)
                break;
        } while (target.Linetarget == null && --i >= 0);

        return pitch;
    }

    public static double BulletSlopeAction(
        BulletSlopeContext ctx,
        object? self,
        LineTargetScratch? target,
        int aimflags) =>
        BulletSlope(ctx, self, target, aimflags);

    public static void SetupPsprites(
        SetupContext ctx,
        bool startweaponup,
        Action<VmPPSpriteWeaponBob.BringUpContext>? bringUpWeapon,
        Action<string>? debugTrace = null)
    {
        var player = ctx.Player;
        var readyName = player.ReadyWeapon != null
            ? player.WeaponClassName?.Invoke(player.ReadyWeapon) ?? "?"
            : "None";
        var pending = player.PendingWeapon;
        var pendingName = pending switch
        {
            null => "None",
            _ when ReferenceEquals(pending, VmPPSpriteWeaponBob.WpNoChange) => "WP_NOCHANGE",
            _ => player.WeaponClassName?.Invoke(pending) ?? "?"
        };

        debugTrace?.Invoke(
            $"playsim.psprite P_SetupPsprites player={player.PlayerIndex} startweaponup={(startweaponup ? 1 : 0)} ReadyWeapon={readyName} PendingWeapon={pendingName}");

        ctx.DestroyPsprites?.Invoke();
        player.PendingWeapon = !startweaponup ? player.ReadyWeapon : VmPPSpriteWeaponBob.WpNoChange;
        bringUpWeapon?.Invoke(new VmPPSpriteWeaponBob.BringUpContext { Player = player });
    }
}
