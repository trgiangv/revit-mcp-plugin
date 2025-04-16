using Newtonsoft.Json;

namespace revit_mcp_plugin.Configuration;

/// <summary>
/// 命令配置类
/// </summary>
public class CommandConfig
{
    /// <summary>
    /// Command name - corresponding IRevitCommand.CommandName
    /// </summary>
    [JsonProperty("commandName")]
    public string CommandName { get; set; }

    /// <summary>
    /// Assembly path - containing this command DLL
    /// </summary>
    [JsonProperty("assemblyPath")]
    public string AssemblyPath { get; set; }

    /// <summary>
    /// Whether to enable this command
    /// </summary>
    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Supported Revit versions
    /// </summary>
    [JsonProperty("supportedRevitVersions")]
    public string[] SupportedRevitVersions { get; set; } = new string[0];

    /// <summary>
    /// Developer information
    /// </summary>
    [JsonProperty("developer")]
    public DeveloperInfo Developer { get; set; } = new DeveloperInfo();

    /// <summary>
    /// Command description
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = "";
}