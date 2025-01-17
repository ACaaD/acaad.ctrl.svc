using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.CoreAsp.Extensions;

public static class ControllerBaseExtensions
{
  [HandlesResourceDisposal]
  public static void RegisterForDispose<EL, ER>(this Either<EL, ER> either, HttpContext httpContext)
    where ER : IDisposable
  {
    either.IfRight(r => httpContext.Response.RegisterForDispose(r));
  }
}