namespace HCDE.Playsim;

/// <summary><c>DGlow</c> from <c>a_lights.cpp</c> (343–385).</summary>
public static class VmALightsGlow
{
    public const int GlowSpeed = 8;

    public static (int lightLevel, int direction) Tick(int direction, int current, int minLight, int maxLight)
    {
        var newLight = current;
        switch (direction)
        {
            case -1:
                newLight -= GlowSpeed;
                if (newLight <= minLight)
                {
                    newLight += GlowSpeed;
                    direction = 1;
                }
                break;
            case 1:
                newLight += GlowSpeed;
                if (newLight >= maxLight)
                {
                    newLight -= GlowSpeed;
                    direction = -1;
                }
                break;
        }

        return (newLight, direction);
    }

    public static (int min, int max, int direction) ConstructFromSector(int sectorLight, int minSurrounding) =>
        (minSurrounding, sectorLight, -1);
}
