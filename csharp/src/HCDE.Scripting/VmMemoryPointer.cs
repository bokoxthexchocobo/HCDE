namespace HCDE.Scripting;

/// <summary>A byte address within a managed VM allocation.</summary>
public sealed class VmMemoryPointer
{
    public object Memory { get; }
    public long ByteOffset { get; }

    public VmMemoryPointer(object memory, long byteOffset = 0)
    {
        ArgumentNullException.ThrowIfNull(memory);
        if (memory is not byte[] && memory is not VmStringMemory && memory is not VmObjectMemory)
            throw new ArgumentException("Unsupported VM allocation.", nameof(memory));
        Memory = memory;
        ByteOffset = byteOffset;
    }
}
