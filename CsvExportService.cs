using System.Globalization;
using System.Text;

namespace NetWatcher.App;

public sealed class CsvExportService
{
    private readonly string _exportDirectory;

    public CsvExportService(string baseDirectory)
    {
        _exportDirectory = Path.Combine(baseDirectory, "exports");
    }

    public async Task<string> ExportTrafficHistoryAsync(IReadOnlyList<TrafficLogEntry> entries)
    {
        Directory.CreateDirectory(_exportDirectory);

        var fileName = $"traffic-log-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
        var filePath = Path.Combine(_exportDirectory, fileName);

        var builder = new StringBuilder();
        builder.AppendLine("Timestamp,DownloadBytesPerSecond,UploadBytesPerSecond,DownloadText,UploadText");

        foreach (var entry in entries)
        {
            builder
                .Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(',')
                .Append(entry.TotalDownloadBytesPerSecond.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                .Append(entry.TotalUploadBytesPerSecond.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                .Append(TrafficFormatter.FormatBytesPerSecond(entry.TotalDownloadBytesPerSecond)).Append(',')
                .Append(TrafficFormatter.FormatBytesPerSecond(entry.TotalUploadBytesPerSecond))
                .AppendLine();
        }

        await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8);
        return filePath;
    }
}
