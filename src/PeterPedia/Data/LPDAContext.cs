using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Data;

public class PeterPediaContext : DbContext
{
    public PeterPediaContext(DbContextOptions<PeterPediaContext> options)
        : base(options)
    {
    }

    public DbSet<AlbumEF> Albums { get; set; } = null!;

    public DbSet<ArticleEF> Articles { get; set; } = null!;

    public DbSet<AuthorEF> Authors { get; set; } = null!;

    public DbSet<BookEF> Books { get; set; } = null!;

    public DbSet<DeleteLogEF> DeleteLog { get; set; } = null!;

    public DbSet<EpisodeEF> Episodes { get; set; } = null!;

    public DbSet<LinkEF> Links { get; set; } = null!;

    public DbSet<MovieEF> Movies { get; set; } = null!;

    public DbSet<PhotoEF> Photos { get; set; } = null!;

    public DbSet<SeasonEF> Seasons { get; set; } = null!;

    public DbSet<ShowEF> Shows { get; set; } = null!;

    public DbSet<SubscriptionEF> Subscriptions { get; set; } = null!;

    public DbSet<VideoEF> Videos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.Entity<AuthorEF>();
        modelBuilder.Entity<BookEF>();
        modelBuilder.Entity<DeleteLogEF>();
        modelBuilder.Entity<MovieEF>();
        modelBuilder.Entity<SubscriptionEF>();
        modelBuilder.Entity<ArticleEF>();
        modelBuilder.Entity<ShowEF>();
        modelBuilder.Entity<SeasonEF>();
        modelBuilder.Entity<EpisodeEF>();
        modelBuilder.Entity<VideoEF>();
        modelBuilder.Entity<LinkEF>();

        modelBuilder
            .Entity<BookEF>()
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(e => e.ToTable("authorbook"));
    }
}
