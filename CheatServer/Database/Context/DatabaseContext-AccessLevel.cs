using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<AccessLevel> AccessLevels { get; set; }

        private static void Internal_Build_AccessLevel_Entities(ModelBuilder builder)
        {
            builder.Entity<AccessLevel>(entity =>
            {
                entity.HasKey(x => x.AccessLevelId);
            });
        }
    }
}
