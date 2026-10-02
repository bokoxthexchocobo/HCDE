using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TextureTransformChecksumTests
{
    [Fact]
    public void EveryWallAndPlaneTransformParticipatesInChecksum()
    {
        foreach (var type in new[] { typeof(LevelSide), typeof(LevelSector) })
        foreach (var property in type.GetProperties().Where(p => p.Name.Contains("Texture")
            && (p.PropertyType == typeof(double) || p.PropertyType == typeof(uint))))
        {
            var baseline = Room();
            var changed = Room();
            var target = type == typeof(LevelSide) ? (object)changed.Level.Sides[0] : changed.Level.Sectors[0];
            var previous = property.GetValue(target);
            property.SetValue(target, property.PropertyType == typeof(double)
                ? (object)((double)previous! + 0.25) : (uint)previous! + 1u);
            baseline.Tick();
            changed.Tick();
            Assert.True(baseline.Checksum != changed.Checksum, $"Missing checksum field: {type.Name}.{property.Name}");
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide()],
    });
}
