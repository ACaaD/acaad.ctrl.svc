namespace Oma.AirVentShaker.Api.Model;

public class GlobalState
{
  public TestStep? ActiveStep { get; set; }

  public TestDefinition? ActiveDefinition { get; set; }
}