using Microsoft.Extensions.Configuration;

namespace Oma.WndwCtrl.Core.Model.Settings;

public class GeneralSettings
{
  public const string SectionName = "ACaaD";

  public string OS { get; init; } = "windows";
  public string Name { get; init; } = "not-configured (PLEASE FIX)";

  public bool UseOtlp { get; init; }

  public static GeneralSettings FromRoot(IConfiguration rootConfiguration) =>
    rootConfiguration.GetSection(SectionName).Get<GeneralSettings>() ?? new GeneralSettings();
}