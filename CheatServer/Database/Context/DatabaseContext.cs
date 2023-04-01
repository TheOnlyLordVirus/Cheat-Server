using Microsoft.EntityFrameworkCore;

namespace CheatServer.Database
{
    public sealed partial class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            Internal_Build_User_Entities(builder);
            Internal_Build_Game_Entities(builder);
            Internal_Build_AccessLevel_Entities(builder);
            Internal_Build_UserCheats_Entities(builder);
            Internal_Build_CheatBinary_Entities(builder);
            Internal_Build_TimeKey_Entities(builder);
        }
    }
}
