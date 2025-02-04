using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Internal;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public partial class TransformationChainTests
{
  public static class SampleCommandOutcomes
  {
    internal const string Ping = """

                                 Pinging 1.1.1.1 with 32 bytes of data:
                                 Reply from 1.1.1.1: bytes=32 time=8ms TTL=58
                                 Reply from 1.1.1.1: bytes=32 time=7ms TTL=58
                                 Reply from 1.1.1.1: bytes=32 time=7ms TTL=58
                                 Reply from 1.1.1.1: bytes=32 time=8ms TTL=58

                                 Ping statistics for 1.1.1.1:
                                     Packets: Sent = 4, Received = 4, Lost = 0 (0% loss),
                                 Approximate round trip times in milli-seconds:
                                     Minimum = 7ms, Maximum = 8ms, Average = 7ms

                                 """;

    internal const string PingTimeout = """

                                        Pinging 1.1.1.1 with 32 bytes of data:
                                        PING: transmit failed. General failure.
                                        PING: transmit failed. General failure.
                                        PING: transmit failed. General failure.
                                        PING: transmit failed. General failure.

                                        Ping statistics for 2a00:1450:4001:806::200e:
                                            Packets: Sent = 4, Received = 0, Lost = 4 (100% loss),

                                        """;
  }

  public static class Payloads
  {
    internal const string DummyCommandNoTransformation = """
                                                         {
                                                           "type": "dummy",
                                                           "returns": [
                                                           ],
                                                           "transformations": [
                                                           ]
                                                         }
                                                         """;

    private const string PingResultOneTransformationTemplate = """
                                                               {
                                                                 "type": "dummy",
                                                                 "returns": $$$INJECT_MULTILINE_ARRAY$$$,
                                                                 "transformations": [
                                                                   {
                                                                     "type": "parser",
                                                                     "statements": [
                                                                       "Regex.Match($'time=(\\d+)ms');",
                                                                       "Regex.YieldGroup(1);",
                                                                       "Values.Average();"
                                                                     ]
                                                                   }
                                                                 ]
                                                               }
                                                               """;

    private const string PingResultMultipleTransformationsTemplate = """
                                                                     {
                                                                       "type": "dummy",
                                                                       "returns": $$$INJECT_MULTILINE_ARRAY$$$,
                                                                       "transformations": [
                                                                         {
                                                                           "type": "parser",
                                                                           "statements": [
                                                                             "Regex.Match($'time=(\\d+)ms');"
                                                                           ]
                                                                         },
                                                                         {
                                                                           "type": "parser",
                                                                           "statements": [
                                                                             "Regex.YieldGroup(1);"
                                                                           ]
                                                                         },
                                                                         {
                                                                           "type": "parser",
                                                                           "statements": [
                                                                             "Values.Average();"
                                                                           ]
                                                                         }
                                                                       ]
                                                                     }
                                                                     """;

    internal static string PingResultOneTransformation =>
      InjectMultilineArray(PingResultOneTransformationTemplate, SampleCommandOutcomes.Ping);

    internal static string PingResultMultipleTransformations =>
      InjectMultilineArray(PingResultMultipleTransformationsTemplate, SampleCommandOutcomes.Ping);

    /// <summary>
    /// Simulates a ping cli result when there is, for example, no internet connection.
    /// The transformation relies on the presence of at least one value to aggregate, but there is none.
    /// Refer to GitHub Issue <see href="https://github.com/OlliMartin/wndw.ctl/issues/35">#35</see>
    /// </summary>
    internal static string PingTimeoutResultOneTransformation =>
      InjectMultilineArray(PingResultOneTransformationTemplate, SampleCommandOutcomes.PingTimeout);

    private static string InjectMultilineArray(string template, string toInject)
    {
      string[] lines = toInject.Split(Environment.NewLine);
      JsonArray array = [];
      lines.ForEach(line => array.Add(JsonValue.Create(line)));

      template = template.Replace("$$$INJECT_MULTILINE_ARRAY$$$", array.ToJsonString());
      JsonObject json = JsonSerializer.Deserialize<JsonObject>(template)!;
      return json.ToJsonString();
    }
  }
}