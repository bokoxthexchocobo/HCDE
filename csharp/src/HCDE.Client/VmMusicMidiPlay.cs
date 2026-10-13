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

public sealed record VmMusicMidiPlayOptions(uint MaximumTime, int Subsong = 0, bool RepeatSong = false,
    bool LiveLoops = false, float MusicVolume = 1, float RelativeVolume = 1, float MasterVolume = 1);

public sealed record VmMusicMidiPlayingSession(VmMusicMidiSession Session, bool Started,
    VmMusicMidiDeviceKind Requested, VmMusicMidiDeviceKind Selected);

public static class VmMusicMidiPlay
{
    // The returned session owns the device lifetime, including disposal; the source remains borrowed.
    public static VmMusicMidiPlayingSession Start(IVmMusicSmfSource source, VmMusicMidiDeviceKind songDevice,
        int deviceSetting, bool systemMidi, Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice> factory,
        VmMusicMidiPlayOptions options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);
        var requested = VmMusicMidiDeviceCreation.Select(songDevice, deviceSetting, systemMidi);
        if (!source.SetSubsong(options.Subsong))
            throw new ArgumentOutOfRangeException(nameof(options), "MIDI subsong does not exist.");
        var created = VmMusicMidiDeviceCreation.Create(requested, systemMidi, factory);
        VmMusicMidiSession? session = null;
        try
        {
            session = new(source, created.Device);
            var started = session.Start(options.MaximumTime, options.MusicVolume, options.RelativeVolume,
                options.MasterVolume, options.Subsong, options.RepeatSong, options.LiveLoops);
            return new(session, started, created.Requested, created.Selected);
        }
        catch (Exception playbackError)
        {
            try
            {
                if (session is null)
                {
                    if (created.Device is IDisposable disposableDevice) disposableDevice.Dispose();
                    else created.Device.Close();
                }
                else session.Dispose();
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("MIDI play and cleanup failed.", playbackError, cleanupError);
            }
            throw;
        }
    }
}
