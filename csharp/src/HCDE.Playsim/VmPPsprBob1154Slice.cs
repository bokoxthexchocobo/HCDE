namespace HCDE.Playsim;

/// <summary>
/// Weapon raise, bob, and extra button checks from <c>p_pspr.cpp</c> (<b>640–834</b>).
/// Chunk <b>1154</b>. Overlay writers begin at <b>835</b>.
/// </summary>
public static class VmPPsprBob1154Slice
{
    public const int Euthanize = 1 << 5;
    public const int WeaponReloadOk = 1 << 5;
    public const int WeaponZoomOk = 1 << 6;
    public const int User1Ok = 1 << 8;
    public const int User2Ok = 1 << 9;
    public const int User3Ok = 1 << 10;
    public const int User4Ok = 1 << 11;
    public const int ReloadButton = 1 << 6;
    public const int ZoomButton = 1 << 7;
    public const int User1Button = 1 << 21;
    public const int User2Button = 1 << 22;
    public const int User3Button = 1 << 23;
    public const int User4Button = 1 << 24;
    public const int PspNone = 0;
    public const int PspFlat = 1;
    public const int PspSpatial = 2;
    public const int KeepX = 1;
    public const int KeepY = 1 << 1;
    public const int Add = 1 << 2;
    public const int Interpolate = 1 << 3;
    public const int Relative = 1 << 4;
    public const int ZeroY = 1 << 5;

    public readonly record struct BobItem(int ObjectFlags, bool Override, double X, double Y);

    public readonly record struct BobItem3(
        int ObjectFlags,
        bool Override,
        double Tx,
        double Ty,
        double Tz,
        double Rx,
        double Ry,
        double Rz);

    public readonly record struct Bob2D(
        bool Reset,
        bool Set,
        bool Interpolate2D,
        double CallFrac,
        int Timer,
        double X,
        double Y,
        int Visited);

    public readonly record struct Bob3D(
        bool Reset,
        bool Set,
        bool Interpolate2D,
        double CallFrac,
        int Timer,
        double Tx,
        double Ty,
        double Tz,
        double Rx,
        double Ry,
        double Rz,
        int Visited);

    public readonly record struct WeaponButton(int StateFlag, int ButtonFlag, bool HasState, int StateId);

    public readonly record struct ButtonHit(bool Changed, int StateId);

    public static bool BringUp(bool hasVirtual)
    {
        if (hasVirtual)
            return true;
        return false;
    }

    public static double SmoothFrac(bool smooth, double ticFrac)
    {
        if (!smooth)
            return ticFrac;
        if (ticFrac <= 0)
            return 0;
        if (ticFrac >= 1)
            return 1;
        return ticFrac * ticFrac * (3 - (2 * ticFrac));
    }

    public static Bob2D BobWeapon(bool hasVirtual, int timer, double baseX, double baseY, IReadOnlyList<BobItem> items)
    {
        if (!hasVirtual)
            return new Bob2D(true, false, false, 0, 0, baseX, baseY, 0);

        double x = baseX;
        double y = baseY;
        int visited = 0;
        foreach (BobItem item in items)
        {
            if ((item.ObjectFlags & Euthanize) != 0)
                break;
            visited++;
            if (item.Override)
            {
                x = item.X;
                y = item.Y;
            }
        }

        return new Bob2D(false, true, true, 1, timer, x, y, visited);
    }

    public static Bob3D BobWeapon3D(
        bool hasVirtual,
        int timer,
        double baseTx,
        double baseTy,
        double baseTz,
        double baseRx,
        double baseRy,
        double baseRz,
        IReadOnlyList<BobItem3> items)
    {
        if (!hasVirtual)
            return new Bob3D(true, false, false, 0, 0, baseTx, baseTy, baseTz, baseRx, baseRy, baseRz, 0);

        double tx = baseTx;
        double ty = baseTy;
        double tz = baseTz;
        double rx = baseRx;
        double ry = baseRy;
        double rz = baseRz;
        int visited = 0;
        foreach (BobItem3 item in items)
        {
            if ((item.ObjectFlags & Euthanize) != 0)
                break;
            visited++;
            if (item.Override)
            {
                tx = item.Tx;
                ty = item.Ty;
                tz = item.Tz;
                rx = item.Rx;
                ry = item.Ry;
                rz = item.Rz;
            }
        }

        return new Bob3D(false, true, false, 1, timer, tx, ty, tz, rx, ry, rz, visited);
    }

    public static ButtonHit CheckButtons(bool hasWeapon, int weaponState, int buttons, IReadOnlyList<WeaponButton> checks)
    {
        if (!hasWeapon)
            return new ButtonHit(false, 0);
        foreach (WeaponButton check in checks)
        {
            if ((weaponState & check.StateFlag) != 0 && (buttons & check.ButtonFlag) != 0 && check.HasState)
                return new ButtonHit(true, check.StateId);
        }

        return new ButtonHit(false, 0);
    }
}
