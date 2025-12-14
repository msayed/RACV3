using Microsoft.Extensions.Logging;

namespace RACFlightDataService.Functions.Logging;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddStructuredConsole(this ILoggingBuilder builder)
    {
        return builder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
            options.SingleLine = true;
        });
    }
}
