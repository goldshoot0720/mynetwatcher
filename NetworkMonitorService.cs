using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NetWatcher.App;

public sealed class NetworkMonitorService
{
    private readonly EtwProcessMonitorService _processMonitorService = new();
    private long _lastTotalBytesReceived;
    private long _lastTotalBytesSent;
    private DateTimeOffset? _lastSampleTime;

    public Task<NetworkSnapshot> CaptureAsync()
    {
        return Task.Run(Capture);
    }

    private NetworkSnapshot Capture()
    {
        var now = DateTimeOffset.UtcNow;
        var intervalSeconds = Math.Max(1e-6, (now - (_lastSampleTime ?? now.AddSeconds(-1))).TotalSeconds);
        _lastSampleTime = now;

        var totalBytesReceived = 0L;
        var totalBytesSent = 0L;

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            var stats = nic.GetIPStatistics();
            totalBytesReceived += stats.BytesReceived;
            totalBytesSent += stats.BytesSent;
        }

        var totalDownload = _lastTotalBytesReceived == 0
            ? 0
            : Math.Max(0, totalBytesReceived - _lastTotalBytesReceived) / intervalSeconds;
        var totalUpload = _lastTotalBytesSent == 0
            ? 0
            : Math.Max(0, totalBytesSent - _lastTotalBytesSent) / intervalSeconds;

        _lastTotalBytesReceived = totalBytesReceived;
        _lastTotalBytesSent = totalBytesSent;

        var processSnapshot = _processMonitorService.CollectSnapshot(intervalSeconds);
        var processStatusMessage = BuildProcessStatusMessage(processSnapshot);
        return new NetworkSnapshot(totalDownload, totalUpload, processSnapshot.Processes, processStatusMessage);
    }

    private static string BuildProcessStatusMessage(ProcessMonitorSnapshot snapshot)
    {
        if (snapshot.Processes.Count > 0)
        {
            return $"已顯示 {snapshot.Processes.Count} 個有流量的程式";
        }

        if (!snapshot.IsRunning)
        {
            return snapshot.StatusMessage;
        }

        return "ETW 已啟用，目前沒有偵測到單一程式流量。";
    }

    public void Dispose()
    {
        _processMonitorService.Dispose();
    }
}
