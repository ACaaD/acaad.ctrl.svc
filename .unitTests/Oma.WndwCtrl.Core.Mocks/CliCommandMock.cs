using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Core.Mocks;

public static partial class StringExtensions
{
  [GeneratedRegex(@"\r\n|\n|\\r\\n|\\n")]
  private static partial Regex NewLineRegex();

  public static IEnumerable<string> SplitToLines(this string input)
  {
    Regex split = NewLineRegex();
    string[] res = split.Split(input);
    return res;
  }
}

[PublicAPI]
public class CliCommandMock
{
  public static int Main(string[] args)
  {
    using Parser parser = new(
      with =>
      {
        with.CaseSensitive = false;
        with.CaseInsensitiveEnumValues = true;
        with.EnableDashDash = true;
      }
    );

    ParserResult<CliInvocationOptions>? parserResult =
      parser.ParseArguments<CliInvocationOptions>(args);

    if (parserResult.Tag != ParserResultType.NotParsed)
    {
      return RunOptions(parserResult.Value);
    }

    string helpText = HelpText.AutoBuild(parserResult).ToString();
    Console.Error.WriteLine("Incorrect arguments provided. Please refer to the following help text:");
    Console.WriteLine(helpText);

    return 255; // Reserved exit code to indicate something is wrong with the invocation.
  }

  private static int RunOptions(CliInvocationOptions opts)
  {
    if (opts.ExitCode >= 255)
    {
      Console.Error.WriteLine("Exit code must be less than 255. This is not a valid test.");
      return 255;
    }

    List<Action<string>> writers = [];

    if (opts.TargetStreams.HasFlag(CliInvocationOptions.OutputTarget.StdOut))
    {
      writers.Add(Console.WriteLine);
    }

    if (opts.TargetStreams.HasFlag(CliInvocationOptions.OutputTarget.StdErr))
    {
      writers.Add(Console.Error.WriteLine);
    }

    WriteText(opts.Text, writers);

    return opts.ExitCode;
  }

  private static void WriteText(string text, List<Action<string>> writers)
  {
    foreach (string line in text.SplitToLines())
    foreach (Action<string> writer in writers)
      writer(line);
  }
}

[PublicAPI]
public record CliInvocationOptions
{
  [Flags]
  public enum OutputTarget
  {
    StdOut = 1,
    StdErr = 2,
  }

  [Option(shortName: 'c', "code", Required = false, HelpText = "The exit code to return.")]
  public int ExitCode { get; init; }

  [Option(
    shortName: 's',
    "targetStreams",
    Required = false,
    HelpText = "If true, writes provided text to standard error instead of out."
  )]
  public OutputTarget TargetStreams { get; init; } = OutputTarget.StdOut;

  [Option(shortName: 't', "text", Required = true, HelpText = "The text to write to standard out or error.")]
  public string Text { get; init; } = string.Empty;

  public override string ToString() =>
    $"--code {ExitCode} --targetStreams \"{TargetStreams}\" --text \"{Text}\"";
}