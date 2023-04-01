using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CheatServer.Database
{
    [Table("USERS")]
    public sealed class User
    {
        [Key]
        [Column("ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "The 'USER_EMAIL' field cannot be null or empty.")]
        [Column("USER_EMAIL")]
        public String Email { get; set; }

        [Required(ErrorMessage = "The 'USER_NAME' field cannot be null or empty.")]
        [Column("USER_NAME")]
        public String Name { get; set; }

        [Required(ErrorMessage = "The 'IUSER_PASSd' field cannot be null or empty.")]
        [Column("USER_PASS")]
        public String Password { get; set; }

        [Required(ErrorMessage = "The 'IS_ADMIN' field cannot be null or empty.")]
        [Column("IS_ADMIN")]
        public Boolean Admin { get; set; } = false;

        [Required(ErrorMessage = "The 'REGISTRATION_IP' field cannot be null or empty.")]
        [Column("REGISTRATION_IP")]
        public String RegistrationIp { get; set; }

        [Column("RECENT_IP")]
        public String? RecentIp { get; set; }

        [Required(ErrorMessage = "The 'CREATION_DATE' field cannot be null or empty.")]
        [Column("CREATION_DATE")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [Column("HWID")]
        public String HardwareId { get; set; }

        [Required(ErrorMessage = "The 'ACTIVE' field cannot be null or empty.")]
        [Column("ACTIVE")]
        public Boolean Active { get; set; } = true;
    }
}
