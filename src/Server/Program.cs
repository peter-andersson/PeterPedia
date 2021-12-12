using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeterPedia.Server.Data;
using PeterPedia.Server.Jobs;
using Quartz;

namespace PeterPedia.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
                                        .ConfigureLogging((hostingContext, logging) =>
                                        {
                                            logging.ClearProviders();
                                            logging.AddConsole();

                                            var configuration = hostingContext.Configuration.GetSection("Logging");
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
                    });

                    services.AddQuartzHostedService(
                        q => q.WaitForJobsToComplete = true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var lpdaContext = services.GetRequiredService<PeterPediaContext>();
                DbInitializer.Initialize(lpdaContext);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");

                throw;
            }
        }        
    }
}
