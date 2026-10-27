namespace HCDE.Playsim.Tests;

public class VmActorDoDamageRelationsTests
{
    [Fact]
    public void ShouldDamageMaster_WhenNonNull() =>
        Assert.True(VmActorDoDamageRelations.ShouldDamageMaster(new object()));

    [Fact]
    public void IsChildOfMaster_SameReference() {
        var self = new object();
        Assert.True(VmActorDoDamageRelations.IsChildOfMaster(self, self));
    }

    [Fact]
    public void ShouldRunSiblingDamageLoop_WhenMasterSet() =>
        Assert.True(VmActorDoDamageRelations.ShouldRunSiblingDamageLoop(new object()));

    [Fact]
    public void SiblingDamageCandidate_MatchesKillHelper() =>
        Assert.True(VmActorDoDamageRelations.SiblingDamageCandidate(true, true, true));

    [Fact]
    public void ShouldDamageChildInIterator_WhenMasterIsSelf() {
        var self = new object();
        Assert.True(VmActorDoDamageRelations.ShouldDamageChildInIterator(self, self));
    }
}
