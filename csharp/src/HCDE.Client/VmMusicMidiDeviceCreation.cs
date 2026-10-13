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

public enum VmMusicMidiDeviceKind
{
    Default = -1, Standard = 0, Opl = 1, SoundSystem = 2, Timidity = 3,
    FluidSynth = 4, Gus = 5, WildMidi = 6, Adl = 7, Opn = 8
}

public sealed class VmMusicMidiDeviceUnavailableException(string message) : Exception(message);
public sealed record VmMusicMidiCreatedDevice(IVmMusicMidiSessionDevice Device,
    VmMusicMidiDeviceKind Requested, VmMusicMidiDeviceKind Selected);

public static class VmMusicMidiDeviceCreation
{
    public static VmMusicMidiDeviceKind Select(VmMusicMidiDeviceKind songDevice, int setting, bool systemMidi)
    {
        if (!Enum.IsDefined(songDevice)) throw new ArgumentOutOfRangeException(nameof(songDevice));
        if (songDevice != VmMusicMidiDeviceKind.Default) return songDevice;
        return setting switch
        {
            -1 => VmMusicMidiDeviceKind.SoundSystem, -2 => VmMusicMidiDeviceKind.Timidity,
            -3 => VmMusicMidiDeviceKind.Opl, -4 => VmMusicMidiDeviceKind.Gus,
            -5 => VmMusicMidiDeviceKind.FluidSynth, -6 => VmMusicMidiDeviceKind.WildMidi,
            -7 => VmMusicMidiDeviceKind.Adl, -8 => VmMusicMidiDeviceKind.Opn,
            _ => systemMidi ? VmMusicMidiDeviceKind.Standard : VmMusicMidiDeviceKind.SoundSystem
        };
    }

    public static VmMusicMidiCreatedDevice Create(VmMusicMidiDeviceKind requested, bool systemMidi,
        Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (!Enum.IsDefined(requested) || requested == VmMusicMidiDeviceKind.Default)
            throw new ArgumentOutOfRangeException(nameof(requested));
        if (requested == VmMusicMidiDeviceKind.SoundSystem) requested = VmMusicMidiDeviceKind.FluidSynth;
        VmMusicMidiDeviceKind[] fallback = [VmMusicMidiDeviceKind.FluidSynth, VmMusicMidiDeviceKind.Timidity,
            VmMusicMidiDeviceKind.WildMidi, VmMusicMidiDeviceKind.Gus, VmMusicMidiDeviceKind.Standard,
            VmMusicMidiDeviceKind.Adl, VmMusicMidiDeviceKind.Opn, VmMusicMidiDeviceKind.Opl];
        List<Exception> failures = [];
        foreach (var selected in new[] { requested }.Concat(fallback).Distinct())
        {
            if (!systemMidi && selected == VmMusicMidiDeviceKind.Standard)
            {
                // Native standard-device creation falls through to FluidSynth on these platforms.
                continue;
            }
            try
            {
                var device = factory(selected) ?? throw new InvalidOperationException("MIDI factory returned no device.");
                return new(device, requested, selected);
            }
            catch (VmMusicMidiDeviceUnavailableException failure)
            {
                failures.Add(failure);
            }
        }
        throw new AggregateException("Failed to play music: unable to create any MIDI device.", failures);
    }
}
