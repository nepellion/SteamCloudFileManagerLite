using SteamCloudFileManager.Lib;

namespace SteamCloudFileManager.UI.Models;

public interface IGameStorageModel
{
    IRemoteStorage? Current { get; }
    
    void SelectAppId(uint appId);
}