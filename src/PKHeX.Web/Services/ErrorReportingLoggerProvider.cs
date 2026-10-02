namespace PKHeX.Web.Services;

// Reports the exceptions .NET logs as errors, such as unhandled rendering exceptions, like Sentry's logging provider did.
public sealed class ErrorReportingLoggerProvider(IServiceProvider services) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(services);

    public void Dispose()
    {
    }

    private sealed class Logger(IServiceProvider services) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel) && exception is not null) services.GetRequiredService<AnalyticsService>().CaptureError(exception);
        }
    }
}
