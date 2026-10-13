/*
** music_midistream.cpp
** Implements base class for MIDI and MUS streaming.
**
**---------------------------------------------------------------------------
** Copyright 2008 Marisa Heit
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

public static class VmMusicFmWaveDump
{
    // Borrows the source; owns the factory-created device and output file.
    public static bool Write(IVmMusicSmfSource source, VmMusicWaveRequest request,
        Func<VmMusicWaveRequest, VmMusicFmSessionDevice> createDevice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createDevice);
        return WriteAudio(source, request, createDevice, cancellationToken);
    }

    internal static bool WriteAudio(IVmMusicSmfSource source, VmMusicWaveRequest request,
        Func<VmMusicWaveRequest, IVmMusicAudioSessionDevice> createDevice,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(createDevice);
        ArgumentException.ThrowIfNullOrEmpty(request.Output);
        if (!source.SetSubsong(request.Subsong))
            throw new ArgumentOutOfRangeException(nameof(request), "MIDI subsong does not exist.");
        cancellationToken.ThrowIfCancellationRequested();
        using var device = createDevice(request) ?? throw new InvalidOperationException("WAVE factory returned no audio device.");
        using var session = new VmMusicMidiSession(source, device);
        using var output = new FileStream(request.Output, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new VmMusicWaveWriter(output, device.GetStreamInfo().SampleRate);
        var finished = false;
        try
        {
            var started = session.Start(600000, subsong: request.Subsong);
            if (started) writer.RenderUntilEnd(device.ServiceStream, cancellationToken);
            else writer.Complete();
            finished = true;
            return started;
        }
        finally
        {
            if (!finished) writer.Abort();
        }
    }
}
