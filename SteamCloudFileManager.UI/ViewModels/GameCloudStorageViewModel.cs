using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using SteamCloudFileManager.Lib;
using SteamCloudFileManager.UI.Commands;
using SteamCloudFileManager.UI.Enums;
using SteamCloudFileManager.UI.Models;
using SteamCloudFileManager.UI.Services;
using SteamCloudFileManager.UI.Views;

namespace SteamCloudFileManager.UI.ViewModels;

public class GameCloudStorageViewModel : ViewModelBase
{
    readonly IRegionManager regionManager;
    readonly IGameStorageModel gameStorageModel;
    readonly IFileDialogService fileDialogService;
    readonly IDialogService dialogService;

    IRemoteFile? selectedFile;
    RemoteFileSize total = new(0);
    RemoteFileSize available = new(0);
    RemoteFileSize used = new(0);

    public ICommand RefreshFilesCommand { get; }
    public ICommand DownloadFileCommand { get; }
    public ICommand DeleteFileCommand { get; }
    public ICommand UploadFileCommand { get; }
    public ICommand DeleteAllFilesCommand { get; }
    public ICommand ReturnToGameSelectionCommand { get; }

    public IRemoteFile? SelectedFile
    {
        get => selectedFile;
        set => SetProperty(ref selectedFile, value);
    }

    public RemoteFileSize Total
    {
        get => total;
        set => SetProperty(ref total, value);
    }

    public RemoteFileSize Used
    {
        get => used;
        set => SetProperty(ref used, value);
    }

    public RemoteFileSize Available
    {
        get => available;
        set => SetProperty(ref available, value);
    }

    public ObservableCollection<IRemoteFile> Files { get; } = [];

    public GameCloudStorageViewModel(
        IRegionManager regionManager, 
        IGameStorageModel gameStorageModel,
        IFileDialogService fileDialogService, 
        IDialogService dialogService)
    {
        this.regionManager = regionManager;
        this.gameStorageModel = gameStorageModel;
        this.fileDialogService = fileDialogService;
        this.dialogService = dialogService;

        RefreshFilesCommand = new DelegateCommand(RefreshFiles);
        DownloadFileCommand = new BlockingDelegateCommand(DownloadFileAsync);
        UploadFileCommand = new BlockingDelegateCommand(UploadFileAsync);
        DeleteFileCommand = new DelegateCommand(DeleteFile);
        DeleteAllFilesCommand = new DelegateCommand(DeleteAllFiles);
        ReturnToGameSelectionCommand = new DelegateCommand(() =>
        {
            regionManager.RequestNavigate("content", nameof(GameSelectionView));
        }, OperatingSystem.IsWindows);
    }


    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (gameStorageModel.Current is null)
        {
            regionManager.RequestNavigate("content", nameof(GameSelectionView));
            return;
        }

        SelectedFile = default;
        Files.Clear();
        Total = new RemoteFileSize(0);
        Available = new RemoteFileSize(0);
        Used = new RemoteFileSize(0);
        
        UpdateQuota();

        List<IRemoteFile>? files = gameStorageModel.Current?.GetFiles();

        Files.AddRange(files);
    }

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        SelectedFile = default;
        Files.Clear();
        Total = new RemoteFileSize(0);
        Available = new RemoteFileSize(0);
        Used = new RemoteFileSize(0);
    }

    void RefreshFiles()
    {
        Files.Clear();

        List<IRemoteFile>? files = gameStorageModel.Current?.GetFiles();

        UpdateQuota();

        Files.AddRange(files);
    }

    async Task DownloadFileAsync()
    {
        if (SelectedFile is null)
            return;

        var fileName = Path.GetFileName(SelectedFile.Name);

        var downloadFile = await fileDialogService.SaveFilePickerAsync("Select download location", fileName);

        if (downloadFile is null)
        {
            ShowDialog("File download error", "Could not download file to specified location.", DialogType.Error);
            return;
        }

        await using var stream = await downloadFile.OpenWriteAsync();

        var remoteFileContent = SelectedFile.ReadAllBytes();

        await stream.WriteAsync(remoteFileContent);

        await stream.FlushAsync();

        ShowDialog("File downloaded",
            $"file has been successfully downloaded as {Path.Combine(downloadFile.Path.AbsolutePath, downloadFile.Name)}",
            DialogType.Info);
    }

    async Task UploadFileAsync()
    {
        IReadOnlyList<IStorageFile> fileList = await fileDialogService.OpenFilePickerAsync("Select file to upload");

        if (fileList.Count != 1)
        {
            ShowDialog("File not selected", "Please select a file to upload and try again.", DialogType.Warn);
            return;
        }

        var fileToUpload = fileList[0];
        var fileInfo = await fileToUpload.GetBasicPropertiesAsync();
        var fileSize = fileInfo.Size;

        if (!fileSize.HasValue)
        {
            ShowDialog("File upload validation error",
                "Size of selected file could not be determined, cancelling upload.", DialogType.Error);
            return;
        }

        await using var stream = await fileToUpload.OpenReadAsync();

        var memory = new Memory<byte>();

        var bytesRead = await stream.ReadAsync(memory);

        if (bytesRead < 0 || (ulong)bytesRead != fileSize.Value)
        {
            ShowDialog("File upload error", "File could not be uploaded: Out of memory!", DialogType.Error);
            return;
        }

        gameStorageModel.Current?.UploadFile(fileToUpload.Name, memory.ToArray());

        ShowDialog("File uploaded", "File has been successfully uploaded.", DialogType.Info);

        UpdateQuota();
    }

    void DeleteFile()
    {
        if (SelectedFile is null)
            return;

        SelectedFile.Delete();
        
        Files.Remove(SelectedFile);

        ShowDialog("File deleted", "File has been successfully deleted.", DialogType.Info);

        UpdateQuota();
    }

    void DeleteAllFiles()
    {
        foreach (var file in Files)
            file.Delete();

        Files.Clear();
        
        ShowDialog("All files deleted", "All files have been successfully deleted.", DialogType.Info);

        UpdateQuota();
    }

    void UpdateQuota()
    {
        if (gameStorageModel.Current is null)
            return;

        gameStorageModel.Current.GetQuota(out var totalBytes, out var availableBytes);
        var usedBytes = totalBytes - availableBytes;

        Total = new RemoteFileSize(totalBytes);
        Available = new RemoteFileSize(availableBytes);
        Used = new RemoteFileSize(usedBytes);
    }

    void ShowDialog(string title, string message, DialogType type)
        => dialogService.ShowDialog(nameof(DialogView),
            new DialogParameters($"title={title}&message={message}&type={type}"));
}