﻿using System;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;



namespace revit_mcp_plugin.Core;

public class Application : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        var mcpPanel = application.CreateRibbonPanel("Revit MCP Plugin");

        var pushButtonData = new PushButtonData("ID_EXCMD_TOGGLE_REVIT_MCP", "Revit MCP\r\n Switch",
            Assembly.GetExecutingAssembly().Location, "revit_mcp_plugin.Core.MCPServiceConnection");
        pushButtonData.ToolTip = "Open / Close mcp server";
        pushButtonData.Image = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources/icon-16.png", UriKind.RelativeOrAbsolute));
        pushButtonData.LargeImage = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources/icon-32.png", UriKind.RelativeOrAbsolute));
        mcpPanel.AddItem(pushButtonData);

        var mcp_settings_pushButtonData = new PushButtonData("ID_EXCMD_MCP_SETTINGS", "Settings",
            Assembly.GetExecutingAssembly().Location, "revit_mcp_plugin.Core.Settings");
        mcp_settings_pushButtonData.ToolTip = "MCP Settings";
        mcp_settings_pushButtonData.Image = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources/settings-16.png", UriKind.RelativeOrAbsolute));
        mcp_settings_pushButtonData.LargeImage = new BitmapImage(new Uri("/revit-mcp-plugin;component/Core/Ressources/settings-32.png", UriKind.RelativeOrAbsolute));
        mcpPanel.AddItem(mcp_settings_pushButtonData);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            if (SocketService.Instance.IsRunning)
            {
                SocketService.Instance.Stop();
            }
        }
        catch { }

        return Result.Succeeded;
    }
}