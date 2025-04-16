using Newtonsoft.Json;
using System;
using System.IO;

namespace revit_mcp_plugin.Utils;

public static class PathManager
{
    /// <summary>
    /// Gets the root application data directory
    /// </summary>
    public static string GetAppDataDirectoryPath()
    {
        var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var applicationDirectory = Path.GetDirectoryName(applicationPath);

        return applicationDirectory;
    }
    /// <summary>
    /// Gets the path to the Commands directory
    /// </summary>
    public static string GetCommandsDirectoryPath()
    {
        var appDataDirectory = GetAppDataDirectoryPath();
        var commandsDirectory = Path.Combine(appDataDirectory, "Commands");

        EnsureDirectoryExists(commandsDirectory);

        return commandsDirectory;
    }
    /// <summary>
    /// Gets the path to the Logs directory
    /// </summary>
    public static string GetLogsDirectoryPath()
    {
        var appDataDirectory = GetAppDataDirectoryPath();
        var logsDirectory = Path.Combine(appDataDirectory, "Logs");

        EnsureDirectoryExists(logsDirectory);

        return logsDirectory;
    }
    /// <summary>
    /// Gets the path to the command registry file.
    /// If the file doesn't exist, creates it with default content.
    /// </summary>
    /// <param name="createIfNotExists">Whether to create a default file if it doesn't exist (default: true)</param>
    /// <returns>Path to the command registry file</returns>
    public static string GetCommandRegistryFilePath(bool createIfNotExists = true)
    {
        var commandsDirectory = GetCommandsDirectoryPath();
        var registryFilePath = Path.Combine(commandsDirectory, "commandRegistry.json");

        if (createIfNotExists && !File.Exists(registryFilePath))
        {
            CreateDefaultCommandRegistryFile(registryFilePath);
        }

        return registryFilePath;
    }
    /// <summary>
    /// Creates a default command registry file with empty commands array
    /// </summary>
    /// <param name="filePath">Path where to create the file</param>
    private static void CreateDefaultCommandRegistryFile(string filePath)
    {
        try
        {
            var defaultRegistry = new { commands = new object[] { } };
            var jsonContent = JsonConvert.SerializeObject(defaultRegistry, Formatting.Indented);

            File.WriteAllText(filePath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default command registry file: {ex.Message}");
        }
    }
    /// <summary>
    /// Ensures that the specified directory exists
    /// </summary>
    /// <param name="directoryPath">The path to check and create if needed</param>
    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}