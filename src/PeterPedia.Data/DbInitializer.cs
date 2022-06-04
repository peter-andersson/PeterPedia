using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Data;

public static class DbInitializer
{
    public static void Initialize(PeterPediaContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Database.Migrate();
    }
}
