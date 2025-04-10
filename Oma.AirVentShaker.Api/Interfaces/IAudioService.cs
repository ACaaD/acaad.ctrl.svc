using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Interfaces;

public interface IAudioService
{
  Task PlayAsync(IWaveDescriptor waveDescriptor, TimeSpan duration, CancellationToken cancelToken);

  Task StopAsync(CancellationToken cancelToken);

  Task UpdateAmplitudeAsync(float newVal, CancellationToken cancelToken);
}