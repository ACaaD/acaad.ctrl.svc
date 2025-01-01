namespace Oma.WndwCtrl.Abstractions;

public interface IOutcome
{
  bool Success { get; }
  string OutcomeRaw { get; }
}