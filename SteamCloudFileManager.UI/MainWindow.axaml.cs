using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SteamCloudFileManager.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}