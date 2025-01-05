namespace Oma.WndwCtrl.Abstractions.Errors;

public record OperationCancelledError : TechnicalError
{
  public OperationCancelledError(Exception ex) : base("The operation was canelled.", Code: 100, ex)
  {
  }
}