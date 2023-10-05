using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheatServer.Database
{
    [Table("TIME_KEYS")]
    public class TimeKey
    {
        [Required(ErrorMessage = "The 'TIME_KEY' field cannot be null or empty.")]
        [Column("TIME_KEY")]
        public String Key { get; set; }


        [Required(ErrorMessage = "The 'GAME_CHEAT_ID' field cannot be null or empty.")]
        [Column("GAME_CHEAT_ID")]
        public Int32 GameCheatId { get; set; }

        [ForeignKey(nameof(GameCheatId))]
        public CheatBinary CheatBinary { get; set; }

        [Required(ErrorMessage = "The 'TIME_VALUE' field cannot be null or empty.")]
        [Column("TIME_VALUE")]
        public Int32 TimeValue { get; set; }

        [Required(ErrorMessage = "The 'KEY_GEN_DATE' field cannot be null or empty.")]
        [Column("KEY_GEN_DATE")]
        public DateTime DateGenerated { get; set; }

        [Required(ErrorMessage = "The 'ACTIVE' field cannot be null or empty.")]
        [Column("ACTIVE")]
        public Boolean Active { get; set; }
    }
}
