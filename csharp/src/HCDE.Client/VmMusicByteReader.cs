namespace HCDE.Client;

public sealed class VmMusicByteReader : IDisposable
{
    private readonly byte[] data;
    private int position;
    private bool disposed;

    public VmMusicByteReader(ReadOnlySpan<byte> data) => this.data = data.ToArray();

    public long Tell()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return position;
    }

    public int Read(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var count = Math.Min(destination.Length, data.Length - position);
        data.AsSpan(position, count).CopyTo(destination);
        position += count;
        return count;
    }

    public int Seek(long offset, SeekOrigin origin)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var basis = origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => position,
            SeekOrigin.End => data.Length,
            _ => throw new ArgumentOutOfRangeException(nameof(origin)),
        };
        if (offset < -basis || offset > data.Length - basis) return -1;
        position = basis + (int)offset;
        return 0;
    }

    public int? Gets(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var budget = Math.Min(destination.Length, data.Length - position);
        if (budget <= 0) return null;
        var written = 0;
        while (budget > 1 && position < data.Length)
        {
            var value = data[position++];
            if (value == 0) break;
            if (value == '\r') continue;
            destination[written++] = value;
            budget--;
            if (value == '\n') break;
        }
        if (written == 0) return null;
        destination[written] = 0;
        return written;
    }

    public int? GetsFileLine(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (destination.Length < 2) throw new ArgumentException("A file line buffer requires space for data and a terminator.", nameof(destination));
        if (position == data.Length) return null;
        var written = 0;
        while (written < destination.Length - 1 && position < data.Length)
        {
            var value = data[position++];
            destination[written++] = value;
            if (value == '\n') break;
        }
        destination[written] = 0;
        return written;
    }

    public void Dispose() => disposed = true;
}
