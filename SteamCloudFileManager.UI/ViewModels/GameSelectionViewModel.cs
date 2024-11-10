using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using SteamCloudFileManager.UI.Enums;
using SteamCloudFileManager.UI.Models;
using SteamCloudFileManager.UI.Views;

namespace SteamCloudFileManager.UI.ViewModels;

public partial class GameSelectionViewModel : ViewModelBase
{
    Regex steamAppUrlRegex = SteamStoreLinkRegex();
    string appIdInput = "";

    public ICommand SelectGameCommand { get; }

    public string AppIdInput
    {
        get => appIdInput;
        set => SetProperty(ref appIdInput, value);
    }

    public GameSelectionViewModel(IDialogService dialogService, IRegionManager regionManager, IGameStorageModel gameStorageModel)
    {
        SelectGameCommand = new DelegateCommand(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppIdInput))
                {
                    ShowDialog("Validation error", "Please enter an App ID.", DialogType.Warn);
                    return;
                }

                if (!uint.TryParse(AppIdInput.Trim(), out var appId))
                {
                    var regexMatch = steamAppUrlRegex.Match(appIdInput);
                    if (!regexMatch.Success)
                    {
                        ShowDialog("Validation error", "Please make sure the App ID or Steam App URL you entered is valid.",
                            DialogType.Warn);
                        return;
                    }

                    if (regexMatch.Groups.Count < 2 || !uint.TryParse(regexMatch.Groups[1].Value.Trim(), out appId))
                    {
                        ShowDialog("Validation error", "Please make sure the Steam App URL you entered is valid.",
                            DialogType.Warn);
                        return;
                    }
                }

                gameStorageModel.SelectAppId(appId);
                regionManager.RequestNavigate("content", nameof(GameCloudStorageView));
            }
            catch (Exception ex)
            {
                ShowDialog("Failed to connect", ex.ToString(), DialogType.Error);
            }
        });
        
        return;

        void ShowDialog(string title, string message, DialogType type)
            => dialogService.ShowDialog(nameof(DialogView),
                new DialogParameters($"title={title}&message={message}&type={type}"));
    }

    [GeneratedRegex(@"^(?:https:\/\/|http:\/\/|)store\.steampowered\.com\/app\/(\d+)")]
    private static partial Regex SteamStoreLinkRegex();
}