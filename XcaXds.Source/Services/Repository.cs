using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XcaXds.Source.Services;

public class Repository : IRepository
{
    private readonly ApplicationConfig _appConfig;
    private readonly string _repositoryPath;
    public Repository(ApplicationConfig appConfig)
    {
        _appConfig = appConfig;
        _repositoryPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "XcaXds.Source", "Repository", _appConfig.RepositoryUniqueId);
    }
}
