namespace HCDE.Client;

/// <summary>Native <c>r_memory.cpp</c> frame-scoped bump allocator.</summary>
public sealed class VmRenderMemoryArena
{
    public const int BlockSize = 1024 * 1024;
    public const int Alignment = 16;

    private readonly List<byte[]> _usedBlocks = new();
    private readonly Stack<byte[]> _freeBlocks = new();
    private int _position;

    public int AllocateBytes(int size)
    {
        var aligned = (size + Alignment - 1) / Alignment * Alignment;
        if (_usedBlocks.Count == 0 || _position + aligned > BlockSize)
        {
            if (_freeBlocks.Count > 0)
            {
                var block = _freeBlocks.Pop();
                _usedBlocks.Add(block);
            }
            else
            {
                _usedBlocks.Add(new byte[BlockSize]);
            }

            _position = 0;
        }

        var offset = _position;
        _position += aligned;
        return offset;
    }

    public ReadOnlySpan<byte> GetBlockSpan(int blockIndex) => _usedBlocks[blockIndex];

    public int ActiveBlockIndex => _usedBlocks.Count - 1;

    public void Clear()
    {
        while (_usedBlocks.Count > 0)
        {
            var block = _usedBlocks[^1];
            _usedBlocks.RemoveAt(_usedBlocks.Count - 1);
            _freeBlocks.Push(block);
        }

        _position = 0;
    }
}
