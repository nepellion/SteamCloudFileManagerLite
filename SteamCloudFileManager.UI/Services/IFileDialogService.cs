using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace SteamCloudFileManager.UI.Services;

public interface IFileDialogService
{
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(string title);
    Task<IStorageFile?> SaveFilePickerAsync(string title, string fileName);
}