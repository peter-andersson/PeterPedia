namespace PeterPedia.Client.Services;

public class SyncService
{    
    private readonly IAuthorManager _authorManager;
    private readonly IBookManager _bookManager;
    private readonly IMovieManager _movieManager;
    private System.Timers.Timer _timer = null!;

    public SyncService(IAuthorManager authorManager, IBookManager bookManager, IMovieManager movieManager)
    {            
        _authorManager = authorManager;
        _bookManager = bookManager;
        _movieManager = movieManager;
    }

    public void Start()
    {
        // Every 5 minutes
        _timer = new System.Timers.Timer(5 * 60 * 1000);
        _timer.Elapsed += TimerElapsedAsync;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private async void TimerElapsedAsync(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await _authorManager.RefreshAsync();

        await _bookManager.RefreshAsync();

        await _movieManager.RefreshAsync();
    }
}
