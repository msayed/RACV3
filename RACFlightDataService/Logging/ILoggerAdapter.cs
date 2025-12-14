using System;

namespace RACFlightDataService.Logging;

public interface ILoggerAdapter<TType>
{
    void SetServiceName(string name);
    void LogInformation(string? message, params object?[] args);

    void LogError(Exception? exception, string? message, params object?[] args);
    void LogError( string? message, params object?[] args);
    void LogWarning(Exception? exception, string? message, params object?[] args);
    void LogWarning(string? message, params object?[] args);
    void LogDebug(string message);
    void LogDebug(string message, params object[] args);
}