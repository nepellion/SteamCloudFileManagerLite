using Steamworks;

namespace SteamCloudFileManager.Lib
{
    public interface IRemoteFile
    {
        string Name { get; }
        RemoteFileSize Size { get; }
        ERemoteStoragePlatform SyncPlatforms { get; set; }
        long Timestamp { get; }
        DateTime LastModified { get; }
        bool IsPersisted { get; }
        bool Exists { get; }
        
        bool Delete();
        bool Forget();
        int Read(byte[] buffer, int count);
        byte[] ReadAllBytes();
        bool Write(byte[] buffer, int count);
        bool WriteAllBytes(byte[] buffer);
    }
}
