namespace HCDE.Playsim.Tests;

public class VmVkRaytrace1300SliceTests
{
    [Fact]
    public void NullMesh_SkipsTheIdleAndBuildsFourTriangles()
    {
        var host = new VmVkRaytrace1300.Host { Raytracing = true };
        var ray = new VmVkRaytrace1300(host);
        Assert.Same(ray.NullMesh, ray.Current);
        Assert.Equal(12, ray.NullMesh.Vertices.Count);
        Assert.Equal(new VmVkRaytrace1300.Vertex(-1, -1, -1), ray.NullMesh.Vertices[0]);
        Assert.Equal(new VmVkRaytrace1300.Vertex(1, 1, -1), ray.NullMesh.Vertices[2]);
        Assert.Equal(new VmVkRaytrace1300.Vertex(-1, -1, 1), ray.NullMesh.Vertices[6]);
        Assert.Equal(new VmVkRaytrace1300.Vertex(1, 1, 1), ray.NullMesh.Vertices[11]);
        Assert.Equal(Enumerable.Range(0, 12), ray.NullMesh.Elements);
        Assert.Equal(0, host.Idles);
        Assert.Equal(1, host.Waits);
        Assert.False(host.LastWaitFinish);
        Assert.True(host.LastWaitUpload);
        Assert.Equal(144, ray.VertexBuffer!.Size);
        Assert.Equal(48, ray.IndexBuffer!.Size);
        Assert.Equal(192, ray.TransferBuffer!.Size);
        Assert.Equal(VmVkRaytrace1300.MemoryCpuOnly, ray.TransferBuffer.Memory);
        Assert.Equal(-1, ray.VertexBuffer.Memory);
        Assert.Equal(VmVkRaytrace1300.UsageTransferSrc, ray.TransferBuffer.Usage);
        Assert.Equal(VmVkRaytrace1300.UsageVertex | VmVkRaytrace1300.UsageTransferDst | VmVkRaytrace1300.UsageShaderAddress | VmVkRaytrace1300.UsageAccelRead, ray.VertexBuffer.Usage);
        Assert.Equal(VmVkRaytrace1300.UsageIndex | VmVkRaytrace1300.UsageTransferDst | VmVkRaytrace1300.UsageShaderAddress | VmVkRaytrace1300.UsageAccelRead, ray.IndexBuffer.Usage);
        Assert.Equal(new VmVkRaytrace1300.Copy("transferBuffer", "vertexBuffer", 0, 0, 144), ray.Copies[0]);
        Assert.Equal(new VmVkRaytrace1300.Copy("transferBuffer", "indexBuffer", 144, 0, 48), ray.Copies[1]);
        Assert.Equal(new VmVkRaytrace1300.Barrier(VmVkRaytrace1300.AccessTransferWrite, VmVkRaytrace1300.AccessShaderRead, VmVkRaytrace1300.StageTransfer, VmVkRaytrace1300.StageAccelBuild), ray.Barriers[0]);

        VmVkRaytrace1300.Build blas = ray.Builds[0];
        Assert.Equal(("blas", 4), ray.Queries[0]);
        Assert.Equal(VmVkRaytrace1300.AccelBottom, blas.Type);
        Assert.Equal(VmVkRaytrace1300.GeometryTriangles, blas.Geometry);
        Assert.Equal(VmVkRaytrace1300.GeometryOpaque, blas.GeometryFlags);
        Assert.Equal(VmVkRaytrace1300.BuildFastTrace, blas.Flags);
        Assert.Equal(VmVkRaytrace1300.BuildModeBuild, blas.Mode);
        Assert.Equal(4, blas.PrimitiveCount);
        Assert.Equal(11, blas.MaxVertex);
        Assert.Equal(VmVkRaytrace1300.VertexStride, blas.VertexStride);
        Assert.Equal(VmVkRaytrace1300.FormatRgb32Float, blas.VertexFormat);
        Assert.Equal(VmVkRaytrace1300.IndexUint32, blas.IndexType);
        Assert.Equal(ray.VertexBuffer.Address, blas.VertexAddress);
        Assert.Equal(ray.IndexBuffer.Address, blas.IndexAddress);
        Assert.Equal(1004, ray.BlStorage!.Size);
        Assert.Equal(54, ray.BlScratch!.Size);
        Assert.Equal(VmVkRaytrace1300.UsageAccelStorage | VmVkRaytrace1300.UsageShaderAddress, ray.BlStorage.Usage);
        Assert.Equal(VmVkRaytrace1300.UsageShaderAddress | VmVkRaytrace1300.UsageStorage, ray.BlScratch.Usage);
        Assert.Equal(new VmVkRaytrace1300.Barrier(VmVkRaytrace1300.AccessAccelWrite, VmVkRaytrace1300.AccessAccelRead, VmVkRaytrace1300.StageAccelBuild, VmVkRaytrace1300.StageAccelBuild), ray.Barriers[1]);

        Assert.Equal(1f, ray.TopInstance!.Matrix[0]);
        Assert.Equal(1f, ray.TopInstance.Matrix[5]);
        Assert.Equal(1f, ray.TopInstance.Matrix[10]);
        Assert.Equal(0f, ray.TopInstance.Matrix[1]);
        Assert.Equal(0xff, ray.TopInstance.Mask);
        Assert.Equal(VmVkRaytrace1300.InstanceCullDisable, ray.TopInstance.Flags);
        Assert.Equal(ray.Blas!.Address, ray.TopInstance.BlasAddress);
        Assert.Equal(VmVkRaytrace1300.InstanceBytes, ray.TlTransfer!.Size);
        Assert.Equal(VmVkRaytrace1300.InstanceBytes, ray.TlInstance!.Size);
        Assert.Equal(VmVkRaytrace1300.UsageShaderAddress | VmVkRaytrace1300.UsageAccelRead | VmVkRaytrace1300.UsageTransferDst, ray.TlInstance.Usage);
        Assert.Equal(new VmVkRaytrace1300.Copy("tlTransferBuffer", "tlInstanceBuffer", 0, 0, 64), ray.Copies[2]);
        VmVkRaytrace1300.Build tlas = ray.Builds[1];
        Assert.Equal(("tlas", 1), ray.Queries[1]);
        Assert.Equal(VmVkRaytrace1300.AccelTop, tlas.Type);
        Assert.Equal(VmVkRaytrace1300.GeometryInstances, tlas.Geometry);
        Assert.Equal(1, tlas.PrimitiveCount);
        Assert.Equal(2001, tlas.AccelSize);
        Assert.Equal(81, tlas.ScratchSize);
        Assert.Equal(ray.TlInstance.Address, tlas.InstanceAddress);
        Assert.Equal(new VmVkRaytrace1300.Barrier(VmVkRaytrace1300.AccessAccelWrite, VmVkRaytrace1300.AccessShaderRead, VmVkRaytrace1300.StageAccelBuild, VmVkRaytrace1300.StageFragment), ray.Barriers[3]);
        Assert.Equal("wait:upload", ray.Trace[^1]);
        int waits = host.Waits;
        ray.SetLevelMesh(null);
        Assert.Equal(0, host.Idles);
        Assert.Equal(waits, host.Waits);
        Assert.Same(ray.NullMesh, ray.Current);
    }

    [Fact]
    public void MeshChange_IdlesBeforeResetAndSkipsTheBuildWhenRaytracingIsOff()
    {
        var host = new VmVkRaytrace1300.Host { Raytracing = true };
        var ray = new VmVkRaytrace1300(host);
        VmVkRaytrace1300.Buffer firstVertex = ray.VertexBuffer!;
        var custom = new VmVkRaytrace1300.Mesh();
        custom.Vertices.Add(new VmVkRaytrace1300.Vertex(2, 3, 4));
        custom.Vertices.Add(new VmVkRaytrace1300.Vertex(5, 6, 7));
        custom.Vertices.Add(new VmVkRaytrace1300.Vertex(8, 9, 10));
        custom.Elements.Add(0);
        custom.Elements.Add(1);
        custom.Elements.Add(2);
        custom.Elements.Add(2);
        ray.SetLevelMesh(custom);
        Assert.Equal(1, host.Idles);
        Assert.Equal(2, host.Waits);
        Assert.True(host.LastWaitUpload);
        Assert.False(host.LastWaitFinish);
        Assert.False(firstVertex.Alive);
        Assert.Same(custom, ray.Current);
        Assert.Equal(36, ray.VertexBuffer!.Size);
        Assert.Equal(16, ray.IndexBuffer!.Size);
        Assert.Equal(2, ray.Builds[^2].MaxVertex);
        Assert.Equal(1, ray.Builds[^2].PrimitiveCount);
        Assert.Equal(1001, ray.BlStorage!.Size);
        int retireAt = ray.Trace.IndexOf("idle") + 1;
        Assert.Equal("retire:vertexBuffer", ray.Trace[retireAt]);
        Assert.Equal("retire:indexBuffer", ray.Trace[retireAt + 1]);
        Assert.Equal("retire:transferBuffer", ray.Trace[retireAt + 2]);
        Assert.Equal("retire:blScratchBuffer", ray.Trace[retireAt + 3]);
        Assert.Equal("retire:blAccelStructBuffer", ray.Trace[retireAt + 4]);
        Assert.Equal("retire:blAccelStruct", ray.Trace[retireAt + 5]);
        Assert.Equal("retire:tlTransferBuffer", ray.Trace[retireAt + 6]);
        Assert.Equal("retire:tlScratchBuffer", ray.Trace[retireAt + 7]);
        Assert.Equal("retire:tlInstanceBuffer", ray.Trace[retireAt + 8]);
        Assert.Equal("retire:tlAccelStructBuffer", ray.Trace[retireAt + 9]);
        Assert.Equal("retire:tlAccelStruct", ray.Trace[retireAt + 10]);
        int idles = host.Idles;
        ray.SetLevelMesh(custom);
        Assert.Equal(idles, host.Idles);
        Assert.Equal(2, host.Waits);

        host.Raytracing = false;
        var other = new VmVkRaytrace1300.Mesh();
        other.Vertices.Add(new VmVkRaytrace1300.Vertex(0, 0, 0));
        other.Elements.Add(0);
        VmVkRaytrace1300.Buffer liveVertex = ray.VertexBuffer;
        ray.SetLevelMesh(other);
        Assert.Equal(2, host.Idles);
        Assert.Equal(2, host.Waits);
        Assert.Same(other, ray.Current);
        Assert.Null(ray.VertexBuffer);
        Assert.Null(ray.Tlas);
        Assert.True(liveVertex.Queued);
        Assert.True(liveVertex.Alive);
        Assert.Contains(liveVertex, ray.DeleteList);
        int builds = ray.Builds.Count;
        ray.SetLevelMesh(other);
        Assert.Equal(2, host.Idles);
        Assert.Equal(builds, ray.Builds.Count);

        host.Raytracing = true;
        ray.SetLevelMesh(null);
        Assert.Equal(3, host.Idles);
        Assert.Equal(3, host.Waits);
        Assert.Same(ray.NullMesh, ray.Current);
        Assert.Equal(4, ray.Builds[^2].PrimitiveCount);
        Assert.False(liveVertex.Alive);
        Assert.Equal("wait:upload", ray.Trace[^1]);
    }
}
