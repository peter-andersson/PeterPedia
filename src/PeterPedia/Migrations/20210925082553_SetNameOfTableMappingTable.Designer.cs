// <auto-generated />
using System;
using PeterPedia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Migrations;

[DbContext(typeof(PeterPediaContext))]
[Migration("20210925082553_SetNameOfTableMappingTable")]
partial class SetNameOfTableMappingTable
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "5.0.10");

        modelBuilder.Entity("AuthorEFBookEF", b =>
            {
                b.Property<int>("AuthorsId")
                    .HasColumnType("INTEGER");

                b.Property<int>("BooksId")
                    .HasColumnType("INTEGER");

                b.HasKey("AuthorsId", "BooksId");

                b.HasIndex("BooksId");

                b.ToTable("authorbook");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.ArticleEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Content")
                    .IsRequired()
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("PublishDate")
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("ReadDate")
                    .HasColumnType("TEXT");

                b.Property<int>("SubscriptionId")
                    .HasColumnType("INTEGER");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<string>("Url")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("SubscriptionId");

                b.ToTable("article");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.AuthorEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("author");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.BookEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("State")
                    .HasColumnType("INTEGER");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("book");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.EpisodeEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime?>("AirDate")
                    .HasColumnType("TEXT");

                b.Property<int>("EpisodeNumber")
                    .HasColumnType("INTEGER");

                b.Property<int>("SeasonId")
                    .HasColumnType("INTEGER");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<bool>("Watched")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("SeasonId");

                b.ToTable("episode");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.MovieEF", b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("INTEGER");

                b.Property<string>("ImdbId")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("OriginalLanguage")
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnType("TEXT");

                b.Property<string>("OriginalTitle")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("ReleaseDate")
                    .HasColumnType("TEXT");

                b.Property<int?>("RunTime")
                    .HasColumnType("INTEGER");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("WatchedDate")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("movie");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.SeasonEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("SeasonNumber")
                    .HasColumnType("INTEGER");

                b.Property<int>("ShowId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("ShowId");

                b.ToTable("season");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.ShowEF", b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("INTEGER");

                b.Property<string>("ETag")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<DateTime>("LastUpdate")
                    .HasColumnType("TEXT");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("show");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.SubscriptionEF", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<byte[]>("Hash")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("BLOB");

                b.Property<DateTime>("LastUpdate")
                    .HasColumnType("TEXT");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<int>("UpdateIntervalMinute")
                    .HasColumnType("INTEGER");

                b.Property<string>("Url")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("subscription");
            });

        modelBuilder.Entity("AuthorEFBookEF", b =>
            {
                b.HasOne("PeterPedia.Server.Data.Models.AuthorEF", null)
                    .WithMany()
                    .HasForeignKey("AuthorsId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("PeterPedia.Server.Data.Models.BookEF", null)
                    .WithMany()
                    .HasForeignKey("BooksId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.ArticleEF", b =>
            {
                b.HasOne("PeterPedia.Server.Data.Models.SubscriptionEF", "Subscription")
                    .WithMany("Articles")
                    .HasForeignKey("SubscriptionId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Subscription");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.EpisodeEF", b =>
            {
                b.HasOne("PeterPedia.Server.Data.Models.SeasonEF", "Season")
                    .WithMany("Episodes")
                    .HasForeignKey("SeasonId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Season");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.SeasonEF", b =>
            {
                b.HasOne("PeterPedia.Server.Data.Models.ShowEF", "Show")
                    .WithMany("Seasons")
                    .HasForeignKey("ShowId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Show");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.SeasonEF", b =>
            {
                b.Navigation("Episodes");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.ShowEF", b =>
            {
                b.Navigation("Seasons");
            });

        modelBuilder.Entity("PeterPedia.Server.Data.Models.SubscriptionEF", b =>
            {
                b.Navigation("Articles");
            });
#pragma warning restore 612, 618
    }
}
