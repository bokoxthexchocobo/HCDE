namespace HCDE.Gamedata;

/// <summary>MBF pointer emitters from <c>d_dehacked.cpp</c> (936–1098).</summary>
public static class VmDehackedMbfPointerEmitters
{
    public static (float angle, int range) FindTracerArgs(float arg0, int arg1) => (arg0, arg1 == 0 ? 10 : arg1);

    public static (int a, int b, int c, int d, int e) SeekerMissileArgs(int a, int b, int c, int d, int e) =>
        (a, b, c, d == 0 ? 50 : d, e == 0 ? 10 : e);

    public static float Tracer2Angle(float arg) => arg == 0 ? 19.6875f : (float)arg;

    public static (int a, int b) FlagSetArgs(int a, int b) => (a, b);

    public static (int spread, int numBullets, int damage) WeaponBulletDefaults(int spread, int num, int dmg) =>
        (spread, num == 0 ? 5 : num, dmg == 0 ? 3 : dmg);
}
