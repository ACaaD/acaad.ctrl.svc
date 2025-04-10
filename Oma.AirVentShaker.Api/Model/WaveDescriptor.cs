namespace Oma.AirVentShaker.Api.Model;

public interface IWaveDescriptor
{
}

public record SineWaveDescriptor : IWaveDescriptor
{
  public decimal Frequency { get; init; }
}