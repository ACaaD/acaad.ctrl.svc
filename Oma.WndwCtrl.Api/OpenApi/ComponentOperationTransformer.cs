using LanguageExt;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Attributes;
using Oma.WndwCtrl.Api.OpenApi.Interfaces;
using Oma.WndwCtrl.Api.OpenApi.Model;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Api.OpenApi;

public class ComponentOperationTransformer(ILogger<ComponentOperationTransformer> logger)
  : IOpenApiOperationTransformer
{
  public async Task TransformAsync(
    OpenApiOperation operation,
    OpenApiOperationTransformerContext context,
    CancellationToken cancellationToken
  )
  {
    if (context.Description.ActionDescriptor.Properties.TryGetValue(
          nameof(Component),
          out object? componentObj
        )
        && componentObj is IComponent component)
    {
      ActionDescriptor ad = context.Description.ActionDescriptor;
      List<AcaadAttribute> acaadMetadata = ad.EndpointMetadata.OfType<AcaadAttribute>().ToList();

      if (acaadMetadata.Any(m => m is AcaadHiddenAttribute))
      {
        return;
      }

      Option<IOpenApiExtension> acaadExtension = await CreateAcaadExtension(
        context.ApplicationServices,
        component,
        acaadMetadata
      );

      acaadExtension.Do(ext => operation.Extensions.Add("acaad", ext));
    }
  }

  private async Task<Option<IOpenApiExtension>> CreateAcaadExtension(
    IServiceProvider serviceProvider,
    IComponent component,
    List<AcaadAttribute> acaadMetadata
  )
  {
    IEnumerable<IOpenApiComponentWriter> writers = serviceProvider.GetServices<IOpenApiComponentWriter>();

    List<IOpenApiComponentWriter> potentialWriters = writers.Where(w => w.Handles(component)).ToList();

    switch (potentialWriters.Count)
    {
      case 0:
        logger.LogWarning(
          "Found no component writer that handles {component}. Skipping.",
          component.GetType().Name
        );

        return Option<IOpenApiExtension>.None;
      case > 1:
        logger.LogWarning(
          "Found more than one component writer that handles {component}. Skipping.",
          component.GetType().Name
        );

        return Option<IOpenApiExtension>.None;
      default:
      {
        Option<OpenApiComponentExtension> result =
          await potentialWriters[index: 0].CreateExtensionAsync(component);

        return result.Map<IOpenApiExtension>(ext => ext.ApplyMetadata(acaadMetadata));
      }
    }
  }
}