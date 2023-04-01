using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheatServer.Database
{
    [Table("CHEAT_BINARYS")]
    public class CheatBinary
    {
        [Required(ErrorMessage = "The 'ID' field cannot be null or empty.")]
        [Column("ID")]
        public Int32 CheatId { get; set; }


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


        [Required(ErrorMessage = "The 'CHEAT' field cannot be null or empty.")]
        [Column("CHEAT")]
        public String Cheat { get; set; } = String.Empty;

        [Column("DESCRIPTION")]
        public String Description { get; set; } = String.Empty;
    }
}
