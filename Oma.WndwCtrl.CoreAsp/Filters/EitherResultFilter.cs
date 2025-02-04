using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.CoreAsp.Filters;

[UsedImplicitly]
public class EitherResultFilter : IAsyncResultFilter
{
  public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
  {
    if (context.Result is not ObjectResult { Value: IEither either, })
    {
      await next();
      return;
    }

    context.Result = either.MatchUntyped<IActionResult>(
      err => GetErrorResult(context, err),
      obj => new OkObjectResult(obj)
    );

    await next();
  }

  private static ObjectResult GetErrorResult(ResultExecutingContext context, object? error)
  {
    return error switch
    {
      Error langExtError => Problem(context, langExtError.ToProblemDetails()),
      null => GetGenericServerError(context),
      var _ => GetGenericServerError(context),
    };
  }

  private static ObjectResult GetGenericServerError(ResultExecutingContext context)
    => Problem(context, title: "An unexpected error occurred.", statusCode: 500);

  private static ObjectResult Problem(
    ResultExecutingContext context,
    string? detail = null,
    string? instance = null,
    int? statusCode = null,
    string? title = null,
    string? type = null,
    IDictionary<string, object?>? extensions = null
  )
  {
    if (context.Controller is not ControllerBase controllerBase)
    {
      throw new InvalidOperationException("Writing problem details without a controller is not supported.");
    }

    return controllerBase.Problem(detail, instance, statusCode, title, type, extensions);
  }

  private static ObjectResult Problem(ResultExecutingContext context, ProblemDetails problemDetails)
    => Problem(
      context,
      problemDetails.Detail,
      problemDetails.Instance,
      problemDetails.Status,
      problemDetails.Title,
      problemDetails.Type,
      problemDetails.Extensions
    );
}

public static class FlowErrorExtensions
{
  public static ProblemDetails ToProblemDetails(this Error error)
  {
    Option<Seq<ProblemDetails>> inner = error.Inner.Map(
      innerError =>
      {
        if (innerError is ManyErrors many)
        {
          return many.Errors.Select(e => e.ToProblemDetails());
        }

        return [innerError.ToProblemDetails(),];
      }
    );

    ProblemDetails details = new()
    {
      Title = error.Message,
      Status = error.IsExceptional
        ? 500
        : 400,
    };

    if (error is FlowError fErr && !string.IsNullOrEmpty(fErr.Detail))
    {
      details.Detail = fErr.Detail;
    }

    inner.Do(
      innerErrors =>
      {
        details.Extensions = new Dictionary<string, object?>
        {
          { "innerErrors", innerErrors.ToList() },
        };
      }
    );

    return details;
  }
}