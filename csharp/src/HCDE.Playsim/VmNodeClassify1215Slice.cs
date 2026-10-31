namespace HCDE.Playsim;

/// <summary>
/// Splitter side test from <c>nodebuild_classify_nosse2.cpp</c> (<b>1–159</b>).
/// Chunk <b>1215</b>. Coordinates are <c>fixed_t</c> integers widened to double.
/// A point inside <c>SIDE_EPSILON</c> is on the splitter. Both on-line endpoints use the splitter direction.
/// </summary>
public static class VmNodeClassify1215Slice
{
    public const double SideEpsilon = 6.5536;
    public const double FarEnough = 17179869184.0;

    public static (int Result, int Side0, int Side1) Classify(
        int x, int y, int dx, int dy, int v1x, int v1y, int v2x, int v2y)
    {
        double dX1 = x;
        double dY1 = y;
        double dDx = dx;
        double dDy = dy;
        double dXv1 = v1x;
        double dXv2 = v2x;
        double dYv1 = v1y;
        double dYv2 = v2y;
        double sNum1 = (dY1 - dYv1) * dDx - (dX1 - dXv1) * dDy;
        double sNum2 = (dY1 - dYv2) * dDx - (dX1 - dXv2) * dDy;
        int side0;
        int side1;
        int nears = 0;

        if (sNum1 <= -FarEnough)
        {
            if (sNum2 <= -FarEnough)
                return (1, 1, 1);
            if (sNum2 >= FarEnough)
                return (-1, 1, -1);
            nears = 1;
        }
        else if (sNum1 >= FarEnough)
        {
            if (sNum2 >= FarEnough)
                return (0, -1, -1);
            if (sNum2 <= -FarEnough)
                return (-1, -1, 1);
            nears = 1;
        }
        else
        {
            nears = 2 | (Math.Abs(sNum2) < FarEnough ? 1 : 0);
        }

        if (nears != 0)
        {
            double length = 1.0 / (dDx * dDx + dDy * dDy);
            if ((nears & 2) != 0)
            {
                double dist = sNum1 * sNum1 * length;
                if (dist < SideEpsilon * SideEpsilon)
                    side0 = 0;
                else
                    side0 = sNum1 > 0.0 ? -1 : 1;
            }
            else
            {
                side0 = sNum1 > 0.0 ? -1 : 1;
            }

            if ((nears & 1) != 0)
            {
                double dist = sNum2 * sNum2 * length;
                if (dist < SideEpsilon * SideEpsilon)
                    side1 = 0;
                else
                    side1 = sNum2 > 0.0 ? -1 : 1;
            }
            else
            {
                side1 = sNum2 > 0.0 ? -1 : 1;
            }
        }
        else
        {
            side0 = sNum1 > 0.0 ? -1 : 1;
            side1 = sNum2 > 0.0 ? -1 : 1;
        }

        if ((side0 | side1) == 0)
        {
            if (dx != 0)
            {
                if ((dx > 0 && v2x > v1x) || (dx < 0 && v2x < v1x))
                    return (0, side0, side1);
                return (1, side0, side1);
            }

            if ((dy > 0 && v2y > v1y) || (dy < 0 && v2y < v1y))
                return (0, side0, side1);
            return (1, side0, side1);
        }

        if (side0 <= 0 && side1 <= 0)
            return (0, side0, side1);
        if (side0 >= 0 && side1 >= 0)
            return (1, side0, side1);
        return (-1, side0, side1);
    }
}
