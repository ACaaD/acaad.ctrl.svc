namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public partial class CommandDeserialization
{
    public static class Payloads
    {
        internal const string NoOpCommand = """
                                           {
                                             "type": "cli",
                                             "fileName": "",
                                             "arguments": "",
                                             "retries": 1,
                                             "timeout": "00:00:00",
                                             "transformations": [
                                               {}
                                             ]
                                           }
                                           """;
    }
}