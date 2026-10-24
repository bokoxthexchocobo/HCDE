namespace HCDE.Net.Core;

/// <summary>Managed mirror of <c>FDynamicBuffer</c> (<c>d_net.cpp</c> 8343–8383).</summary>
public sealed class NetDynamicBuffer
{
    private byte[]? _data;
    private int _len;
    private int _bufferLen;

    public int Length => _len;

    public NetDynamicBuffer()
    {
        _data = null;
        _len = 0;
        _bufferLen = 0;
    }

    public void DisposeBuffer()
    {
        _data = null;
        _len = 0;
        _bufferLen = 0;
    }

    public void SetData(byte[]? data, int len)
    {
        if (len > _bufferLen)
        {
            _bufferLen = (len + 255) & ~255;
            _data = new byte[_bufferLen];
        }

        if (data is not null)
        {
            _len = len;
            if (len > 0)
                Array.Copy(data, 0, _data!, 0, len);
        }
        else
            _len = 0;
    }

    public byte[]? GetData(out int len)
    {
        len = _len;
        return _len > 0 ? _data : null;
    }
}
