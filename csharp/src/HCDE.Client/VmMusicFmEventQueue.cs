/*
** music_softsynth_mididevice.cpp
** Common base class for software synthesis MIDI devices.
**
**---------------------------------------------------------------------------
** Copyright 2008-2010 Marisa Heit
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

// Access is serialized by the caller. Completion callbacks may submit another buffer.
public sealed class VmMusicFmEventQueue
{
    private readonly IVmMusicFmMidiDevice device;
    private readonly Queue<uint[]> buffers = new();
    private int position;
    private bool processing;
    private bool rendering;

    public VmMusicFmEventQueue(IVmMusicFmMidiDevice device, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        this.device = device;
        SampleRate = sampleRate;
    }

    public int SampleRate { get; }
    public uint Tempo { get; private set; } = 500000;
    public ushort Division { get; private set; } = 100;
    public double SamplesPerTick => SampleRate / (1000000.0 / Tempo) / Division;
    public double InitialDelaySamples { get; private set; }
    public double NextTickInSamples { get; private set; }
    public bool HasEvents => buffers.Count != 0;
    public Action? BufferCompleted { get; set; }

    public void SetTempo(uint tempo)
    {
        ArgumentOutOfRangeException.ThrowIfZero(tempo);
        Tempo = tempo;
    }

    public void SetTimeDivision(ushort division)
    {
        ArgumentOutOfRangeException.ThrowIfZero(division);
        Division = division;
    }

    public void Submit(ReadOnlySpan<uint> buffer)
    {
        if (buffer.IsEmpty) throw new ArgumentException("MIDI buffer cannot be empty.", nameof(buffer));
        var owned = buffer.ToArray();
        for (var offset = 0; offset < owned.Length;)
        {
            var words = VmMusicFmStreamEvents.GetRecordWords(owned.AsSpan(offset));
            if ((owned[offset + 2] >> 24) == 1 && (owned[offset + 2] & 0x00ffffff) == 0)
                throw new ArgumentException("MIDI tempo cannot be zero.", nameof(buffer));
            offset += words;
        }
        if (!HasEvents)
        {
            position = 0;
            InitialDelaySamples = SamplesPerTick * owned[0];
            NextTickInSamples = InitialDelaySamples;
        }
        buffers.Enqueue(owned);
    }

    public bool ServiceStream(IVmMusicFmAudioDevice audio, Span<float> stereoSamples, float outputGain)
    {
        ArgumentNullException.ThrowIfNull(audio);
        if ((stereoSamples.Length & 1) != 0)
            throw new ArgumentException("FM stream requires complete stereo frames.", nameof(stereoSamples));
        if (rendering || processing) throw new InvalidOperationException("FM stream processing cannot be reentered.");
        rendering = true;
        try
        {
            stereoSamples.Clear();
            var remainingFrames = stereoSamples.Length / 2;
            var offset = 0;
            while (HasEvents && remainingFrames > 0)
            {
                // Keep fractional samples between calls, but dispatch once less than one frame remains.
                var frames = (int)Math.Min(remainingFrames, Math.Floor(NextTickInSamples));
                if (frames > 0)
                {
                    VmMusicFmOutput.ComputeOutput(audio, stereoSamples.Slice(offset, frames * 2), frames, outputGain);
                    NextTickInSamples -= frames;
                    remainingFrames -= frames;
                    offset += frames * 2;
                }
                if (NextTickInSamples < 1)
                {
                    var delay = PlayTick();
                    if (delay == 0)
                    {
                        if (remainingFrames > 0)
                            VmMusicFmOutput.ComputeOutput(audio, stereoSamples[offset..], remainingFrames, outputGain);
                        return false;
                    }
                    NextTickInSamples += SamplesPerTick * delay;
                }
            }
            return HasEvents;
        }
        finally
        {
            rendering = false;
        }
    }

    public uint PlayTick()
    {
        if (processing) throw new InvalidOperationException("MIDI tick processing cannot be reentered.");
        processing = true;
        try
        {
            while (HasEvents)
            {
                var buffer = buffers.Peek();
                var item = VmMusicFmStreamEvents.DispatchNext(device, buffer.AsSpan(position));
                if (item.Tempo is { } tempo) SetTempo(tempo);
                position += item.WordsConsumed;
                if (position == buffer.Length)
                {
                    buffers.Dequeue();
                    position = 0;
                    BufferCompleted?.Invoke();
                }
                if (!HasEvents) return Division;
                var delay = buffers.Peek()[position];
                if (delay != 0) return delay;
            }
            return 0;
        }
        finally
        {
            processing = false;
        }
    }
}
