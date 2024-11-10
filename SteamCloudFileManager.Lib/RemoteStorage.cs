using System.Diagnostics;
using Steamworks;

namespace SteamCloudFileManager.Lib
{
    public class RemoteStorage : IRemoteStorage, IDisposable
    {
        static RemoteStorage? instance;
        static object sync = new object();

        internal bool IsDisposed { get; private set; }

        public bool IsCloudEnabledForAccount
        {
            get
            {
                //checkDisposed();
                // Not static because we need to ensure Steamworks API is initted
                return SteamRemoteStorage.IsCloudEnabledForAccount();
            }
        }
        public bool IsCloudEnabledForApp
        {
            get
            {
                CheckDisposed();
                return SteamRemoteStorage.IsCloudEnabledForApp();
            }
            set
            {
                CheckDisposed();
                SteamRemoteStorage.SetCloudEnabledForApp(value);
            }
        }

        public void UploadFile(string filePath)
            => UploadFile(Path.GetFileName(filePath), File.ReadAllBytes(filePath));
        
        public void UploadFile(string fileName, byte[] data) 
            => SteamRemoteStorage.FileWrite(fileName, data, data.Length);

        RemoteStorage(uint appID)
        {
            Environment.SetEnvironmentVariable("SteamAppID", appID.ToString());
            
            bool init = SteamAPI.Init();
            if (!init)
            {
                // Setting environment variable didn't work, so use steam_appid.txt instead
                try
                {
                    File.WriteAllText("steam_appid.txt", appID.ToString());
                    init = SteamAPI.Init();
                    File.Delete("steam_appid.txt");
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Error initializing using a steam_appid.txt file: {ex.Message}");
                }
            }

            if (!init) throw new Exception("Cannot initialize Steamworks API.");
        }

        public List<IRemoteFile> GetFiles()
        {
            CheckDisposed();
            List<IRemoteFile> files = new List<IRemoteFile>();
            
            int fileCount = SteamRemoteStorage.GetFileCount();
            for (int i = 0; i < fileCount; ++i)
            {
                int length;
                string name = SteamRemoteStorage.GetFileNameAndSize(i, out length);
                RemoteFile file = new RemoteFile(this, name);
                files.Add(file);
            }

            return files;
        }

        public IRemoteFile GetFile(string name)
        {
            CheckDisposed();
            return new RemoteFile(this, name.ToLowerInvariant());
        }

        public bool GetQuota(out ulong totalBytes, out ulong availableBytes)
        {
            CheckDisposed();
            return SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
        }

        void CheckDisposed()
        {
            if (IsDisposed) throw new InvalidOperationException("Instance is no longer valid.");
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                SteamAPI.Shutdown();
                IsDisposed = true;
            }
        }

        public static RemoteStorage CreateInstance(uint appID)
        {
            lock (sync)
            {
                if (instance != null)
                {
                    instance.Dispose();
                    instance = null;
                }

                RemoteStorage rs = new RemoteStorage(appID);
                instance = rs;
                return rs;
            }
        }
    }
}
