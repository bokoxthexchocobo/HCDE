namespace HCDE.Playsim.Tests;

public class VmPMapUtlBlockThingsIteratorTests
{
    [Fact]
    public void Next_ReturnsSingleBlockActorWithoutHashing()
    {
        var actor = new object();
        var blockHead = new object();
        var node = new VmPMapUtlBlockThingsIterator.BlockNode
        {
            Me = actor,
            ActorBlockNodeHead = blockHead,
            PrevBlock = blockHead,
        };
        var state = new VmPMapUtlBlockThingsIterator.IteratorState
        {
            MinX = 0,
            MaxX = 0,
            MinY = 0,
            MaxY = 0,
            CurX = -1,
            CurY = 0,
        };
        VmPMapUtlBlockThingsIterator.StartBlock(state, 0, 0, (_, _) => true, (_, _) => node);
        Assert.Same(actor, VmPMapUtlBlockThingsIterator.Next(state, false, (_, _) => true, (_, _) => null, _ => 0, _ => 0));
    }

    [Fact]
    public void Next_SkipsDuplicateActorUsingHash()
    {
        var actor = new object();
        var other = new object();
        var n2 = new VmPMapUtlBlockThingsIterator.BlockNode { Me = other, NextBlock = new object(), PrevBlock = new object() };
        var n1 = new VmPMapUtlBlockThingsIterator.BlockNode { Me = actor, NextActor = n2, NextBlock = new object(), PrevBlock = new object() };
        var state = new VmPMapUtlBlockThingsIterator.IteratorState
        {
            MinX = 0,
            MaxX = 0,
            MinY = 0,
            MaxY = 0,
            CurX = -1,
            CurY = 0,
        };
        VmPMapUtlBlockThingsIterator.StartBlock(state, 0, 0, (_, _) => true, (_, _) => n1);
        Assert.Same(actor, VmPMapUtlBlockThingsIterator.Next(state, false, (_, _) => true, (_, _) => null, _ => 0, _ => 0));
        Assert.Same(other, VmPMapUtlBlockThingsIterator.Next(state, false, (_, _) => true, (_, _) => null, _ => 0, _ => 0));
        Assert.Null(VmPMapUtlBlockThingsIterator.Next(state, false, (_, _) => true, (_, _) => null, _ => 0, _ => 0));
    }

    [Fact]
    public void Next_CenterOnlyFiltersToBlockInterior()
    {
        var inside = new object();
        var node = new VmPMapUtlBlockThingsIterator.BlockNode
        {
            Me = inside,
            NextBlock = new object(),
            PrevBlock = new object(),
        };
        var state = new VmPMapUtlBlockThingsIterator.IteratorState
        {
            MinX = 0,
            MaxX = 0,
            MinY = 0,
            MaxY = 0,
            CurX = -1,
            CurY = 0,
            BmapOriginX = 0,
            BmapOriginY = 0,
        };
        VmPMapUtlBlockThingsIterator.StartBlock(state, 0, 0, (_, _) => true, (_, _) => node);
        var found = VmPMapUtlBlockThingsIterator.Next(state, true, (_, _) => true, (_, _) => null, _ => 64, _ => 64);
        Assert.Same(inside, found);
    }
}
