using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheatServer.Database
{
    [Table("USER_CHEATS")]
    public class UserCheat
    {
        [Required(ErrorMessage = "The 'USER_ID' field cannot be null or empty.")]
        [Column("USER_ID")]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(GameId))]
        public User User { get; set; }


        [Required(ErrorMessage = "The 'GAME_ID' field cannot be null or empty.")]
        [Column("GAME_ID")]
        public Guid GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }


        [Required(ErrorMessage = "The 'ACCESS_LEVEL' field cannot be null or empty.")]
        [Column("ACCESS_LEVEL")]
        public Int32 AccessLevelId { get; set; }

        [ForeignKey(nameof(AccessLevelId))]
        public AccessLevel AccessLevel { get; set; }


        [Column("AUTH_END_DATE")]
        public DateTime AUTH_END_DATE { get; set; }
    }
}
