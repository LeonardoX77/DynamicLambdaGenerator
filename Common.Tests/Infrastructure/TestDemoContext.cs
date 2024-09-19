using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Common.Core.Data.Context;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;

namespace Common.Tests.Infrastructure
{
    internal class TestDemoContext : AppDbContext
    {
        public TestDemoContext(DbContextOptions dbContextOptions, IOptions<DynamicFiltersConfiguration> options) : base(dbContextOptions, options)
        {
        }
    }
}
