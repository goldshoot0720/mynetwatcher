using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace NetWatcher.App;

public sealed class EtwProcessMonitorService : IDisposable
{
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<int, ProcessIdentity> _processIdentityCache = new();
    private readonly Dictionary<int, ProcessCounter> _processCounters = new();
    private readonly string _sessionName = $"NetWatcher-Etw-{Environment.ProcessId}";

    private TraceEventSession? _session;
    private Task? _processingTask;
    private string _startupStatus = "正在啟動 ETW 監聽...";
    private bool _isRunning;
    private bool _isDisposed;

    public EtwProcessMonitorService()
    {
        Start();
    }

    public bool IsRunning => _isRunning;

    public string StartupStatus => _startupStatus;

    public ProcessMonitorSnapshot CollectSnapshot(double intervalSeconds)
    {
        lock (_sync)
        {
            var processes = _processCounters
                .Values
                .Select(counter =>
                {
                    var identity = _processIdentityCache.GetOrAdd(counter.ProcessId, ResolveIdentity);
                    return new ProcessTrafficSnapshot(
                        counter.ProcessId,
                        identity.ProcessName,
                        identity.Description,
                        counter.DownloadBytes / intervalSeconds,
                        counter.UploadBytes / intervalSeconds);
                })
                .ToList();

            _processCounters.Clear();
            return new ProcessMonitorSnapshot(processes, _startupStatus, _isRunning);
        }
    }

    private void Start()
    {
        if (!OperatingSystem.IsWindows())
        {
            _startupStatus = "目前只有 Windows 支援 ETW 單一程式流量監聽。";
            return;
        }

        try
        {
            _session = new TraceEventSession(_sessionName)
            {
                StopOnDispose = true
            };

            _session.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

            _session.Source.Kernel.TcpIpRecv += data => RecordDownload(data.ProcessID, GetPayloadSize(data));
            _session.Source.Kernel.TcpIpSend += data => RecordUpload(data.ProcessID, GetPayloadSize(data));
            _session.Source.Kernel.UdpIpRecv += data => RecordDownload(data.ProcessID, GetPayloadSize(data));
            _session.Source.Kernel.UdpIpSend += data => RecordUpload(data.ProcessID, GetPayloadSize(data));

            _processingTask = Task.Run(() =>
            {
                try
                {
                    _session.Source.Process();
                }
                catch (Exception ex) when (!_isDisposed)
                {
                    _startupStatus = $"ETW 監聽中斷：{ex.Message}";
                    _isRunning = false;
                }
            });

            _startupStatus = "ETW 單一程式流量監聽中";
            _isRunning = true;
        }
        catch (UnauthorizedAccessException)
        {
            _startupStatus = "ETW 需要系統管理員權限，請以系統管理員身分執行。";
            _isRunning = false;
        }
        catch (Exception ex)
        {
            _startupStatus = $"ETW 啟動失敗：{ex.Message}";
            _isRunning = false;
        }
    }

    private static long GetPayloadSize(dynamic data)
    {
        try
        {
            return Math.Max(0, (long)data.size);
        }
        catch
        {
            return 0;
        }
    }

    private void RecordDownload(int processId, long bytes)
    {
        if (processId <= 0 || bytes <= 0)
        {
            return;
        }

        lock (_sync)
        {
            if (!_processCounters.TryGetValue(processId, out var counter))
            {
                counter = new ProcessCounter(processId);
                _processCounters.Add(processId, counter);
            }

            counter.DownloadBytes += bytes;
        }
    }

    private void RecordUpload(int processId, long bytes)
    {
        if (processId <= 0 || bytes <= 0)
        {
            return;
        }

        lock (_sync)
        {
            if (!_processCounters.TryGetValue(processId, out var counter))
            {
                counter = new ProcessCounter(processId);
                _processCounters.Add(processId, counter);
            }

            counter.UploadBytes += bytes;
        }
    }

    private static ProcessIdentity ResolveIdentity(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return new ProcessIdentity(
                process.ProcessName,
                process.MainModule?.FileName ?? "系統或受保護行程");
        }
        catch
        {
            return new ProcessIdentity("Unknown", "無法取得程式資訊");
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        _isRunning = false;

        try
        {
            _session?.Dispose();
        }
        catch
        {
            // Ignore ETW session cleanup issues during shutdown.
        }

        _session = null;
    }

    private sealed class ProcessCounter
    {
        public ProcessCounter(int processId)
        {
            ProcessId = processId;
        }

        public int ProcessId { get; }

        public long DownloadBytes { get; set; }

        public long UploadBytes { get; set; }
    }

    private sealed record ProcessIdentity(string ProcessName, string Description);
}

public sealed record ProcessMonitorSnapshot(
    IReadOnlyList<ProcessTrafficSnapshot> Processes,
    string StatusMessage,
    bool IsRunning);
