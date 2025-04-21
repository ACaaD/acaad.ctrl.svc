using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;

namespace Oma.AirVentShaker.Api.Audio;

public sealed class AudioService : IAudioService, IDisposable
{
  private readonly AudioEngine _audioEngine;
  private readonly SemaphoreSlim _mutex = new(initialCount: 1);
  private readonly Oscillator _oscillator;

  private CancellationTokenSource? _delayCancellationTokenSource;

  public AudioService()
  {
    _audioEngine = new MiniAudioEngine(sampleRate: 44100, Capability.Playback);

    _oscillator = new Oscillator()
      { Frequency = 50, Type = Oscillator.WaveformType.Sine, Amplitude = 0.2f, Enabled = false, };

    Mixer.Master.AddComponent(_oscillator);
  }

  public async Task PlayAsync(
    IWaveDescriptor waveDescriptor,
    TimeSpan duration,
    CancellationToken cancelToken
  )
  {
    try
    {
      await _mutex.WaitAsync(cancelToken);

      if (_delayCancellationTokenSource is not null)
      {
        await _delayCancellationTokenSource.CancelAsync();
      }

      _delayCancellationTokenSource = new CancellationTokenSource();

      AdjustOscillator(waveDescriptor);
      ScheduleStop(duration, _delayCancellationTokenSource.Token);

      cancelToken.Register(() => _oscillator.Enabled = false);
    }
    finally
    {
      _mutex.Release();
    }
  }

  public async Task StopAsync(CancellationToken cancelToken)
  {
    try
    {
      await _mutex.WaitAsync(cancelToken);

      _oscillator.Enabled = false;

      await (_delayCancellationTokenSource?.CancelAsync() ?? Task.CompletedTask);
      _delayCancellationTokenSource?.Dispose();
      _delayCancellationTokenSource = null;
    }
    finally
    {
      _mutex.Release();
    }
  }

  public async Task UpdateAmplitudeAsync(float newVal, CancellationToken cancelToken)
  {
    try
    {
      await _mutex.WaitAsync(cancelToken);

      _oscillator.Amplitude = newVal;
    }
    finally
    {
      _mutex.Release();
    }
  }

  public void Dispose()
  {
    _delayCancellationTokenSource?.Dispose();
    _audioEngine.Dispose();
  }

  private void ScheduleStop(TimeSpan duration, CancellationToken cancelToken)
  {
    Task.Run(
      async () =>
      {
        try
        {
          await Task.Delay(duration, cancelToken);
          _oscillator.Enabled = false;

          _delayCancellationTokenSource?.Dispose();
          _delayCancellationTokenSource = null;
        }
        catch (OperationCanceledException)
        {
          // do nothing, empty on purpose (PlayAsync was called again)
        }
      }
    );
  }

  private void AdjustOscillator(IWaveDescriptor waveDescriptor)
  {
    Mixer.Master.RemoveComponent(_oscillator);

    if (waveDescriptor is SineWaveDescriptor sineDescriptor)
    {
      _oscillator.Type = Oscillator.WaveformType.Sine;
      _oscillator.Frequency = sineDescriptor.Frequency;
    }
    else
    {
      throw new InvalidOperationException(
        $"Unknown wave descriptor {waveDescriptor.GetType().Name}. This is a programming error."
      );
    }

    _oscillator.Amplitude = waveDescriptor.Amplitude;

    _oscillator.Enabled = true;
    Mixer.Master.AddComponent(_oscillator);
  }
}