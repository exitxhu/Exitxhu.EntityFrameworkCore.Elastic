using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exitxhu.EntityFrameworkCore.Elastic.Infrastructure;

public class ElasticDbContextOptionsExtension : IDbContextOptionsExtension
{
    public DbContextOptionsExtensionInfo Info => throw new NotImplementedException();

    public void ApplyServices(IServiceCollection services)
    {
        throw new NotImplementedException();
    }

    public void Validate(IDbContextOptions options)
    {
        throw new NotImplementedException();
    }
}
