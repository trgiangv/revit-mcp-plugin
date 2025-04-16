using Autodesk.Revit.UI;
using RevitMCPSDK.API.Interfaces;
using RevitMCPSDK.API.Utils;
using revit_mcp_plugin.Configuration;
using revit_mcp_plugin.Utils;
using System;
using System.IO;
using System.Reflection;

namespace revit_mcp_plugin.Core;

/// <summary>
/// Command Manager, responsible for loading and managing commands
/// </summary>
public class CommandManager
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger _logger;
    private readonly ConfigurationManager _configManager;
    private readonly UIApplication _uiApplication;
    private readonly RevitVersionAdapter _versionAdapter;

    public CommandManager(
        ICommandRegistry commandRegistry,
        ILogger logger,
        ConfigurationManager configManager,
        UIApplication uiApplication)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
        _configManager = configManager;
        _uiApplication = uiApplication;
        _versionAdapter = new RevitVersionAdapter(_uiApplication.Application);
    }

    /// <summary>
    /// Load all commands specified in the configuration file
    /// </summary>
    public void LoadCommands()
    {
        _logger.Info("Start loading command");
        var currentVersion = _versionAdapter.GetRevitVersion();
        _logger.Info("Current Revit version: {0}", currentVersion);

        // Load external commands from configuration
        foreach (var commandConfig in _configManager.Config.Commands)
        {
            try
            {
                if (!commandConfig.Enabled)
                {
                    _logger.Info("Skip the disabled command: {0}", commandConfig.CommandName);
                    continue;
                }

                // Check version compatibility
                if (commandConfig.SupportedRevitVersions != null &&
                    commandConfig.SupportedRevitVersions.Length > 0 &&
                    !_versionAdapter.IsVersionSupported(commandConfig.SupportedRevitVersions))
                {
                    _logger.Warning("命令 {0} 不支持当前 Revit 版本 {1}，已跳过",
                        commandConfig.CommandName, currentVersion);
                    continue;
                }

                // Replace version placeholders in paths
                commandConfig.AssemblyPath = commandConfig.AssemblyPath.Contains("{VERSION}")
                    ? commandConfig.AssemblyPath.Replace("{VERSION}", currentVersion)
                    : commandConfig.AssemblyPath;

                // Load external command assembly
                LoadCommandFromAssembly(commandConfig);
            }
            catch (Exception ex)
            {
                _logger.Error("Loading command {0} failed: {1}", commandConfig.CommandName, ex.Message);
            }
        }

        _logger.Info("Command loading is complete");
    }

    /// <summary>
    /// 加载特定程序集中的特定命令
    /// </summary>
    /// <param name="config">commandConfig</param>
    private void LoadCommandFromAssembly(CommandConfig config)
    {
        try
        {
            // Determine the assembly path
            var assemblyPath = config.AssemblyPath;
            if (!Path.IsPathRooted(assemblyPath))
            {
                // If not an absolute path, relative to the Commands directory
                var baseDir = PathManager.GetCommandsDirectoryPath();
                assemblyPath = Path.Combine(baseDir, assemblyPath);
            }

            if (!File.Exists(assemblyPath))
            {
                _logger.Error("The command assembly does not exist: {0}", assemblyPath);
                return;
            }

            // Loading assembly
            var assembly = Assembly.LoadFrom(assemblyPath);

            // Find the type that implements the IRevitCommand interface
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IRevitCommand).IsAssignableFrom(type) &&
                    !type.IsInterface &&
                    !type.IsAbstract)
                {
                    try
                    {
                        // Create a command instance
                        IRevitCommand command;

                        // Check whether the command implements an initializable interface
                        if (typeof(IRevitCommandInitializable).IsAssignableFrom(type))
                        {
                            // Create an instance and initialize it
                            command = (IRevitCommand)Activator.CreateInstance(type);
                            ((IRevitCommandInitializable)command).Initialize(_uiApplication);
                        }
                        else
                        {
                            // Try to find a constructor that accepts UIApplication
                            var constructor = type.GetConstructor(new[] { typeof(UIApplication) });
                            if (constructor != null)
                            {
                                command = (IRevitCommand)constructor.Invoke(new object[] { _uiApplication });
                            }
                            else
                            {
                                // Use parameterless constructor
                                command = (IRevitCommand)Activator.CreateInstance(type);
                            }
                        }

                        // Check if the command name matches the configuration
                        if (command.CommandName == config.CommandName)
                        {
                            _commandRegistry.RegisterCommand(command);
                            _logger.Info("Registered external command: {0} (from {1})",
                                command.CommandName, Path.GetFileName(assemblyPath));
                            break; // Exit the loop after finding the matching command
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Create command instance failed [{0}]: {1}", type.FullName, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load the command assembly: {0}", ex.Message);
        }
    }
}