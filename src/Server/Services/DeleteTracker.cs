using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Services;

public interface IDeleteTracker
{
    Task<IList<DeleteLog>> DeletedSinceAsync(DeleteType type, DateTime since);

    Task TrackAsync(DeleteType type, int id);
}

public class DeleteTracker : IDeleteTracker
{
    private readonly PeterPediaContext _dbContext;

    public DeleteTracker(PeterPediaContext dbContext) => _dbContext = dbContext;

    public async Task<IList<DeleteLog>> DeletedSinceAsync(DeleteType type, DateTime since)
    {
        List<DeleteLogEF>? deletions = await _dbContext.DeleteLog
            .Where(d => d.Type == type && d.Deleted > since)
            .OrderBy(d => d.Deleted)
            .ToListAsync();

        var result = new List<DeleteLog>(deletions.Count);
        foreach (DeleteLogEF? deleteLog in deletions)
        {
            result.Add(ConvertToDeleteLog(deleteLog));
        }

        return result;
    }

    public async Task TrackAsync(DeleteType type, int id)
    {
        _dbContext.DeleteLog.Add(new DeleteLogEF()
        {
            DataId = id,
            Deleted = DateTime.UtcNow,
            Type = type,
        });

        await _dbContext.SaveChangesAsync();
    }

    private static DeleteLog ConvertToDeleteLog(DeleteLogEF deleteEF)
    {
        return new DeleteLog()
        {
            DataId = deleteEF.DataId,
            Deleted = deleteEF.Deleted,
        };
    }
}
