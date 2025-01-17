using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions.Extensions;

namespace Oma.WndwCtrl.Core.Extensions;

public static class TaskExtensions
{
  [HandlesResourceDisposal]
  public static Task<U> BindDispose<EL, ER, U>(
    this Task<Either<EL, ER>> self,
    Func<Either<EL, ER>, Task<U>> bind
  )
    where ER : IDisposable =>
    self.SelectManyDispose(bind);

  private async static Task<U> SelectManyDispose<EL, ER, U>(
    this Task<Either<EL, ER>> self,
    Func<Either<EL, ER>, Task<U>> bind
  )
    where ER : IDisposable
  {
    Either<EL, ER> selfRes = await self.ConfigureAwait(continueOnCapturedContext: false);

    try
    {
      U u = await bind(selfRes).ConfigureAwait(continueOnCapturedContext: false);
      return u;
    }
    finally
    {
      selfRes.Dispose();
    }
  }
}