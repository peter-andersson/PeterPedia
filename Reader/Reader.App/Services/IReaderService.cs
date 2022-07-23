using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.App.Services;

public interface IReaderService
{
    Task<UnreadGroup[]> UnreadArticles();
}
