using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Core.Errors;

[PublicAPI]
public record SimulatedFlowError : FlowError
{
  public SimulatedFlowError(string errorMessage, bool isExceptional) : base(isExceptional)
  {
    ErrorMessage = errorMessage;
  }

  public string ErrorMessage { get; }

  public override string Message => ErrorMessage;
}