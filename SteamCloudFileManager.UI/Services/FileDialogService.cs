using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Prism.DryIoc;

namespace SteamCloudFileManager.UI.Services;

public class FileDialogService : IFileDialogService
{
    static IStorageProvider StorageProvider
        => (Application.Current as PrismApplication)?.MainWindow switch
        {
            Window window => window.StorageProvider,
            UserControl userControl => TopLevel.GetTopLevel(userControl)?.StorageProvider
                                       ?? throw new InvalidOperationException("Tried to access storage provider from a disconnected user control."),
            _ => throw new InvalidOperationException("Avalonia application isn't running?!")
        };

    public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(string title)
        => StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title
        });
    
    public Task<IStorageFile?> SaveFilePickerAsync(string title, string fileName)
        => StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = fileName
        });
    
}