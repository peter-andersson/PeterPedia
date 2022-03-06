using PeterPedia.Server.Jobs;
using Quartz;

namespace PeterPedia.Server;

public class Program
{
    public static void Main(string[] args)
    {
        IHost? host = CreateHostBuilder(args)
                                    .ConfigureLogging((hostingContext, logging) =>
                                    {
                                        logging.ClearProviders();
                                        logging.AddConsole();

                                        IConfigurationSection? configuration = hostingContext.Configuration.GetSection("Logging");
                                        logging.AddFile(configuration);
                                    })
                                    .Build();

        CreateDbIfNotExists(host);

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return 
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();

                    q.AddJobAndTrigger<RemoveArticleJob>(hostContext.Configuration);
                    q.AddJobAndTrigger<RefreshArticleJob>(hostContext.Configuration);
                    q.AddJobAndTrigger<VideoJob>(hostContext.Configuration);
                    q.AddJobAndTrigger<ShowUpdateJob>(hostContext.Configuration);
                    q.AddJobAndTrigger<PhotoJob>(hostContext.Configuration);
                });

                services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }

    private static void CreateDbIfNotExists(IHost host)
    {
        using IServiceScope? scope = host.Services.CreateScope();
        IServiceProvider? services = scope.ServiceProvider;

        try
        {
            PeterPediaContext? lpdaContext = services.GetRequiredService<PeterPediaContext>();
            DbInitializer.Initialize(lpdaContext);
        }
        catch (Exception ex)
        {
            ILogger<Program>? logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");

            throw;
        }
    }        
}
