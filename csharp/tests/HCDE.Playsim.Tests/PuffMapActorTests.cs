using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffMapActorTests
{
    [Fact]
    public void PuffIsMapActorEvenWithoutBlockmapCollision()
    {
        var puff = new PuffActor();
        Assert.True(puff.IsMapActor()); Assert.False(puff.IsBlockmapActor);
        Assert.False(puff.Solid); Assert.False(puff.Shootable);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void AcsCountsLivePuffAndSkipsDestroyedPuff(bool destroyed, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
                SideFront = 0, SideBack = 0, Flags = LevelLine.BlockHitscanFlag }],
            Things = [new LevelThing { Type = 1, X = 32 }],
        });
        Assert.True(AcsCallFunctions.TryInvoke(sim, new List<int> { 0, 0, 0, 10 },
            new AcsActivatorBinding { Value = sim.Players.Single() }, [], new AcsGlobalStrings(),
            AcsCallFunctions.LineAttack, 4, out _));
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        if (destroyed) puff.Destroy();
        int[] words = [(int)AcsPcode.ThingCountDirect, 0, 0,
            (int)AcsPcode.PushNumber, expected, (int)AcsPcode.Eq, (int)AcsPcode.IfGoto, 36,
            (int)AcsPcode.Terminate, (int)AcsPcode.Lspec1Direct, LineSpecials.ExitNormal, 0];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code }); sim.Acs.Enqueue(1);
        sim.Tick(); Assert.True(sim.Exited);
    }
}
