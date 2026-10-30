namespace HCDE.Playsim.Tests;

public class VmPostShader1254SliceTests
{
    const string EnableGolden =
"""
enabled Bloom 1 n=2
enabled bloom 0 n=1
enabled missing 1 n=0

""";

    const string UniformGolden =
"""
u4f Bloom k n=2 1.500000 -2.000000 3.000000 4.000000
u1f Bloom k n=2 0.250000 0.000000 0.000000 1.000000
u2f Bloom uv n=2 0.500000 0.750000 0.000000 1.000000
u3f Tonemap rgb n=1 0.100000 0.200000 0.300000 1.000000
u1i Bloom k n=2 -7.000000 0.000000 0.000000 1.000000
u1f Nope k n=0 5.000000 0.000000 0.000000 1.000000
u1f Bloom K n=2 1.000000 0.000000 0.000000 1.000000

""";

    [Fact]
    public void Enabled_UpdatesEveryCaseSensitiveMatch()
    {
        var vm = new VmPostShader1254();
        var a = new VmPostShader1254.Shader { Name = "Bloom" };
        var a2 = new VmPostShader1254.Shader { Name = "Bloom" };
        var b = new VmPostShader1254.Shader { Name = "bloom" };
        var c = new VmPostShader1254.Shader { Name = "Tonemap" };
        vm.Shaders.Add(a);
        vm.Shaders.Add(a2);
        vm.Shaders.Add(b);
        vm.Shaders.Add(c);
        Assert.Equal(0, vm.SetEnabled("Bloom", true));
        Assert.Equal(0, vm.SetEnabled("bloom", false));
        Assert.Equal(0, vm.SetEnabled("missing", true));
        Assert.True(a.Enabled);
        Assert.True(a2.Enabled);
        Assert.False(b.Enabled);
        Assert.False(c.Enabled);
        Assert.Equal(EnableGolden, string.Join('\n', vm.Log) + "\n");
    }

    [Fact]
    public void Uniform_FillsUnusedLanesAndLeavesTheTypeAlone()
    {
        var vm = new VmPostShader1254();
        var a = new VmPostShader1254.Shader { Name = "Bloom" };
        var a2 = new VmPostShader1254.Shader { Name = "Bloom" };
        var b = new VmPostShader1254.Shader { Name = "bloom" };
        var c = new VmPostShader1254.Shader { Name = "Tonemap" };
        vm.Shaders.Add(a);
        vm.Shaders.Add(a2);
        vm.Shaders.Add(b);
        vm.Shaders.Add(c);
        var keep = new VmPostShader1254.Uniform { Type = 2, Values = new[] { 9.0, 8.0, 7.0, 6.0 } };
        a.Uniforms["k"] = keep;
        Assert.Equal(0, vm.SetUniform4f("Bloom", "k", 1.5, -2, 3, 4));
        Assert.Equal(0, vm.SetUniform1f("Bloom", "k", 0.25));
        Assert.Equal(0, vm.SetUniform2f("Bloom", "uv", 0.5, 0.75));
        Assert.Equal(0, vm.SetUniform3f("Tonemap", "rgb", 0.1, 0.2, 0.3));
        Assert.Equal(0, vm.SetUniform1i("Bloom", "k", -7));
        Assert.Equal(0, vm.SetUniform1f("Nope", "k", 5));
        Assert.Equal(0, vm.SetUniform1f("Bloom", "K", 1));
        Assert.Equal(2, keep.Type);
        Assert.Equal(new[] { -7d, 0d, 0d, 1d }, keep.Values);
        Assert.Equal(0, a2.Uniforms["k"].Type);
        Assert.Equal(new[] { -7d, 0d, 0d, 1d }, a2.Uniforms["k"].Values);
        Assert.Equal(new[] { 0.5, 0.75, 0d, 1d }, a.Uniforms["uv"].Values);
        Assert.Equal(new[] { 0.5, 0.75, 0d, 1d }, a2.Uniforms["uv"].Values);
        Assert.Equal(0, a.Uniforms["K"].Type);
        Assert.Equal(new[] { 1d, 0d, 0d, 1d }, a.Uniforms["K"].Values);
        Assert.Empty(b.Uniforms);
        Assert.Equal(new[] { 0.1, 0.2, 0.3, 1d }, c.Uniforms["rgb"].Values);
        Assert.False(c.Uniforms.ContainsKey("k"));
        Assert.Equal(UniformGolden, string.Join('\n', vm.Log) + "\n");
    }
}
