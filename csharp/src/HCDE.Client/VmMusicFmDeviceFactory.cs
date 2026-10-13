/*
** music_timidity_mididevice.cpp
** Provides access to TiMidity as a generic MIDI device.
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

public static class VmMusicFmDeviceFactory
{
    public const int NativeSampleRate = 44100;

    public static Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice> CreateRouter<TAdl, TOpn>(
        Func<int, TAdl?>? allocateAdl, Func<int, TOpn?>? allocateOpn,
        VmMusicAdlConfiguration adlConfiguration, VmMusicOpnConfiguration opnConfiguration,
        VmMusicStartupData resources, Func<VmMusicMidiDeviceKind, IVmMusicMidiSessionDevice>? otherDevices = null)
        where TAdl : class, IVmMusicAdlRenderer, IDisposable
        where TOpn : class, IVmMusicOpnRenderer, IDisposable
    {
        ArgumentNullException.ThrowIfNull(adlConfiguration);
        ArgumentNullException.ThrowIfNull(opnConfiguration);
        ArgumentNullException.ThrowIfNull(resources);
        return kind => kind switch
        {
            VmMusicMidiDeviceKind.Adl => allocateAdl is null
                ? throw new VmMusicMidiDeviceUnavailableException("ADL renderer backend is unavailable.")
                : CreateAdl(allocateAdl, adlConfiguration, resources),
            VmMusicMidiDeviceKind.Opn => allocateOpn is null
                ? throw new VmMusicMidiDeviceUnavailableException("OPN renderer backend is unavailable.")
                : CreateOpn(allocateOpn, opnConfiguration, resources),
            _ when !Enum.IsDefined(kind) || kind == VmMusicMidiDeviceKind.Default =>
                throw new ArgumentOutOfRangeException(nameof(kind)),
            _ => otherDevices is null
                ? throw new VmMusicMidiDeviceUnavailableException($"MIDI backend {kind} is unavailable.")
                : otherDevices(kind) ?? throw new InvalidOperationException("MIDI factory returned no device.")
        };
    }

    public static VmMusicFmSessionDevice CreateAdl<T>(Func<int, T?> allocate,
        VmMusicAdlConfiguration configuration, VmMusicStartupData resources)
        where T : class, IVmMusicAdlRenderer, IDisposable
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(resources);
        return Create(allocate, renderer => VmMusicFmSessionDevice.CreateAdl(renderer, NativeSampleRate, configuration, resources), "ADL");
    }

    public static VmMusicFmSessionDevice CreateOpn<T>(Func<int, T?> allocate,
        VmMusicOpnConfiguration configuration, VmMusicStartupData resources)
        where T : class, IVmMusicOpnRenderer, IDisposable
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(resources);
        return Create(allocate, renderer => VmMusicFmSessionDevice.CreateOpn(renderer, NativeSampleRate, configuration, resources), "OPN");
    }

    private static VmMusicFmSessionDevice Create<T>(Func<int, T?> allocate,
        Func<T, VmMusicFmSessionDevice> initialize, string name) where T : class, IDisposable
    {
        ArgumentNullException.ThrowIfNull(allocate);
        var renderer = allocate(NativeSampleRate)
            ?? throw new VmMusicMidiDeviceUnavailableException($"Unable to create {name} renderer.");
        try
        {
            var device = initialize(renderer);
            device.OwnRenderer(renderer);
            return device;
        }
        catch (Exception initializationError)
        {
            try { renderer.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("FM renderer initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }
}
