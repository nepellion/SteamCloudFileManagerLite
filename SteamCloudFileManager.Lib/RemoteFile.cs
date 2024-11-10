using Steamworks;

namespace SteamCloudFileManager.Lib
{
    public class RemoteFile : IRemoteFile
    {
        static readonly DateTime UnixEpoch = new(1970,
            1,
            1,
            0,
            0,
            0,
            0,
            DateTimeKind.Utc);

        RemoteStorage parent;

        bool? exists,
            isPersisted;

        long? timestamp;
        RemoteFileSize? size;

        public string Name { get; private set; }

        public bool Exists =>
            exists ??= ValidateParentNotDisposed(() => SteamRemoteStorage.FileExists(Name));

        public bool IsPersisted =>
            isPersisted ??= ValidateParentNotDisposed(() => SteamRemoteStorage.FilePersisted(Name));

        public RemoteFileSize Size =>
            size ??= ValidateParentNotDisposed(() => new RemoteFileSize((ulong)SteamRemoteStorage.GetFileSize(Name)));

        public long Timestamp =>
            timestamp ??= ValidateParentNotDisposed(() => SteamRemoteStorage.GetFileTimestamp(Name));

        public DateTime LastModified => UnixEpoch.AddSeconds(Timestamp).ToLocalTime();

        public ERemoteStoragePlatform SyncPlatforms
        {
            get => ValidateParentNotDisposed(() => SteamRemoteStorage.GetSyncPlatforms(Name));
            set => ValidateParentNotDisposed(() => SteamRemoteStorage.SetSyncPlatforms(Name, value));
        }

        internal RemoteFile(RemoteStorage parent, string name)
        {
            this.parent = parent;
            Name = name;
        }

        public int Read(byte[] buffer, int count)
            => ValidateParentNotDisposed(() => SteamRemoteStorage.FileRead(Name, buffer, count));

        public byte[] ReadAllBytes()
        {
            var buffer = new byte[Size.Bytes];
            var read = Read(buffer, buffer.Length);

            if (read != buffer.Length)
                throw new IOException("Could not read entire file.");

            return buffer;
        }

        public bool Write(byte[] buffer, int count)
            => ValidateParentNotDisposed(() => SteamRemoteStorage.FileWrite(Name, buffer, count));

        public bool WriteAllBytes(byte[] buffer)
            => Write(buffer, buffer.Length);

        // Todo: implement async write

        public bool Forget()
            => ValidateParentNotDisposed(() => SteamRemoteStorage.FileForget(Name));

        public bool Delete()
            => ValidateParentNotDisposed(() => SteamRemoteStorage.FileDelete(Name));

        T ValidateParentNotDisposed<T>(Func<T> getValue)
        {
            if (parent.IsDisposed)
                throw new InvalidOperationException("Instance is no longer valid.");
            
            return getValue();
        }
    }
}