using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LinkedFrozenPropertyTests
{
    public static IEnumerable<object[]> Modes()
    {
        foreach (var linked in new[] { false, true })
        foreach (var property in new[] { 0, 4 })
        foreach (var all in new[] { false, true })
        foreach (var value in new[] { 0, 1 })
            yield return [linked, property, all, value];
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void OnlyCompatibleAllPlayerActionsLinkBothFlags(bool linked, int property, bool all, int value)
    {
        var sim = Room(linked); var players = sim.Players.ToArray();
        foreach (var p in players) p.Frozen = p.TotallyFrozen = value == 0;
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, players[0], all ? -1 : 0, value, property));
        for (var i = 0; i < players.Length; i++)
        {
            var affected = all || i == 0;
            var frozen = affected && (property == 0 || all && linked) ? value != 0 : value == 0;
            var total = affected && (property == 4 || all && linked) ? value != 0 : value == 0;
            Assert.Equal(frozen, players[i].Frozen); Assert.Equal(total, players[i].TotallyFrozen);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsFormsReachCompatibilityRule(bool stack)
    {
        var sim = Room(true); foreach (var p in sim.Players) p.TotallyFrozen = true;
        int[] words = stack ? [3, 1, 3, 0, 3, 0, 6, 191, 1] : [11, 191, 1, 0, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], null)); sim.Acs.Tick(sim);
        Assert.All(sim.Players, p => { Assert.False(p.Frozen); Assert.False(p.TotallyFrozen); });
    }

    [Theory]
    [InlineData(MapDataFormat.HexenBinary)]
    [InlineData(MapDataFormat.UdmfText)]
    public void MapActionLinksAndSavedFlagsPersist(MapDataFormat format)
    {
        var sim = Room(true, format); var player = sim.Players.First();
        var line = new LevelLine { Special = 191, Arg0 = 1, Arg1 = 1, Arg2 = 0, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.All(sim.Players, p => { Assert.True(p.Frozen); Assert.True(p.TotallyFrozen); });
        var saved = SimSavegame.Write(sim, true);
        PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 1, 0, 4); SimSavegame.Apply(sim, saved);
        Assert.All(sim.Players, p => { Assert.True(p.Frozen); Assert.True(p.TotallyFrozen); });
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void CompatibilityDoesNotLinkUnrelatedProperties()
    {
        var sim = Room(true);
        Assert.True(PlayerPropertyActions.ExecuteSpecial(sim, 191, null, 1, 1, 21));
        Assert.All(sim.Players, p => { Assert.True(p.GodMode); Assert.False(p.Frozen); Assert.False(p.TotallyFrozen); });
    }

    private static AuthoritySimulation Room(bool linked, MapDataFormat format = MapDataFormat.HexenBinary) =>
        AuthoritySimulation.Start(new PlayLevel
        { Format = format, Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 128 }] },
            compat: linked ? CompatSurface.LinkFrozenProperties : CompatSurface.None,
            spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
