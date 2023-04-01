using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DbSet<CheatBinary> CheatBinaries { get; set; }

        private static void Internal_Build_CheatBinary_Entities(ModelBuilder builder)
        {
            builder.Entity<CheatBinary>(entity =>
            {
                entity.HasKey(x => x.CheatId);
            });
        }
    }
}
