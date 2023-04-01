using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        private static void Internal_Build_User_Entities(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.UserId);
            });
        }
    }
}
