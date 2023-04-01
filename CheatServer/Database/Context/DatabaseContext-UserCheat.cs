using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<UserCheat> UserCheats { get; set; }

        private static void Internal_Build_UserCheats_Entities(ModelBuilder builder)
        {
            builder.Entity<UserCheat>(entity =>
            {
                entity.HasKey(x => new { x.UserId, x.GameId, x.AccessLevelId });
            });
        }
    }
}
