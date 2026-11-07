using S = HCDE.Playsim.VmInterpolate1397Slice;

namespace HCDE.Playsim.Tests;

public class VmInterpolate1397SliceTests
{
    [Fact]
    public void RegistrationOrderAbstractFlagsAndPointerTable()
    {
        var regs = S.CreateRegistrations();
        Assert.Equal(
            new[]
            {
                "DInterpolation",
                "DSectorPlaneInterpolation",
                "DSectorScrollInterpolation",
                "DWallScrollInterpolation",
                "DPolyobjInterpolation",
            },
            regs.Select(reg => reg.Name).ToArray());

        var basis = regs[0];
        Assert.True(basis.Abstract);
        Assert.True(basis.HasPointers);
        Assert.False(S.CanConstruct(basis));
        Assert.False(basis.DefaultConstructorInBand);
        Assert.Null(basis.ParentName);
        Assert.Null(S.ParentRegistration(basis.Name));
        Assert.Null(S.StaticType(basis));
        var token = new object();
        basis.MyClass = token;
        Assert.Same(token, S.StaticType(basis));
        Assert.Equal(new[] { "Next", "Prev" }, basis.PointerFields);
        Assert.Empty(basis.Fields);

        ulong[] table = S.PublishedPointerTable(S.BaseClass, 8, 24)!;
        Assert.Equal(new ulong[] { 0, 16, ulong.MaxValue }, table);
        Assert.Equal(ulong.MaxValue, S.PointerSentinel);
        Assert.Equal(ulong.MaxValue - 7, S.FieldOffset(0));
        Assert.Null(S.PublishedPointerTable("DSectorPlaneInterpolation", 8, 16));

        foreach (var reg in regs.Skip(1))
        {
            Assert.False(reg.Abstract);
            Assert.False(reg.HasPointers);
            Assert.True(reg.DefaultConstructorInBand);
            Assert.True(S.CanConstruct(reg));
            Assert.Equal(S.BaseClass, reg.ParentName);
            Assert.Equal(S.BaseClass, S.ParentRegistration(reg.Name)!.Name);
            Assert.Equal("override", S.MethodKind(reg.Name, "UnlinkFromMap"));
            Assert.Equal("virtual", S.MethodKind(reg.Name, "Serialize"));
            Assert.False(S.ParameterizedBodyInBand(reg.Name));
        }

        Assert.True(S.Declares("DSectorPlaneInterpolation", "PropagateMark"));
        Assert.False(S.Declares("DSectorScrollInterpolation", "PropagateMark"));
        Assert.False(S.Declares("DWallScrollInterpolation", "PropagateMark"));
        Assert.False(S.Declares("DPolyobjInterpolation", "PropagateMark"));
        Assert.Throws<InvalidOperationException>(() => S.ConstructInPlace(S.BaseClass));
        Assert.Throws<InvalidOperationException>(() => S.ParameterizedBodyInBand(S.BaseClass));
        Assert.Throws<InvalidOperationException>(() => S.Find("DMissingInterpolation"));
        Assert.True(S.HeaderSetMatches(S.Headers));
        Assert.False(S.HeaderSetMatches(new[] { "p_3dmidtex.h" }));
        Assert.Equal(170, S.CountInterpolationsLine);
        Assert.True(S.CountInterpolationsLine > S.BandEnd);
    }

    [Fact]
    public void InPlaceConstructionInitializesOnlyClassMembers()
    {
        var plane = S.ConstructInPlace("DSectorPlaneInterpolation");
        Assert.Equal(new[] { "attached" }, plane.InitializedMembers);
        Assert.Equal(
            new[] { "sector", "oldheight", "oldtexz", "bakheight", "baktexz", "ceiling" },
            plane.IndeterminateMembers);
        Assert.Equal(379, S.Find("DSectorPlaneInterpolation").ParameterizedLine);

        var scroll = S.ConstructInPlace("DSectorScrollInterpolation");
        Assert.Empty(scroll.InitializedMembers);
        Assert.Equal(new[] { "sector", "oldx", "oldy", "bakx", "baky", "ceiling" }, scroll.IndeterminateMembers);
        Assert.Equal(547, S.Find("DSectorScrollInterpolation").ParameterizedLine);

        var wall = S.ConstructInPlace("DWallScrollInterpolation");
        Assert.Empty(wall.InitializedMembers);
        Assert.Equal(new[] { "side", "part", "oldx", "oldy", "bakx", "baky" }, wall.IndeterminateMembers);
        Assert.Equal(654, S.Find("DWallScrollInterpolation").ParameterizedLine);

        var poly = S.ConstructInPlace("DPolyobjInterpolation");
        Assert.Equal(new[] { "oldverts", "bakverts" }, poly.InitializedMembers);
        Assert.Equal(new[] { "poly", "oldcx", "oldcy", "bakcx", "bakcy" }, poly.IndeterminateMembers);
        Assert.Equal(753, S.Find("DPolyobjInterpolation").ParameterizedLine);
        Assert.Equal("DPolyobjInterpolation", poly.ClassName);
    }
}
