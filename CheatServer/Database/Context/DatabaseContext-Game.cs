using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<Game> Games { get; set; }

        private static void Internal_Build_Game_Entities(ModelBuilder builder)
        {
            builder.Entity<Game>(entity =>
            {
                entity.HasKey(x => x.GameId);
            });
        }
    }
}
