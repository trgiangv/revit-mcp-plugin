using Newtonsoft.Json;

namespace revit_mcp_plugin.Configuration;

/// <summary>
/// 服务设置类
/// </summary>
public class ServiceSettings
{
    /// <summary>
    /// Log Level
    /// </summary>
    [JsonProperty("logLevel")]
    public string LogLevel { get; set; } = "Info";

    /// <summary>
    /// socket service port
    /// </summary>
    [JsonProperty("port")]
    public int Port { get; set; } = 8080;

}