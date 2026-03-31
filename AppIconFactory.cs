using System.IO;

namespace NetWatcher.App;

public static class AppIconFactory
{
    private const string IconBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABeSURBVDhPY/j24/d/SjADiOAQlCULYxiQc+A/BkbXhNMAdI3EGEI9A9A1oGN0jRgGUOwCqhiAyxCQ+P+VvhgYqwHYMLpGZEx7A9A1oGOCBhAyhD4G4DIEJA43gBIMAE9NgWsFQz8hAAAAAElFTkSuQmCC";

    public static Avalonia.Controls.WindowIcon CreateWindowIcon()
    {
        var bytes = Convert.FromBase64String(IconBase64);
        return new Avalonia.Controls.WindowIcon(new MemoryStream(bytes));
    }
}
