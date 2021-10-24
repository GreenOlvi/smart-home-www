using Microsoft.EntityFrameworkCore;
using SmartHomeCore.Infrastructure;
using System;

namespace SmartHomeWWWTests
{
    public static class DbContextHelper
    {
        private static SmartHomeDbContext GetMemoryContext()
        {
            var opts = new DbContextOptionsBuilder<SmartHomeDbContext>().UseSqlite("Data Source=:memory:").Options;
            return new SmartHomeDbContext(opts);
        }

        private static readonly Lazy<SmartHomeDbContext> _memoryContext = new(GetMemoryContext);
        public static SmartHomeDbContext MemoryContext => _memoryContext.Value;
    }
}
