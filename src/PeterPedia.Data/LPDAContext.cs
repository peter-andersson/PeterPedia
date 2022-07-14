using Microsoft.EntityFrameworkCore;
using PeterPedia.Data.Models;

namespace PeterPedia.Data;

public class PeterPediaContext : DbContext
{
    public PeterPediaContext(DbContextOptions<PeterPediaContext> options)
        : base(options)
    {
    }

    public DbSet<ArticleEF> Articles { get; set; } = null!;

    public DbSet<EpisodeEF> Episodes { get; set; } = null!;

    public DbSet<LinkEF> Links { get; set; } = null!;

    public DbSet<SeasonEF> Seasons { get; set; } = null!;

    public DbSet<ShowEF> Shows { get; set; } = null!;

    public DbSet<SubscriptionEF> Subscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.Entity<ArticleEF>();
        modelBuilder.Entity<EpisodeEF>();
        modelBuilder.Entity<LinkEF>();
        modelBuilder.Entity<SeasonEF>();
        modelBuilder.Entity<ShowEF>();
        modelBuilder.Entity<SubscriptionEF>();
    }
}
