namespace NetWatcher.App;

public sealed class ProcessTrafficViewModel : ObservableObject
{
    private string _processName = string.Empty;
    private string _description = string.Empty;
    private string _downloadSpeedText = "0 B/s";
    private string _uploadSpeedText = "0 B/s";
    private int _processId;
    private double _downloadBytesPerSecond;
    private double _uploadBytesPerSecond;

    public string ProcessName
    {
        get => _processName;
        set => SetProperty(ref _processName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string DownloadSpeedText
    {
        get => _downloadSpeedText;
        set => SetProperty(ref _downloadSpeedText, value);
    }

    public string UploadSpeedText
    {
        get => _uploadSpeedText;
        set => SetProperty(ref _uploadSpeedText, value);
    }

    public int ProcessId
    {
        get => _processId;
        set => SetProperty(ref _processId, value);
    }

    public double DownloadBytesPerSecond
    {
        get => _downloadBytesPerSecond;
        set => SetProperty(ref _downloadBytesPerSecond, value);
    }

    public double UploadBytesPerSecond
    {
        get => _uploadBytesPerSecond;
        set => SetProperty(ref _uploadBytesPerSecond, value);
    }

    public double TotalBytesPerSecond => DownloadBytesPerSecond + UploadBytesPerSecond;
}
