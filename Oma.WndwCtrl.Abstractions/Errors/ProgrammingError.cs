namespace Oma.WndwCtrl.Abstractions.Errors;

public record ProgrammingError : TechnicalError
{
  private const int ErrorCodeOffset = 50_000;

  public ProgrammingError(string Message, int Code) : base(
    $"An error caused by insufficient programming occured: '{Message}'",
    ErrorCodeOffset + Code)
  {
  }
}