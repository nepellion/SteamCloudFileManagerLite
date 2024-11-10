namespace SteamCloudFileManager.Lib;


public record RemoteFileSize(ulong Bytes)
{
    public string HumanReadable => Bytes switch
    {
        < 1024 => $"{Bytes} B",
        < 1024 * 1024 => $"{Math.Round(Bytes / 1024d, 1)} kB",
        _ => $"{Math.Round(Bytes / 1024d / 1024d, 1)} MB"
    };
}