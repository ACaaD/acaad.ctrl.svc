using JetBrains.Annotations;
using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Extensions;

public static class EitherExtensions
{
  [HandlesResourceDisposal]
  public static void Dispose<TLeft, TRight>(this Either<TLeft, TRight> either)
    where TRight : IDisposable
  {
    either.Match(
      l =>
      {
        if (l is IDisposable leftDisposable)
        {
          leftDisposable.Dispose();
        }
      },
      r =>
      {
        if (r is IDisposable rightDisposable)
        {
          rightDisposable.Dispose();
        }
      }
    );
  }
}