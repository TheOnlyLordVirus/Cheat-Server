using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheatServer.Database
{
    [Table("ACCESS_LEVELS")]
    public sealed class AccessLevel
    {
        [Key]
        [Column("ID")]
        public Int32 AccessLevelId { get; set; }

        [Required(ErrorMessage = "The 'NAME' field cannot be null or empty.")]
        [Column("NAME")]
        public String AccessLevelName { get; set; } = string.Empty;
    }
}
