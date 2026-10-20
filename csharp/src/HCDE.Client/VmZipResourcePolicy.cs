using System.Buffers.Binary;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace HCDE.Client;

/// <summary>Native <c>file_zip.cpp</c> central-directory scan and entry filter policy.</summary>
public static class VmZipResourcePolicy
{
    public const ushort MethodStored = 0;
    public const ushort MethodDeflate = 8;
    public const ushort MethodLzma = 14;
    public const ushort MethodBZip2 = 12;
    public const ushort MethodImplode = 6;
    public const ushort MethodShrink = 1;
    public const ushort MethodXz = 95;
    public const ushort FlagEncrypted = 1;

    private const int BufferReadComment = 0x400;

    public static bool IsSupportedCompression(ushort method) =>
        method is MethodStored or MethodDeflate or MethodLzma or MethodBZip2
            or MethodImplode or MethodShrink or MethodXz;

    public static bool IsSupportedArchiveEntry(IArchiveEntry entry) =>
        IsSupportedCompressionType(entry.CompressionType) && !entry.IsEncrypted;

    public static bool IsSupportedCompressionType(CompressionType method) =>
        method is CompressionType.None or CompressionType.Deflate or CompressionType.LZMA
            or CompressionType.BZip2 or CompressionType.Explode or CompressionType.Shrink or CompressionType.Xz;

    public static bool TryFindEndOfCentralDirectory(ReadOnlySpan<byte> zip, out int offset, out bool zip64)
    {
        zip64 = false;
        offset = 0;
        if (zip.Length < 22)
            return false;
        var fileSize = zip.Length;
        var maxBack = Math.Min(0xffff, fileSize);
        var backRead = 4;
        while (backRead < maxBack)
        {
            backRead = backRead + BufferReadComment > maxBack ? maxBack : backRead + BufferReadComment;
            var readPos = fileSize - backRead;
            var readSize = Math.Min(BufferReadComment + 4, fileSize - readPos);
            for (var index = readSize - 4; index >= 0; index--)
            {
                if (zip[readPos + index] == (byte)'P' && zip[readPos + index + 1] == (byte)'K' &&
                    zip[readPos + index + 2] == 5 && zip[readPos + index + 3] == 6 && offset == 0)
                {
                    zip64 = false;
                    offset = (int)(readPos + index);
                }

                if (zip[readPos + index] == (byte)'P' && zip[readPos + index + 1] == (byte)'K' &&
                    zip[readPos + index + 2] == 6 && zip[readPos + index + 3] == 6)
                {
                    zip64 = true;
                    offset = (int)(readPos + index);
                    return true;
                }
            }
        }

        return offset != 0;
    }

    public static bool IsMultipartEndRecord(ReadOnlySpan<byte> zip, int endOffset, bool zip64)
    {
        if (zip64)
        {
            if (endOffset + 56 > zip.Length)
                return true;
            var span = zip[endOffset..];
            var numEntries = BinaryPrimitives.ReadUInt64LittleEndian(span[32..]);
            var numEntriesAll = BinaryPrimitives.ReadUInt64LittleEndian(span[40..]);
            var disk = BinaryPrimitives.ReadUInt32LittleEndian(span[16..]);
            var startDisk = BinaryPrimitives.ReadUInt32LittleEndian(span[20..]);
            return numEntries != numEntriesAll || disk != 0 || startDisk != 0;
        }

        if (endOffset + 22 > zip.Length)
            return true;
        var record = zip[endOffset..];
        var entries = BinaryPrimitives.ReadUInt16LittleEndian(record[10..]);
        var entriesAll = BinaryPrimitives.ReadUInt16LittleEndian(record[8..]);
        var diskNumber = BinaryPrimitives.ReadUInt16LittleEndian(record[4..]);
        var firstDisk = BinaryPrimitives.ReadUInt16LittleEndian(record[6..]);
        return entries != entriesAll || diskNumber != 0 || firstDisk != 0;
    }
}
