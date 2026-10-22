namespace HCDE.Playsim;

/// <summary>Extra weapon button routing to psprite states (<c>p_pspr.cpp</c> 789–821).</summary>
public static class VmPPSpriteCheckWeaponButtons
{
    public const int WrfAllowReload = 1 << 4;
    public const int WrfAllowZoom = 1 << 5;
    public const int WrfAllowUser1 = 1 << 7;
    public const int WrfAllowUser2 = 1 << 8;
    public const int WrfAllowUser3 = 1 << 9;
    public const int WrfAllowUser4 = 1 << 10;

    public const int WfWeaponReloadOk = 1 << 5;
    public const int WfWeaponZoomOk = 1 << 6;
    public const int WfUser1Ok = 1 << 8;
    public const int WfUser2Ok = 1 << 9;
    public const int WfUser3Ok = 1 << 10;
    public const int WfUser4Ok = 1 << 11;

    public const int BtReload = 1 << 6;
    public const int BtZoom = 1 << 7;
    public const int BtUser1 = 1 << 21;
    public const int BtUser2 = 1 << 22;
    public const int BtUser3 = 1 << 23;
    public const int BtUser4 = 1 << 24;

    public readonly record struct ButtonCheck(int ReadyFlag, int StateFlag, int ButtonFlag, string StateName);

    public static readonly ButtonCheck[] ButtonChecks =
    [
        new(WrfAllowZoom, WfWeaponZoomOk, BtZoom, "Zoom"),
        new(WrfAllowReload, WfWeaponReloadOk, BtReload, "Reload"),
        new(WrfAllowUser1, WfUser1Ok, BtUser1, "User1"),
        new(WrfAllowUser2, WfUser2Ok, BtUser2, "User2"),
        new(WrfAllowUser3, WfUser3Ok, BtUser3, "User3"),
        new(WrfAllowUser4, WfUser4Ok, BtUser4, "User4"),
    ];

    public sealed class PlayerModel
    {
        public object? ReadyWeapon { get; set; }
        public int WeaponState { get; set; }
        public int Buttons { get; set; }
    }

    public sealed class ActorModel
    {
        public PlayerModel? Player { get; set; }
    }

    public sealed class CheckContext
    {
        public PlayerModel Player { get; init; } = null!;
        public Func<object?, string, object?>? FindWeaponState { get; init; }
        public Action<PlayerModel, int, object?>? SetPsprite { get; init; }
    }

    public static void CheckWeaponButtons(CheckContext ctx)
    {
        var weapon = ctx.Player.ReadyWeapon;
        if (weapon == null)
            return;

        foreach (var check in ButtonChecks)
        {
            if ((ctx.Player.WeaponState & check.StateFlag) != 0
                && (ctx.Player.Buttons & check.ButtonFlag) != 0)
            {
                var state = ctx.FindWeaponState?.Invoke(weapon, check.StateName);
                if (state != null)
                {
                    ctx.SetPsprite?.Invoke(ctx.Player, VmPPSpriteLayers.PspWeapon, state);
                    return;
                }
            }
        }
    }

    public static void CheckWeaponButtonsAction(ActorModel actor, CheckContext ctx)
    {
        if (actor.Player == null)
            return;

        CheckWeaponButtons(new CheckContext
        {
            Player = actor.Player,
            FindWeaponState = ctx.FindWeaponState,
            SetPsprite = ctx.SetPsprite,
        });
    }
}
