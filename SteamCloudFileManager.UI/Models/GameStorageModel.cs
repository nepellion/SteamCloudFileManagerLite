using SteamCloudFileManager.Lib;

namespace SteamCloudFileManager.UI.Models;

public class GameStorageModel : IGameStorageModel
{
    RemoteStorage? Current { get; set; }
    
    IRemoteStorage? IGameStorageModel.Current => Current;
    
    public void SelectAppId(uint appId)
    {
        Current?.Dispose();
        
        Current = RemoteStorage.CreateInstance(appId);
    }
}