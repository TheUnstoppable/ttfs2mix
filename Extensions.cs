namespace Ttfs2Mix;

public static class Extensions
{
    extension(AnsiConsole)
    {
        public static void ErrorLine(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{message}[/]");
        }
        
        public static void WarningLine(string message)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{message}[/]");
        }
        
        public static void Exception(Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
        }
    }
}