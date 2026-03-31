namespace NetWatcher.App;

public sealed record ProcessTrafficSnapshot(
    int ProcessId,
    string ProcessName,
    string Description,
    double DownloadBytesPerSecond,
    double UploadBytesPerSecond);

public sealed record NetworkSnapshot(
    double TotalDownloadBytesPerSecond,
    double TotalUploadBytesPerSecond,
    IReadOnlyList<ProcessTrafficSnapshot> Processes,
    string ProcessStatusMessage);

public sealed record TrafficLogEntry(
    DateTime Timestamp,
    double TotalDownloadBytesPerSecond,
    double TotalUploadBytesPerSecond);
