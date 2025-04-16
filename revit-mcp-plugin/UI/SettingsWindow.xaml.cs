using System.Windows.Controls;

namespace revit_mcp_plugin.UI;

/// <summary>
/// Settings.xaml 的交互逻辑
/// </summary>
public partial class SettingsWindow
{
    private readonly CommandSetSettingsPage _commandSetPage;
    private readonly bool _isInitialized = false;

    public SettingsWindow()
    {
        InitializeComponent();

        // Initialize the page
        _commandSetPage = new CommandSetSettingsPage();

        // Load the default page
        ContentFrame.Navigate(_commandSetPage);

        _isInitialized = true;
    }

    private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        if (Equals(NavListBox.SelectedItem, CommandSetItem))
        {
            ContentFrame.Navigate(_commandSetPage);
        }
    }
}