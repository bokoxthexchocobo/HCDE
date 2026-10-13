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

using System.Buffers.Binary;

namespace HCDE.Client;

public static class VmMusicWaveFile
{
    public const int HeaderBytes = 68;

    public static void WriteHeader(Span<byte> buffer, int sampleRate, uint dataBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        if (dataBytes % 8 != 0) throw new ArgumentException("WAVE data must contain complete float stereo frames.", nameof(dataBytes));
        var riffSize = checked(dataBytes + HeaderBytes - 8u);
        var byteRate = checked((uint)sampleRate * 8);
        if (buffer.Length < HeaderBytes) throw new ArgumentException("WAVE header buffer is too small.", nameof(buffer));
        var header = buffer[..HeaderBytes];
        header.Clear();
        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], riffSize);
        "WAVEfmt "u8.CopyTo(header[8..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[16..], 40);
        BinaryPrimitives.WriteUInt16LittleEndian(header[20..], 0xfffe);
        BinaryPrimitives.WriteUInt16LittleEndian(header[22..], 2);
        BinaryPrimitives.WriteUInt32LittleEndian(header[24..], (uint)sampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(header[28..], byteRate);
        BinaryPrimitives.WriteUInt16LittleEndian(header[32..], 8);
        BinaryPrimitives.WriteUInt16LittleEndian(header[34..], 32);
        BinaryPrimitives.WriteUInt16LittleEndian(header[36..], 22);
        BinaryPrimitives.WriteUInt16LittleEndian(header[38..], 32);
        BinaryPrimitives.WriteUInt32LittleEndian(header[40..], 3);
        // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT, serialized in WAVE GUID byte order.
        new byte[] { 3, 0, 0, 0, 0, 0, 0x10, 0, 0x80, 0, 0, 0xaa, 0, 0x38, 0x9b, 0x71 }.CopyTo(header[44..]);
        "data"u8.CopyTo(header[60..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[64..], dataBytes);
    }

    public static int WriteSamples(ReadOnlySpan<float> stereoSamples, Span<byte> buffer)
    {
        if ((stereoSamples.Length & 1) != 0)
            throw new ArgumentException("WAVE samples must contain complete stereo frames.", nameof(stereoSamples));
        var bytes = checked(stereoSamples.Length * sizeof(float));
        if (buffer.Length < bytes) throw new ArgumentException("WAVE sample buffer is too small.", nameof(buffer));
        for (var i = 0; i < stereoSamples.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(buffer[(i * sizeof(float))..], BitConverter.SingleToInt32Bits(stereoSamples[i]));
        return bytes;
    }
}
