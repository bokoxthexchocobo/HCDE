using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class SnapshotChecksumCodec
{
    public static int Write(Span<byte> output, uint gameTic, ReadOnlySpan<uint> categoryHashes)
    {
        if (output.Length < LiveConstants.SnapshotChecksumBlockSize
            || categoryHashes.Length != LiveConstants.SnapshotChecksumCategoryCount)
            return 0;

        LiveConstants.SnapshotChecksumMagic.CopyTo(output);
        output[4] = LiveConstants.SnapshotChecksumProtocolVersion;
        output[5] = LiveConstants.SnapshotChecksumCategoryCount;
        BinaryPrimitives.WriteUInt32LittleEndian(output[6..], gameTic);

        var cursor = 10;
        foreach (var hash in categoryHashes)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(output[cursor..], hash);
            cursor += 4;
        }

        return LiveConstants.SnapshotChecksumBlockSize;
    }

    public static bool TryRead(
        ReadOnlySpan<byte> chunk,
        out uint gameTic,
        out uint[] categoryHashes,
        out int bytesConsumed,
        out string? rejectReason)
    {
        gameTic = 0;
        categoryHashes = Array.Empty<uint>();
        bytesConsumed = 0;
        rejectReason = null;

        if (chunk.Length < LiveConstants.SnapshotChecksumBlockSize)
        {
            rejectReason = "checksum-truncated";
            return false;
        }

        if (!chunk[..4].SequenceEqual(LiveConstants.SnapshotChecksumMagic))
        {
            rejectReason = "checksum-magic-mismatch";
            return false;
        }

        if (chunk[4] != LiveConstants.SnapshotChecksumProtocolVersion
            || chunk[5] != LiveConstants.SnapshotChecksumCategoryCount)
        {
            rejectReason = "checksum-header-invalid";
            return false;
        }

        gameTic = BinaryPrimitives.ReadUInt32LittleEndian(chunk[6..]);
        categoryHashes = new uint[LiveConstants.SnapshotChecksumCategoryCount];
        var cursor = 10;
        for (var i = 0; i < categoryHashes.Length; i++)
        {
            categoryHashes[i] = BinaryPrimitives.ReadUInt32LittleEndian(chunk[cursor..]);
            cursor += 4;
        }

        bytesConsumed = LiveConstants.SnapshotChecksumBlockSize;
        return true;
    }
}
