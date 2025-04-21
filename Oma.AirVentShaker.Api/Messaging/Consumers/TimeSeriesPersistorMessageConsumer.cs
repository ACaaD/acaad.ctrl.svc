using System.Globalization;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.AirVentShaker.Api.Messaging.Consumers;

public sealed class TimeSeriesPersistorMessageConsumer(ILogger<TimeSeriesPersistorMessageConsumer> logger)
  : IMessageConsumer<GForceValueBatchEvent>, IDisposable
{
  private readonly InfluxDBClient _influx = new(
    "http://influx.mon.acaad.dev:8086",
    "aSJGpOeErhhn4Mwb8gYfoP-haRn3vd_Ef2mzvs-5qipYnX1tTrv50UwXcx9B2tJsjKq9jN9tU62gEy-VyNxewQ=="
  );

  public void Dispose()
  {
    _influx.Dispose();
  }

  public bool IsSubscribedTo(IMessage message) => message is GForceValueBatchEvent;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(
      exception,
      "An unexpected error occurred processing event {type}.",
      message.GetType().Name
    );

    return Task.CompletedTask;
  }

  public async Task OnMessageAsync(GForceValueBatchEvent message, CancellationToken cancelToken = default)
  {
    IEnumerable<string> stepNames = message.DataPoints
      .DistinctBy(dp => dp.TestStep?.ToString() ?? "none")
      .Select(dp => dp.TestStep?.ToString() ?? "none");

    logger.LogDebug(
      "Received {count} events. Average value: {avg} Test Steps: [{stepNames}]",
      message.DataPoints.Count,
      message.DataPoints.Average(dp => dp.NetForce),
      string.Join(", ", stepNames)
    );

    WriteApiAsync? writeApi = _influx.GetWriteApiAsync();

    List<PointData> points = message.DataPoints.Select(
      dp => PointData.Measurement("g-force")
        .Field("value", (double)dp.NetForce)
        .Field("target", (double)(dp.TestStep?.TargetGravitationalForce ?? 0))
        .Field("amplitude", dp.TestStep?.Amplitude ?? 0)
        .Timestamp(dp.AsOf, WritePrecision.Ms)
        .Tag("targetG", (dp.TestStep?.TargetGravitationalForce ?? -1).ToString(CultureInfo.InvariantCulture))
        .Tag("freq", (dp.TestStep?.Frequency ?? -1).ToString(CultureInfo.InvariantCulture))
        .Tag("test-name", dp.TestDefinition?.Name ?? "None")
    ).ToList();

    await writeApi.WritePointsAsync(points, "airvents", "ollimart.in", cancelToken);
  }
}