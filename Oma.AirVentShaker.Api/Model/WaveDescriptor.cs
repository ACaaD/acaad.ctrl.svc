namespace Oma.AirVentShaker.Api.Model;

public interface IWaveDescriptor
{
  float Amplitude { get; init; }
}

public record SineWaveDescriptor : IWaveDescriptor
{
  public float Frequency { get; init; }

  public float Amplitude { get; init; }
}