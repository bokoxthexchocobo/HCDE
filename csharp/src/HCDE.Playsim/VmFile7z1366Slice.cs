namespace HCDE.Playsim;

/// <summary>
/// 7z stream, archive object, and <c>F7ZFile::Open</c> through the file count
/// from <c>file_7z.cpp</c> lines 1-235. The directory loop at line 236 is not this slice.
/// </summary>
public sealed class VmFile7z1366Slice
{
    public const int SzOk = 0;
    public const int SzErrorData = 1;
    public const int SzErrorMem = 2;
    public const int SzErrorCrc = 3;
    public const int SzErrorUnsupported = 4;
    public const int SzErrorRead = 8;
    public const int SzSeekSet = 0;
    public const int SzSeekCur = 1;
    public const int SzSeekEnd = 2;
    public const int SeekSet = 0;
    public const int SeekCur = 1;
    public const int SeekEnd = 2;
    public const int StreamBufferSize = 1 << 14;
    public const uint BlockIndexNone = 0xFFFFFFFFu;
    public const int FsMessageError = 1;

    public uint CrcTable1 { get; set; }
    public Action? CrcGenerateTable { get; set; }
    public Func<byte[]?, long, long>? FileRead { get; set; }
    public Func<long, int, long>? FileSeek { get; set; }
    public Func<long>? FileTell { get; set; }
    public Action<int>? CreateLookVTable { get; set; }
    public Action? SzArExInit { get; set; }
    public Func<int>? SzArExOpen { get; set; }
    public Func<uint, ExtractOutcome>? SzArExExtract { get; set; }
    public Action? SzArExFree { get; set; }
    public Action<byte[]?>? AllocFree { get; set; }
    public Action<uint>? AllocateEntries { get; set; }
    public Action<int, string>? Printf { get; set; }

    public string? FileName { get; private set; }
    public object? Pool { get; private set; }
    public object? Reader { get; private set; }
    public Archive? Opened { get; private set; }
    public int Skipped { get; private set; } = -1;
    public uint? NumLumps { get; private set; }
    public uint? J { get; private set; }
    public bool Returned { get; private set; }
    public bool Result { get; private set; }
    public List<string> Trace { get; } = [];
    public List<(int Level, string Text)> Messages { get; } = [];

    public sealed class ExtractOutcome
    {
        public int Res { get; init; }
        public int Offset { get; init; }
        public int Size { get; init; }
    }

    public sealed class Archive
    {
        private readonly VmFile7z1366Slice _host;
        public object? Reader { get; }
        public byte[] StreamBuffer { get; } = new byte[StreamBufferSize];
        public byte[]? LookBuf { get; private set; }
        public int LookPos { get; private set; } = -1;
        public int LookSize { get; private set; } = -1;
        public bool RealStreamAttached { get; private set; }
        public uint BlockIndex { get; private set; }
        public byte[]? OutBuffer { get; set; }
        public nuint OutBufferSize { get; set; }
        public uint NumFiles { get; set; }
        public int FreeOut { get; private set; }
        public int FreeDb { get; private set; }

        public Archive(VmFile7z1366Slice host, object? reader)
        {
            _host = host;
            Reader = reader;
            if (host.CrcTable1 == 0)
            {
                if (host.CrcGenerateTable == null)
                    throw new InvalidOperationException("CrcGenerateTable");
                host.Trace.Add("CrcGenerateTable");
                host.CrcGenerateTable();
            }

            if (host.FileSeek == null)
                throw new InvalidOperationException("Seek");
            host.Trace.Add("Seek");
            host.FileSeek(0, SeekSet);
            if (host.CreateLookVTable == null)
                throw new InvalidOperationException("LookToRead2_CreateVTable");
            host.Trace.Add("LookToRead2_CreateVTable");
            host.CreateLookVTable(0);
            RealStreamAttached = true;
            host.Trace.Add("LookToRead2_INIT");
            LookPos = 0;
            LookSize = 0;
            LookBuf = StreamBuffer;
            if (host.SzArExInit == null)
                throw new InvalidOperationException("SzArEx_Init");
            host.Trace.Add("SzArEx_Init");
            host.SzArExInit();
            BlockIndex = BlockIndexNone;
            OutBuffer = null;
            OutBufferSize = 0;
        }

        public int Read(byte[]? buffer, ref long size)
        {
            if (_host.FileRead == null)
                throw new InvalidOperationException("Read");
            var numread = _host.FileRead(buffer, size);
            if (numread < 0)
            {
                size = 0;
                return SzErrorRead;
            }

            size = numread;
            return SzOk;
        }

        public int Seek(ref long pos, int origin)
        {
            int move;
            if (origin == SzSeekSet)
                move = SeekSet;
            else if (origin == SzSeekCur)
                move = SeekCur;
            else if (origin == SzSeekEnd)
                move = SeekEnd;
            else
                return 1;
            if (_host.FileSeek == null)
                throw new InvalidOperationException("Seek");
            var res = _host.FileSeek(pos, move);
            if (_host.FileTell == null)
                throw new InvalidOperationException("Tell");
            pos = _host.FileTell();
            return unchecked((int)res);
        }

        public int OpenDb()
        {
            if (_host.SzArExOpen == null)
                throw new InvalidOperationException("SzArEx_Open");
            _host.Trace.Add("SzArEx_Open");
            return _host.SzArExOpen();
        }

        public int Extract(uint fileIndex, byte[]? buffer)
        {
            if (_host.SzArExExtract == null)
                throw new InvalidOperationException("SzArEx_Extract");
            _host.Trace.Add("SzArEx_Extract");
            var outcome = _host.SzArExExtract(fileIndex);
            if (outcome.Res != SzOk)
                return outcome.Res;
            if (OutBuffer == null)
                throw new InvalidOperationException("OutBuffer");
            if (buffer == null)
                throw new InvalidOperationException("buffer");
            if (outcome.Offset < 0 || outcome.Size < 0
                || outcome.Offset > OutBuffer.Length
                || outcome.Size > buffer.Length
                || outcome.Offset + outcome.Size > OutBuffer.Length)
                throw new IndexOutOfRangeException("OutBuffer");
            Array.Copy(OutBuffer, outcome.Offset, buffer, 0, outcome.Size);
            return SzOk;
        }

        public void Destroy()
        {
            if (OutBuffer != null)
            {
                if (_host.AllocFree == null)
                    throw new InvalidOperationException("IAlloc_Free");
                _host.Trace.Add("IAlloc_Free");
                _host.AllocFree(OutBuffer);
                FreeOut++;
            }

            if (_host.SzArExFree == null)
                throw new InvalidOperationException("SzArEx_Free");
            _host.Trace.Add("SzArEx_Free");
            _host.SzArExFree();
            FreeDb++;
        }
    }

    public void Construct(string? filename, object? reader, object? pool)
    {
        FileName = filename;
        Reader = reader;
        Pool = pool;
        Opened = null;
    }

    public void Open()
    {
        var archive = new Archive(this, Reader);
        Opened = archive;
        Skipped = 0;
        var res = archive.OpenDb();
        if (res != SzOk)
        {
            archive.Destroy();
            Opened = null;
            if (Printf == null)
                throw new InvalidOperationException("Printf");
            string text;
            if (res == SzErrorUnsupported)
            {
                if (FileName == null)
                    throw new InvalidOperationException("FileName");
                text = FileName + ": Decoder does not support this archive\n";
            }
            else if (res == SzErrorMem)
                text = "Cannot allocate memory\n";
            else if (res == SzErrorCrc)
                text = "CRC error\n";
            else
                text = "error #" + res + "\n";
            Trace.Add("Printf");
            Printf(FsMessageError, text);
            Messages.Add((FsMessageError, text));
            Returned = true;
            Result = false;
            return;
        }

        if (AllocateEntries == null)
            throw new InvalidOperationException("AllocateEntries");
        Trace.Add("AllocateEntries");
        AllocateEntries(archive.NumFiles);
        NumLumps = archive.NumFiles;
        J = 0;
    }
}
