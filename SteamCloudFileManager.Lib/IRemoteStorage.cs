namespace SteamCloudFileManager.Lib
{
    public interface IRemoteStorage
    {
        IRemoteFile GetFile(string name);
        List<IRemoteFile> GetFiles();
        bool GetQuota(out ulong totalBytes, out ulong availableBytes);
        bool IsCloudEnabledForAccount { get; }
        bool IsCloudEnabledForApp { get; set; }
        void UploadFile(string filePath);
        void UploadFile(string fileName, byte[] data);
    }
}
