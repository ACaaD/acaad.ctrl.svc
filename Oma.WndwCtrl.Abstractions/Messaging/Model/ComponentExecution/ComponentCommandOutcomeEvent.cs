using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
[Serializable]
[MustDisposeResource]
public record ComponentCommandOutcomeEvent(IComponent Component, ICommand Command, IOutcome Outcome)
  : ComponentEvent(Component)
{
  public override string Type => "Outcome";
  public override string Name => nameof(ComponentCommandOutcomeEvent);
}