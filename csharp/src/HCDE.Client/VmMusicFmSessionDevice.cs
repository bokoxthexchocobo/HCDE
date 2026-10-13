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

public interface IVmMusicFmRenderer : IVmMusicFmMidiDevice, IVmMusicFmAudioDevice
{
    void ResetState();
}

public interface IVmMusicAdlRenderer : IVmMusicFmRenderer, IVmMusicAdlConfigurationDevice { }
public interface IVmMusicOpnRenderer : IVmMusicFmRenderer, IVmMusicOpnConfigurationDevice { }

// Borrows an initialized renderer. The caller serializes session and audio access.
public sealed class VmMusicFmSessionDevice : IVmMusicAudioSessionDevice, IVmMusicIntegerSettingsDevice,
    IVmMusicNumberSettingsDevice, IVmMusicStringSettingsDevice
{
    private readonly IVmMusicFmMidiDevice midi;
    private readonly IVmMusicFmAudioDevice audio;
    private readonly int sampleRate;
    public float OutputGain { get; set; }
    private readonly Dictionary<int, uint[]> prepared = [];
    private readonly Queue<int> submitted = new();
    private VmMusicFmEventQueue events;
    private Func<int>? completion;
    private bool started;
    private bool paused;
    private readonly Action? resetState;
    private VmMusicFmRuntimeSettings? runtimeSettings;
    private Func<string, int, bool>? changeBankInteger;
    private Func<string, string, bool>? changeBankString;
    private IDisposable? ownedRenderer;
    private bool disposed;

    internal void OwnRenderer(IDisposable renderer) => ownedRenderer = renderer;

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Close();
        ownedRenderer?.Dispose();
    }

    public static VmMusicFmSessionDevice CreateAdl(IVmMusicAdlRenderer renderer, int sampleRate,
        VmMusicAdlConfiguration configuration, VmMusicStartupData resources)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        var initialized = VmMusicFmInitialization.InitializeAdl(configuration, resources, renderer);
        var device = new VmMusicFmSessionDevice(renderer, sampleRate, initialized.OutputGain);
        var settings = new VmMusicFmRuntimeSettings((IVmMusicAdlConfigurationDevice)renderer, configuration.Settings.Gain);
        var banks = new VmMusicAdlRuntimeBanks(configuration.Bank, resources, renderer, settings);
        device.runtimeSettings = settings;
        device.changeBankInteger = banks.ChangeInteger;
        device.changeBankString = banks.ChangeString;
        return device;
    }

    public static VmMusicFmSessionDevice CreateOpn(IVmMusicOpnRenderer renderer, int sampleRate,
        VmMusicOpnConfiguration configuration, VmMusicStartupData resources)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        var initialized = VmMusicFmInitialization.InitializeOpn(configuration, resources, renderer);
        var device = new VmMusicFmSessionDevice(renderer, sampleRate, initialized.OutputGain);
        var settings = new VmMusicFmRuntimeSettings((IVmMusicOpnConfigurationDevice)renderer, configuration.Settings.Gain);
        var banks = new VmMusicOpnRuntimeBanks(configuration.Bank, resources, renderer);
        device.runtimeSettings = settings;
        device.changeBankInteger = banks.ChangeInteger;
        device.changeBankString = banks.ChangeString;
        return device;
    }

    public bool ChangeInteger(string setting, int value)
    {
        RequireOpen();
        ArgumentNullException.ThrowIfNull(setting);
        var handled = runtimeSettings?.ChangeInteger(setting, value) == true || changeBankInteger?.Invoke(setting, value) == true;
        if (handled) OutputGain = runtimeSettings!.OutputGain;
        return handled;
    }

    public bool ChangeNumber(string setting, double value)
    {
        RequireOpen();
        ArgumentNullException.ThrowIfNull(setting);
        var handled = runtimeSettings?.ChangeNumber(setting, value) == true;
        if (handled) OutputGain = runtimeSettings!.OutputGain;
        return handled;
    }

    public bool ChangeString(string setting, string value)
    {
        RequireOpen();
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(value);
        var handled = changeBankString?.Invoke(setting, value) == true;
        if (handled) OutputGain = runtimeSettings!.OutputGain;
        return handled;
    }

    public VmMusicFmSessionDevice(IVmMusicFmRenderer renderer, int sampleRate, float outputGain)
        : this(renderer, renderer, sampleRate, outputGain)
    {
        resetState = renderer.ResetState;
    }

    public VmMusicFmSessionDevice(IVmMusicFmMidiDevice midi, IVmMusicFmAudioDevice audio, int sampleRate, float outputGain)
    {
        ArgumentNullException.ThrowIfNull(midi);
        ArgumentNullException.ThrowIfNull(audio);
        this.midi = midi;
        this.audio = audio;
        this.sampleRate = sampleRate;
        OutputGain = outputGain;
        events = CreateQueue();
    }

    public bool IsOpen { get; private set; }
    public VmMusicStreamFormat GetStreamInfo() => VmMusicSoftSynthStreamInfo.Get(sampleRate);
    public int Technology => 7;
    public bool CanHandleSysex => true;
    public bool FakeVolume => true;

    private VmMusicFmEventQueue CreateQueue()
    {
        var queue = new VmMusicFmEventQueue(midi, sampleRate);
        queue.BufferCompleted = () =>
        {
            submitted.Dequeue();
            var error = completion?.Invoke() ?? 0;
            if (error != 0)
            {
                started = false;
                throw new InvalidOperationException($"FM MIDI completion failed: {error}.");
            }
        };
        return queue;
    }

    public int Open()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (IsOpen) throw new InvalidOperationException("FM MIDI device is already open.");
        resetState?.Invoke();
        events = CreateQueue();
        prepared.Clear();
        submitted.Clear();
        started = paused = false;
        IsOpen = true;
        return 0;
    }

    private void RequireOpen()
    {
        if (!IsOpen) throw new InvalidOperationException("FM MIDI device is closed.");
    }

    public void SetCompletionCallback(Func<int>? callback) => completion = callback;
    public void PrecacheInstruments(IReadOnlyList<ushort> instruments) => ArgumentNullException.ThrowIfNull(instruments);
    public void InitializePlayback() => RequireOpen();
    public int SetTimeDivision(ushort division) { RequireOpen(); events.SetTimeDivision(division); return 0; }
    public int SetTempo(uint tempo) { RequireOpen(); events.SetTempo(tempo); return 0; }
    public int Prepare(int bufferNumber, ReadOnlyMemory<uint> data)
    {
        RequireOpen();
        if (prepared.ContainsKey(bufferNumber)) throw new InvalidOperationException("MIDI buffer is already prepared.");
        prepared.Add(bufferNumber, data.ToArray());
        return 0;
    }
    public int Unprepare(int bufferNumber)
    {
        RequireOpen();
        if (submitted.Contains(bufferNumber)) throw new InvalidOperationException("MIDI buffer is still queued.");
        prepared.Remove(bufferNumber);
        return 0;
    }
    public int StreamOut(int bufferNumber)
    {
        RequireOpen();
        if (submitted.Contains(bufferNumber)) throw new InvalidOperationException("MIDI buffer is already queued.");
        if (!prepared.TryGetValue(bufferNumber, out var data)) throw new InvalidOperationException("MIDI buffer is not prepared.");
        events.Submit(data);
        submitted.Enqueue(bufferNumber);
        return 0;
    }
    public int StreamOutSync(int bufferNumber) => StreamOut(bufferNumber);
    public int StartStream() { RequireOpen(); started = true; return 0; }
    public bool Pause(bool value) { RequireOpen(); paused = value; return true; }
    public bool Update() => IsOpen && started && events.HasEvents;
    public bool ServiceStream(Span<float> stereoSamples)
    {
        RequireOpen();
        if ((stereoSamples.Length & 1) != 0)
            throw new ArgumentException("FM stream requires complete stereo frames.", nameof(stereoSamples));
        if (!started || paused) { stereoSamples.Clear(); return started; }
        var completed = false;
        try
        {
            var active = events.ServiceStream(audio, stereoSamples, OutputGain);
            if (!active) started = false;
            completed = true;
            return active;
        }
        finally
        {
            // A partially dispatched request cannot safely resume after a backend failure.
            if (!completed) started = false;
        }
    }

    public bool ServiceStreamBytes(Span<byte> buffer)
    {
        if (buffer.Length % (2 * sizeof(float)) != 0)
            throw new ArgumentException("FM stream byte buffer requires complete float stereo frames.", nameof(buffer));
        return ServiceStream(System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(buffer));
    }
    public void Stop()
    {
        started = paused = false;
        submitted.Clear();
        events = CreateQueue();
    }
    public void Close()
    {
        Stop();
        prepared.Clear();
        IsOpen = false;
    }
}
