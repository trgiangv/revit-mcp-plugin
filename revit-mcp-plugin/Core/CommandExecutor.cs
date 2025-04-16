using Newtonsoft.Json.Linq;
using RevitMCPSDK.API.Interfaces;
using RevitMCPSDK.API.Models.JsonRPC;
using RevitMCPSDK.Exceptions;
using System;

namespace revit_mcp_plugin.Core;

public class CommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger _logger;

    public CommandExecutor(ICommandRegistry commandRegistry, ILogger logger)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
    }

    public string ExecuteCommand(JsonRPCRequest request)
    {
        try
        {
            // Find commands
            if (!_commandRegistry.TryGetCommand(request.Method, out var command))
            {
                _logger.Warning("未找到命令: {0}", request.Method);
                return CreateErrorResponse(request.Id,
                    JsonRPCErrorCodes.MethodNotFound,
                    $"未找到方法: '{request.Method}'");
            }

            _logger.Info("执行命令: {0}", request.Method);

            // Execute the command
            try
            {
                var result = command.Execute(request.GetParamsObject(), request.Id);
                _logger.Info("命令 {0} 执行成功", request.Method);

                return CreateSuccessResponse(request.Id, result);
            }
            catch (CommandExecutionException ex)
            {
                _logger.Error("命令 {0} 执行失败: {1}", request.Method, ex.Message);
                return CreateErrorResponse(request.Id,
                    ex.ErrorCode,
                    ex.Message,
                    ex.ErrorData);
            }
            catch (Exception ex)
            {
                _logger.Error("An exception occurred while command {0} was executed: {1}", request.Method, ex.Message);
                return CreateErrorResponse(request.Id,
                    JsonRPCErrorCodes.InternalError,
                    ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("An exception occurred during execution of command processing: {0}", ex.Message);
            return CreateErrorResponse(request.Id,
                JsonRPCErrorCodes.InternalError,
                $"Internal error: {ex.Message}");
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