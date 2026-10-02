using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class SectorTextureAlignment
{
    public const int AlignCeiling = 183;
    public const int AlignFloor = 184;

    public static bool? Execute(AuthoritySimulation sim, int special, int lineId, int side)
    {
        if (special is not (AlignFloor or AlignCeiling)) return null;
        if (lineId == -1) return false; // Native reserves -1 for an unset line ID.
        var aligned = false;
        foreach (var line in AcsLineActivation.LinesFromId(sim, lineId))
        {
            var sideIndex = side != 0 ? line.SideBack : line.SideFront;
            if ((uint)sideIndex >= (uint)sim.Level.Sides.Count) continue;
            var sectorIndex = sim.Level.Sides[sideIndex].Sector;
            if ((uint)sectorIndex >= (uint)sim.Level.Sectors.Count) continue;
            var angle = Math.Atan2(line.Y2 - line.Y1, line.X2 - line.X1);
            var normal = angle - Math.PI / 2;
            var distance = -(Math.Cos(normal) * line.X1 + Math.Sin(normal) * line.Y1);
            if (side != 0)
            {
                angle += Math.PI;
                distance = -distance;
            }
            var baseAngle = LevelBuilder.TextureAngleFromDegrees(-angle * (180 / Math.PI));
            var sector = sim.Level.Sectors[sectorIndex];
            if (special == AlignFloor)
            {
                sector.FloorTextureBaseAngle = baseAngle;
                sector.FloorTextureBaseOffsetY = distance;
            }
            else
            {
                sector.CeilingTextureBaseAngle = baseAngle;
                sector.CeilingTextureBaseOffsetY = distance;
            }
            aligned = true;
        }
        return aligned;
    }
}
