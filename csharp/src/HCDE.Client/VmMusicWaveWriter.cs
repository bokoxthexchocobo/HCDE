/*
** music_wavewriter_mididevice.cpp
** Dumps a MIDI to a wave file by using one of the other software synths.
**
**---------------------------------------------------------------------------
** Copyright 2008 Marisa Heit
** Copyright 2018 Christoph Oelckers
** All rights reserved.
**
** Redistribution and use in source and binary forms, with or without
** modification, are permitted provided that the following conditions
** are met:
**
** 1. Redistributions of source code must retain the above copyright
**    notice, this list of conditions and the following disclaimer.
** 2. Redistributions in binary form must reproduce the above copyright
**    notice, this list of conditions and the following disclaimer in the
**    documentation and/or other materials provided with the distribution.
** 3. The name of the author may not be used to endorse or promote products
**    derived from this software without specific prior written permission.
**
** THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
** IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
** OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
** IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
** NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
** THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**---------------------------------------------------------------------------
**
*/

namespace HCDE.Client;

public delegate bool VmMusicWaveRenderCallback(Span<float> stereoSamples);

public sealed class VmMusicWaveWriter : IDisposable
{
    private readonly Stream stream;
    private readonly int sampleRate;
    private readonly bool leaveOpen;
    private readonly byte[] sampleBytes = new byte[16384];
    private bool completed;
    private bool disposed;
    private bool failed;
    public uint DataBytes { get; private set; }
    internal void Abort() => failed = true;

    public VmMusicWaveWriter(Stream stream, int sampleRate, bool leaveOpen = true)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite || !stream.CanSeek) throw new ArgumentException("WAVE output must be writable and seekable.", nameof(stream));
        if (stream.Position != 0 || stream.Length != 0) throw new ArgumentException("WAVE output must be an empty stream.", nameof(stream));
        Span<byte> header = stackalloc byte[VmMusicWaveFile.HeaderBytes];
        VmMusicWaveFile.WriteHeader(header, sampleRate, 0);
        header.Slice(4, 4).Clear(); // Native header leaves sizes provisional until CloseFile.
        this.stream = stream;
        this.sampleRate = sampleRate;
        this.leaveOpen = leaveOpen;
        var written = false;
        try { stream.Write(header); written = true; }
        finally { if (!written && !leaveOpen) stream.Dispose(); }
    }

    private void RequireWritable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("WAVE output is already finalized.");
        if (failed) throw new InvalidOperationException("WAVE output failed and cannot be continued.");
    }

    public void RenderUntilEnd(VmMusicWaveRenderCallback render, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(render);
        RequireWritable();
        var finished = false;
        try
        {
            var samples = new float[4096];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Native Resume writes only requests for which ServiceStream returns true.
                if (!render(samples)) break;
                WriteSamples(samples);
            }
            Complete();
            finished = true;
        }
        finally { if (!finished) failed = true; }
    }

    public void WriteSamples(ReadOnlySpan<float> samples)
    {
        RequireWritable();
        if ((samples.Length & 1) != 0) throw new ArgumentException("WAVE output requires complete stereo frames.", nameof(samples));
        var nextSize = checked(DataBytes + (uint)checked(samples.Length * sizeof(float)));
        _ = checked(nextSize + VmMusicWaveFile.HeaderBytes - 8u);
        var written = false;
        try
        {
            while (!samples.IsEmpty)
            {
                var count = Math.Min(samples.Length, sampleBytes.Length / sizeof(float));
                var bytes = VmMusicWaveFile.WriteSamples(samples[..count], sampleBytes);
                stream.Write(sampleBytes.AsSpan(0, bytes));
                samples = samples[count..];
            }
            DataBytes = nextSize;
            written = true;
        }
        finally { if (!written) failed = true; }
    }

    public void Complete()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) return;
        RequireWritable();
        var finished = false;
        try
        {
            Span<byte> header = stackalloc byte[VmMusicWaveFile.HeaderBytes];
            VmMusicWaveFile.WriteHeader(header, sampleRate, DataBytes);
            stream.Position = 0;
            stream.Write(header);
            stream.Position = VmMusicWaveFile.HeaderBytes + (long)DataBytes;
            stream.Flush();
            completed = true;
            finished = true;
        }
        finally { if (!finished) failed = true; }
    }

    public void Dispose()
    {
        if (disposed) return;
        try { if (!failed) Complete(); }
        finally
        {
            disposed = true;
            if (!leaveOpen) stream.Dispose();
        }
    }
}
