using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<TimeKey> TimeKeys { get; set; }

        private static void Internal_Build_TimeKey_Entities(ModelBuilder builder)
        {
            builder.Entity<TimeKey>(entity =>
            {
                entity.HasKey(x => x.Key);
            });
        }
    }
}
