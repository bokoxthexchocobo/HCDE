using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsMeleeRangeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(8388608)]
    public void SetGetCheckPreserveFixedRange(int value)
    {
        var sim = Room(); var actor = Monster(sim);
        Run(sim, actor, [3, 0, 3, 38, 3, value, 245,
            3, 7, 3, 0, 3, 38, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.MeleeRange.Raw); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 38, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 38, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(59.9999847412109375, true)]
    [InlineData(60, false)]
    [InlineData(60.0000152587890625, false)]
    public void DefaultRangeHasStrictBoundaryAndIgnoresAttackerRadius(double distance, bool expected)
    {
        var sim = Room(); var actor = Monster(sim); var target = sim.Players.Single();
        target.X = Fixed.FromDouble(distance); actor.Radius = Fixed.FromInt(100);
        Assert.Equal(44, actor.MeleeRange.ToDouble());
        Assert.Equal(expected, MonsterBrain.CheckMeleeRange(actor, target, true));
    }

    [Theory]
    [InlineData(56, true)]
    [InlineData(56.0000152587890625, false)]
    [InlineData(-56, true)]
    [InlineData(-56.0000152587890625, false)]
    public void VerticalContactIsInclusiveButSeparationFails(double z, bool expected)
    {
        var sim = Room(); var actor = Monster(sim); var target = sim.Players.Single();
        target.X = Fixed.FromInt(50); actor.Height = target.Height = Fixed.FromInt(56);
        target.Z = Fixed.FromDouble(z);
        Assert.Equal(expected, MonsterBrain.CheckMeleeRange(actor, target, true));
    }

    [Fact]
    public void FriendsAndOccludedTargetsAreRejected()
    {
        var sim = Room(); var actor = Monster(sim); var target = sim.Players.Single(); target.X = Fixed.FromInt(50);
        Assert.False(MonsterBrain.CheckMeleeRange(actor, target, false));
        actor.Friendly = target.Friendly = true;
        Assert.False(MonsterBrain.CheckMeleeRange(actor, target, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptRangeEnablesMeleeWindupAtPreviouslyUnreachableDistance(bool extended)
    {
        var sim = Room(); var actor = Monster(sim); actor.ChaseSpeed = 0;
        if (extended) Run(sim, actor, [3, 0, 3, 38, 3, 128 * 65536, 245, 1]);
        for (var i = 0; i < 20 && actor.Brain!.Mode != MonsterMode.Windup; i++) sim.Tick();
        Assert.Equal(extended, actor.Brain!.Mode == MonsterMode.Windup);
    }

    [Fact]
    public void RangeParticipatesInChecksumAtRest()
    {
        var first = Room(); var second = Room(); Monster(second).MeleeRange = Fixed.FromInt(128);
        Monster(first).Brain!.Enabled = Monster(second).Brain!.Enabled = false;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void ChangingRangeDuringWindupPreventsMeleeDamage()
    {
        var sim = Room(); var actor = Monster(sim); actor.ChaseSpeed = 0;
        Run(sim, actor, [3, 0, 3, 38, 3, 128 * 65536, 245, 1]);
        for (var i = 0; i < 20 && actor.Brain!.Mode != MonsterMode.Windup; i++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, actor.Brain!.Mode);
        Run(sim, actor, [3, 0, 3, 38, 3, 0, 245, 1]);
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetsIgnoreSetterAndReturnZero(bool destroyed)
    {
        var sim = Room(); var actor = Monster(sim); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 38, 3, 12345, 245,
            3, 7, 3, tid, 3, 38, 246, 5, 112, 1]);
        Assert.Equal(44 * 65536, actor.MeleeRange.Raw); Assert.Equal(0, sim.LightOf(0));
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.DoomEdNum == 3002);
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 3002 }, new LevelThing { Type = 1, X = 100 }],
    });
}
