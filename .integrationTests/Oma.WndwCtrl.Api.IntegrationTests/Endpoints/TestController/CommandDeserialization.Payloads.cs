namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public partial class CommandDeserialization
{
    public static class Payloads
    {
        internal const string NoOpCommand = """
                                           {
                                             "type": "custom",
                                             "retries": 1,
                                             "timeout": "00:00:00",
                                             "transformations": [
                                             ]
                                           }
                                           """;
        
        internal const string ParserTransformationCommand = """
                                            {
                                              "type": "custom",
                                              "retries": 1,
                                              "timeout": "00:00:00",
                                              "transformations": [
                                                {
                                                  "type": "parser",
                                                  "statements": [ 
                                                    "Anchor.From('START');",
                                                    "Anchor.From('END');"
                                                  ]
                                                }
                                              ]
                                            }
                                            """;
    }
}