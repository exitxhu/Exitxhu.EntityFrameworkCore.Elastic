using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exitxhu.EntityFrameworkCore.Elastic.Storage;

public class ElasticDatabaseProvider : IDatabaseProvider
{
    public string Name => throw new NotImplementedException();

    public bool IsConfigured(IDbContextOptions options)
    {
        throw new NotImplementedException();
    }
}
