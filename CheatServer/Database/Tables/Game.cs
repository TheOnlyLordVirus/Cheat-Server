using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheatServer.Database
{
    [Table("GAMES")]
    public sealed class Game
    {
        [Key]
        [Column("ID")]
        public Guid GameId { get; set; }

        [Required(ErrorMessage = "The 'GAME_PROCESS_NAME' field cannot be null or empty.")]
        [Column("GAME_PROCESS_NAME")]
        public string GameProcessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The 'GAME_NAME' field cannot be null or empty.")]
        [Column("GAME_NAME")]
        public String GameName { get; set; } = string.Empty;

        [Required(ErrorMessage = "The 'GAME_VERSION' field cannot be null or empty.")]
        [Column("GAME_VERSION")]
        public String GameVersion { get; set; } = string.Empty;
    }
}
