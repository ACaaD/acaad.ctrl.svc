using FluentAssertions;

namespace Oma.WndwCtrl.CliOutputParser.Tests;

public class CliOutputParserImplTests
{
    private readonly CliOutputParserImpl _instance;
    
    public CliOutputParserImplTests()
    {
        _instance = new();
    }

    [Fact]
    public void ShouldParseTransformationSuccessfully()
    {
        const string transformationInput = """
                                           Anchor.From("Pinging xkcd.com").To("Ping statistics");
                                           Regex.Match($"time=(\d+)ms").YieldGroup(1); 
                                           Values.Average();
                                           """;
        
        var action = () => _instance.Parse(transformationInput);
        
        action.Should().NotThrow();
    }
    
    [Fact]
    public void ShouldFailOnExtraneousInput()
    {
        const string transformationInput = """
                                           Anchor.From("Pinging xkcd.com").To("Ping statistics");
                                           Regex.Match($"time=(\d+)ms").YieldGroup(1); 
                                           Values.Average2();
                                           """;
        
        var action = () => _instance.Parse(transformationInput);
        
        action.Should().Throw<Exception>();
    }
}