
using Avalonia;
using Avalonia.Markup.Xaml;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using SteamCloudFileManager.UI.Models;
using SteamCloudFileManager.UI.Services;
using SteamCloudFileManager.UI.ViewModels;
using SteamCloudFileManager.UI.Views;

namespace SteamCloudFileManager.UI;

public partial class App : PrismApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    protected override AvaloniaObject CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }
    
    protected override void OnInitialized()
    {
        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("content", nameof(GameSelectionView));
    }
    
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IFileDialogService>(new FileDialogService());
        containerRegistry.RegisterSingleton<IGameStorageModel, GameStorageModel>();
        
        containerRegistry.RegisterDialog<DialogView, DialogViewModel>();
        
        containerRegistry.RegisterForNavigation<GameCloudStorageView, GameCloudStorageViewModel>();
        containerRegistry.RegisterForNavigation<GameSelectionView, GameSelectionViewModel>();
    }
}