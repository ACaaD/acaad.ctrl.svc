using FluentAssertions;
using Xunit.Abstractions;

namespace Oma.WndwCtrl.CliOutputParser.Tests;

public class CliOutputParserImplTests
{
    private const string _testInputPing = """
                                          $ ping xkcd.com

                                          Pinging xkcd.com [151.101.64.67] with 32 bytes of data:
                                          Reply from 151.101.64.67: bytes=32 time=8ms TTL=59
                                          Reply from 151.101.64.67: bytes=32 time=9ms TTL=59
                                          Reply from 151.101.64.67: bytes=32 time=8ms TTL=59
                                          Reply from 151.101.64.67: bytes=32 time=8ms TTL=59

                                          Ping statistics for 151.101.64.67:
                                              Packets: Sent = 4, Received = 4, Lost = 0 (0% loss),
                                          Approximate round trip times in milli-seconds:
                                              Minimum = 8ms, Maximum = 9ms, Average = 8ms
                                          """;

    private const string _testInputNested = """
                                            1 2 3
                                            4 5 6
                                            7 8 9
                                            """;

    private const string _testInputNested2 = """
                                            1.a 1.b 1.c
                                            2.d 2.e 2.f
                                            3.g 3.h 3.i
                                            """;

    private readonly CliOutputParserImpl _instance;

    public CliOutputParserImplTests(ITestOutputHelper outputHelper)
    {
        _instance = new(obj => outputHelper.WriteLine(obj.ToString()?.Replace("\r", string.Empty)));
    }

    [Fact]
    public void ShouldParseTransformationSuccessfully()
    {
        const string transformationInput = """
                                           Anchor.From("Pinging xkcd.com");
                                           Anchor.To("Ping statistics");
                                           Regex.Match($"time=(\d+)ms");
                                           Regex.YieldGroup(1); 
                                           Values.Average();
                                           """;

        List<object> output = new();
        
        var action = () =>
        {
            var enumerable = _instance.Parse(transformationInput, _testInputPing);
            output = enumerable.ToList();
        };
        action.Should().NotThrow();
        output.Should().HaveCount(1);
        output.First().Should().Be(8.25);
    }

    [Fact]
    public void ShouldSkipIfItemNotValidInReduce()
    {
        const string text = """
                            match 1
                            no-match 2
                            match 3
                            """;

        const string transformation = """
                                      Regex.Match($"^match\s(\d)");
                                      Regex.YieldGroup(1); 
                                      Values.Sum();
                                      """;
        
        List<object> output = new();
        
        var action = () =>
        {
            var enumerable = _instance.Parse(transformation, text);
            output = enumerable.ToList();
        };
        action.Should().NotThrow();
        output.Should().HaveCount(1);
        output.First().Should().Be(4); // Not 6 because there is no match in the regex.
    }
    
    [Fact]
    public void ShouldFailOnExtraneousInput()
    {
        const string transformationInput = """
                                           Anchor.From("Pinging xkcd.com").To("Ping statistics");
                                           Regex.Match($"time=(\d+)ms").YieldGroup(1); 
                                           Values.Average2();
                                           """;

        var action = () => _instance.Parse(transformationInput, _testInputPing);

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldApplyAnchors()
    {
        const string transformationInput = """
                                           Anchor.From("statistics");
                                           Anchor.To("151.101.64.67");
                                           """;

        List<object> output = new();

        var action = () =>
        {
            var enumerable = _instance.Parse(transformationInput, _testInputPing);
            output = enumerable.ToList();
        };

        action.Should().NotThrow();
        output.Should().HaveCount(1);
        output.First().Should().Be("statistics for 151.101.64.67");
    }

    [Fact]
    public void ShouldHandleNestedTransformations()
    {
        const string transformationInput = """
                                           Regex.Match($"^.*$"); // [ s ] -> [ [ s1, s2 ] ]
                                           Regex.Match($"(\d)"); // [ [ s ] ] -> [ [ [ s ] ] ]
                                           Values.Last(); // Choose inner-most regex group
                                           Values.Last(); // Choose from line
                                           Values.First(); // Choose i dont know what
                                           """;

        List<object> output = new();

        var action = () =>
        {
            var enumerable = _instance.Parse(transformationInput, _testInputNested);
            output = enumerable.ToList();
        };

        action.Should().NotThrow();

        output.Should().HaveCount(1);
        output.First().Should().Be("7");
    }

    [Fact]
    public void ShouldHandleDoubleNestedTransformations()
    {
        const string transformationInput = """
                                           Regex.Match($"^.*$");
                                           Regex.Match($"\d\.\w");
                                           Regex.Match($".");
                                           Values.Last();
                                           Values.Last();
                                           Values.Last();
                                           Values.Last();
                                           """;

        List<object> output = new();

        var action = () => { output = _instance.Parse(transformationInput, _testInputNested2).ToList(); };

        action.Should().NotThrow();

        output.Should().HaveCount(1);
        output.First().Should().Be("i");
    }
}
