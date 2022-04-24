using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Server.Data;

public static class DbInitializer
{
    public static void Initialize(PeterPediaContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Database.Migrate();

        var command = "UPDATE book SET LastUpdated = '2000-01-01' WHERE LastUpdated = 'LastUpdated'";            
        context.Database.ExecuteSqlRaw(command);
    }
}
