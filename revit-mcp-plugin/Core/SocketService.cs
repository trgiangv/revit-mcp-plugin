using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevitMCPSDK.API.Models.JsonRPC;
using RevitMCPSDK.API.Interfaces;
using revit_mcp_plugin.Configuration;
using revit_mcp_plugin.Utils;

namespace revit_mcp_plugin.Core;

public class SocketService
{
    private static SocketService _instance;
    private TcpListener _listener;
    private Thread _listenerThread;
    private UIApplication _uiApp;
    private ICommandRegistry _commandRegistry;
    private ILogger _logger;
    private CommandExecutor _commandExecutor;

    public static SocketService Instance => _instance ??= new SocketService();

    private SocketService()
    {
        _commandRegistry = new RevitCommandRegistry();
        _logger = new Logger();
    }

    public bool IsRunning { get; private set; }

    public int Port { get; set; } = 8080;

    // Initialize
    public void Initialize(UIApplication uiApp)
    {
        _uiApp = uiApp;

        // Initialize event manager
        ExternalEventManager.Instance.Initialize(uiApp, _logger);

        // Record the current Revit version
        var versionAdapter = new RevitMCPSDK.API.Utils.RevitVersionAdapter(_uiApp.Application);
        var currentVersion = versionAdapter.GetRevitVersion();
        _logger.Info("当前 Revit 版本: {0}", currentVersion);



        // Create a command executor
        _commandExecutor = new CommandExecutor(_commandRegistry, _logger);

        // Load configuration and register commands
        var configManager = new ConfigurationManager(_logger);
        configManager.LoadConfiguration();
            

        //// Read the service port from the configuration
        //if (configManager.Config.Settings.Port > 0)
        //{
        //    _port = configManager.Config.Settings.Port;
        //}
        Port = 8080; // Fixed port number

        // Loading command
        var commandManager = new CommandManager(
            _commandRegistry, _logger, configManager, _uiApp);
        commandManager.LoadCommands();

        _logger.Info($"Socket service initialized on port {Port}");
    }

    public void Start()
    {
        if (IsRunning) return;

        try
        {
            IsRunning = true;
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();

            _listenerThread = new Thread(ListenForClients)
            {
                IsBackground = true
            };
            _listenerThread.Start();              
        }
        catch (Exception)
        {
            IsRunning = false;
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;

        try
        {
            IsRunning = false;

            _listener?.Stop();
            _listener = null;

            if(_listenerThread!=null && _listenerThread.IsAlive)
            {
                _listenerThread.Join(1000);
            }
        }
        catch (Exception)
        {
            // log error
        }
    }

    private void ListenForClients()
    {
        try
        {
            while (IsRunning)
            {
                var client = _listener.AcceptTcpClient();

                var clientThread = new Thread(HandleClientCommunication)
                {
                    IsBackground = true
                };
                clientThread.Start(client);
            }
        }
        catch (SocketException)
        {
                
        }
        catch(Exception)
        {
            // log
        }
    }

    private void HandleClientCommunication(object clientObj)
    {
        var tcpClient = (TcpClient)clientObj;
        var stream = tcpClient.GetStream();

        try
        {
            var buffer = new byte[8192];

            while (IsRunning && tcpClient.Connected)
            {
                // Read client messages
                var bytesRead = 0;

                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException)
                {
                    // Client disconnection
                    break;
                }

                if (bytesRead == 0)
                {
                    // Client disconnection
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                System.Diagnostics.Trace.WriteLine($"收到消息: {message}");

                var response = ProcessJsonRPCRequest(message);

                // Send a response
                var responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
        }
        catch(Exception)
        {
            // log
        }
        finally
        {
            tcpClient.Close();
        }
    }

    private string ProcessJsonRPCRequest(string requestJson)
    {
        JsonRPCRequest request;

        try
        {
            // Parse JSON-RPC requests
            request = JsonConvert.DeserializeObject<JsonRPCRequest>(requestJson);

            // Verify that the request format is valid
            if (request == null || !request.IsValid())
            {
                return CreateErrorResponse(
                    null,
                    JsonRPCErrorCodes.InvalidRequest,
                    "Invalid JSON-RPC request"
                );
            }

            // Find commands
            if (!_commandRegistry.TryGetCommand(request.Method, out var command))
            {
                return CreateErrorResponse(request.Id, JsonRPCErrorCodes.MethodNotFound,
                    $"Method '{request.Method}' not found");
            }

            // Execute the command
            try
            {                
                var result = command.Execute(request.GetParamsObject(), request.Id);

                return CreateSuccessResponse(request.Id, result);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(request.Id, JsonRPCErrorCodes.InternalError, ex.Message);
            }
        }
        catch (JsonException)
        {
            // JSON parsing error
            return CreateErrorResponse(
                null,
                JsonRPCErrorCodes.ParseError,
                "Invalid JSON"
            );
        }
        catch (Exception ex)
        {
            // Other errors when processing requests
            return CreateErrorResponse(
                null,
                JsonRPCErrorCodes.InternalError,
                $"Internal error: {ex.Message}"
            );
        }
    }

    private string CreateSuccessResponse(string id, object result)
    {
        var response = new JsonRPCSuccessResponse
        {
            Id = id,
            Result = result is JToken jToken ? jToken : JToken.FromObject(result)
        };

        return response.ToJson();
    }

    private string CreateErrorResponse(string id, int code, string message, object data = null)
    {
        var response = new JsonRPCErrorResponse
        {
            Id = id,
            Error = new JsonRPCError
            {
                Code = code,
                Message = message,
                Data = data != null ? JToken.FromObject(data) : null
            }
        };

        return response.ToJson();
    }
}