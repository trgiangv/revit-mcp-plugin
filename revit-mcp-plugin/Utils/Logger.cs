using RevitMCPSDK.API.Interfaces;
using System;
using System.IO;

namespace revit_mcp_plugin.Utils;

public class Logger : ILogger
{
    private readonly string _logFilePath;
    private const LogLevel CurrentLogLevel = LogLevel.Info;

    public Logger()
    {
        _logFilePath = Path.Combine(PathManager.GetLogsDirectoryPath(), $"mcp_{DateTime.Now:yyyyMMdd}.log");

    }

    public void Log(LogLevel level, string message, params object[] args)
    {
        if (level < CurrentLogLevel)
            return;

        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {formattedMessage}";

        // Output to Debug window
        System.Diagnostics.Debug.WriteLine(logEntry);

        // Write to log files
        try
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // If the log file fails to be written, no exception is thrown
        }
    }

    public void Debug(string message, params object[] args)
    {
        Log(LogLevel.Debug, message, args);
    }

    public void Info(string message, params object[] args)
    {
        Log(LogLevel.Info, message, args);
    }

    public void Warning(string message, params object[] args)
    {
        Log(LogLevel.Warning, message, args);
    }

    public void Error(string message, params object[] args)
    {
        Log(LogLevel.Error, message, args);
    }
}