using Microsoft.AspNetCore.Components;

namespace PeterPedia.Pages;

public partial class LogPage : ComponentBase
{
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    public List<LogFile> Files { get; set; } = new();

    protected override void OnInitialized()
    {
        var path = Configuration["LogsPath"];

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(path, file);

            var logFile = new LogFile()
            {
                FileName = Path.GetFileName(file),
                Url = "/logs/" + relativePath.Replace('\\', '/')
            };

            Files.Add(logFile);
        }    
    }
}
