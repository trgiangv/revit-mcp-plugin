using Newtonsoft.Json;
using RevitMCPSDK.API.Interfaces;
using revit_mcp_plugin.Utils;
using System;
using System.IO;

namespace revit_mcp_plugin.Configuration;

public class ConfigurationManager
{
    private readonly ILogger _logger;
    private readonly string _configPath;

    public FrameworkConfig Config { get; private set; }

    public ConfigurationManager(ILogger logger)
    {
        _logger = logger;

        // Configuration file path
        _configPath = PathManager.GetCommandRegistryFilePath();
    }

    /// <summary>
    /// Loading configuration
    /// </summary>
    public void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                Config = JsonConvert.DeserializeObject<FrameworkConfig>(json);
                _logger.Info("The configuration file has been loaded: {0}", _configPath);
            }
            else
            {
                _logger.Error("The configuration file was not found");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load the configuration file: {0}", ex.Message);
        }

        // Record the loading time
        _lastConfigLoadTime = DateTime.Now;
    }

    private DateTime _lastConfigLoadTime;
}