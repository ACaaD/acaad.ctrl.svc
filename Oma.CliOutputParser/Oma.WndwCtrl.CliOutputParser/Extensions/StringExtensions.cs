namespace Oma.WndwCtrl.CliOutputParser.Extensions;

public static class StringExtensions
{
    public static string From(this string text, string from)
    {
        int start = text.IndexOf(from, StringComparison.Ordinal);
        if (start == -1)
        {
            return text;
        }
        
        return text.Substring(start);
    }

    public static string To(this string text, string to)
    {
        int end = text.IndexOf(to, StringComparison.Ordinal);

        if (end == -1)
        {
            // TODO: Does this make sense here? If the string is not found we might want to return nothing.
            return text;
        }
        
        return text.Substring(0, end + to.Length);
    }
}