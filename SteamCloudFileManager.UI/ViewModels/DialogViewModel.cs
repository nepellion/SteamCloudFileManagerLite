using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using SteamCloudFileManager.UI.Enums;

namespace SteamCloudFileManager.UI.ViewModels;

public class DialogViewModel : BindableBase, IDialogAware
{
    string message = string.Empty;
    string title = "";
    DialogType dialogType;
    
    int maxHeight;
    int maxWidth;

    public DialogViewModel()
    {
        MaxHeight = 800;
        MaxWidth = 600;
    }

    public event Action<IDialogResult>? RequestClose;

    public string Title { get => title; set => SetProperty(ref title, value); }
    public string Message { get => message; set => SetProperty(ref message, value); }
    public DialogType DialogType { get => dialogType; set => SetProperty(ref dialogType, value); }
    
    public int MaxHeight { get => maxHeight; set => SetProperty(ref maxHeight, value); }
    public int MaxWidth { get => maxWidth; set => SetProperty(ref maxWidth, value); }

    public DelegateCommand<string> OkCommand => new((param) =>
    {
        var result = int.TryParse(param, out var intResult) && Enum.IsDefined(typeof(ButtonResult), intResult)
            ? (ButtonResult)intResult
            : ButtonResult.None;

        RaiseRequestClose(new DialogResult(result));
    });

    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        var titleParameter = parameters.GetValue<string>("title");
        if (!string.IsNullOrEmpty(titleParameter))
            Title = titleParameter;

        Message = parameters.GetValue<string>("message");

        DialogType = parameters.TryGetValue("dialogType", out DialogType dialogTypeParameter)
            ? dialogTypeParameter
            : DialogType.Info;
    }

    public virtual void RaiseRequestClose(IDialogResult dialogResult)
    {
        RequestClose?.Invoke(dialogResult);
    }
}