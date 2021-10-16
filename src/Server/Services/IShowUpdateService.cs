using System.Threading;
using System.Threading.Tasks;

namespace PeterPedia.Server.Services
{
    internal interface IShowUpdateService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
}