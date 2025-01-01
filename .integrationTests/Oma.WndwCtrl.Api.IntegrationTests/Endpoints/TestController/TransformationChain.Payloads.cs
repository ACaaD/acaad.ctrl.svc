using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Internal;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public partial class TransformationChain
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

    internal const string PingResultOneTransformationTemplate = """
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

    internal const string PingResultMultipleTransformationsTemplate = """
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