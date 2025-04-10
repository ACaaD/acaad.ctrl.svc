namespace Oma.AirVentShaker.Api.Model;

public class TestDefinition
{
  public string Name { get; init; } = $"Unnamed_Test_{DateTime.UtcNow:o}";

  public List<TestStep> Steps { get; init; } = new();
}