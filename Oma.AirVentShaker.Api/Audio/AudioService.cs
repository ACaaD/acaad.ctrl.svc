using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Audio;

public class AudioService : IAudioService
{
  public Task StopAllAsync(CancellationToken cancelToken) => throw new NotImplementedException();

  public async Task PlayAsync(
    IWaveDescriptor waveDescriptor,
    TimeSpan duration,
    CancellationToken cancelToken
  )
  {
  }
}