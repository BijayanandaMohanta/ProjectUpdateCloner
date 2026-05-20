namespace ProjectUpdateCloner.Models;

public enum LogLevel
{
    Info,
    Success,
    Skip,
    Error
}

public sealed record LogEntry(LogLevel Level, string Message);
