using System.Runtime.InteropServices;

namespace HCDE.Client;

/// <summary>Native <c>r_memory.cpp</c> frame-scoped bump allocator.</summary>
public sealed class VmRenderMemoryArena : IDisposable
{
    public const int BlockSize = 1024 * 1024;
    public const int Alignment = 16;

    private readonly List<VmRenderMemoryBlock> _usedBlocks = new();
    private readonly Stack<VmRenderMemoryBlock> _freeBlocks = new();
    private VmRenderMemoryBlock? _activeBlock;
    private nuint _position;

    public nint AllocateBytes(int size)
    {
        var aligned = (size + Alignment - 1) / Alignment * Alignment;
        if (_activeBlock is null || _position + (nuint)aligned > BlockSize)
        {
            _activeBlock = _freeBlocks.Count > 0 ? _freeBlocks.Pop() : new VmRenderMemoryBlock();
            _activeBlock.Position = 0;
            _usedBlocks.Add(_activeBlock);
            _position = 0;
        }

        var pointer = _activeBlock.Data + (nint)_position;
        _position += (nuint)aligned;
        return pointer;
    }

    public void Clear()
    {
        while (_usedBlocks.Count > 0)
        {
            var block = _usedBlocks[^1];
            _usedBlocks.RemoveAt(_usedBlocks.Count - 1);
            block.Position = 0;
            _freeBlocks.Push(block);
        }

        _activeBlock = null;
        _position = 0;
    }

    public void Dispose()
    {
        Clear();
        while (_freeBlocks.Count > 0)
            _freeBlocks.Pop().Dispose();
    }

    private sealed class VmRenderMemoryBlock : IDisposable
    {
        public nint Data { get; }
        public uint Position { get; set; }

        public unsafe VmRenderMemoryBlock()
        {
            Data = (nint)NativeMemory.AlignedAlloc((nuint)BlockSize, (nuint)Alignment);
            if (Data == 0)
                throw new OutOfMemoryException("RenderMemory block allocation failed.");
        }

        public unsafe void Dispose()
        {
            if (Data != 0)
                NativeMemory.Free((void*)Data);
        }
    }
}
