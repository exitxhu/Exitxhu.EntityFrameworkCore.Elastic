using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Exitxhu.EntityFrameworkCore.Elastic.Infrastructure;

public class ElasticDbContextServices : IDbContextServices
{
    public ICurrentDbContext CurrentContext => throw new NotImplementedException();

    public IModel Model => throw new NotImplementedException();

    public IModel DesignTimeModel => throw new NotImplementedException();

    public DbContextOptions ContextOptions => throw new NotImplementedException();

    public IServiceProvider InternalServiceProvider => throw new NotImplementedException();

    public IDbContextServices Initialize(IServiceProvider scopedProvider, DbContextOptions contextOptions, DbContext context)
    {
        throw new NotImplementedException();
    }
}