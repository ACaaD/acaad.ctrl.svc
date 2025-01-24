namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

public interface IHasSchedulingDelay
{
  public TimeSpan? DelayedBy { get; }
}