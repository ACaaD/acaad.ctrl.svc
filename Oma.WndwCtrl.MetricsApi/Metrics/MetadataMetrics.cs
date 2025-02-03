using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Oma.WndwCtrl.Core.Model.Settings;

namespace Oma.WndwCtrl.MetricsApi.Metrics;

public class MetadataMetrics
{
  private readonly Gauge<int> _targetInfo;

  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Won't fix; Meter instances are not meant to be disposed."
  )]
  public MetadataMetrics(IMeterFactory meterFactory)
  {
    Meter meter = meterFactory.Create("ACaaD.Metadata");

    _targetInfo = meter.CreateGauge<int>("acaad.target.info", description: "Target Metadata");
  }

  public void PopulateMetadata(GeneralSettings settings)
  {
    TagList tags = default;
    tags.Add("acaad.name", settings.Name);

    _targetInfo.Record(value: 1, tags);
  }
}