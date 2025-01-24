using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Oma.WndwCtrl.CliOutputParser.Metrics;

public class ParserMetrics
{
  private const string MeterName = "ACaaD.Processing.Parser";

  private const string CachedTransformationsCounterName = "acaad.processing.parser.transformations.cached";
  private const string InvalidTransformationsCounterName = "acaad.processing.parser.transformations.invalid";

  private const string ParseTreeGenerationDuration = "acaad.processing.parser.parse_tree_generation.duration";

  private readonly Counter<long> _cachedTransformationsCounter;
  private readonly Counter<long> _invalidTransformationsCounter;

  private readonly Histogram<double> _parseTreeGenerationDuration;

  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Won't fix; Meter instances are not meant to be disposed."
  )]
  public ParserMetrics(IMeterFactory meterFactory)
  {
    Meter meter = meterFactory.Create(MeterName);

    _cachedTransformationsCounter = meter.CreateCounter<long>(
      CachedTransformationsCounterName,
      description: "The amount of successfully cached transformations"
    );

    _invalidTransformationsCounter = meter.CreateCounter<long>(
      InvalidTransformationsCounterName,
      description: "The amount of invalid (malformed/non parseable) transformations"
    );

    _parseTreeGenerationDuration = meter.CreateHistogram(
      ParseTreeGenerationDuration,
      "ms",
      "Processing time to obtain parse tree",
      advice: new InstrumentAdvice<double>
      {
        HistogramBucketBoundaries =
          [0.05, 0.1, 0.25, 0.5, 1, 2, 3, 5, 10, 20, 30, 50, 80, 130,],
      }
    );
  }

  public void RecordTransformationCreation(string transformationId, string transformationHint)
  {
    TagList tags = GetTags(transformationId, transformationHint);

    _cachedTransformationsCounter.Add(delta: 1, tags);
  }

  public void RecordInvalidTransformation(string transformationId, string transformationHint)
  {
    TagList tags = GetTags(transformationId, transformationHint);

    _invalidTransformationsCounter.Add(delta: 1, tags);
  }

  private static TagList GetTags(string transformationId, string transformationHint)
  {
    TagList tags = default;
    tags.Add("transformation.id", transformationId);
    tags.Add("transformation.hint", transformationHint);
    return tags;
  }

  public void RecordParseTreeGenerationDuration(
    string transformationId,
    string transformationHint,
    TimeSpan duration
  )
  {
    TagList tags = GetTags(transformationId, transformationHint);

    _parseTreeGenerationDuration.Record(duration.TotalMicroseconds / 1000, tags);
  }
}