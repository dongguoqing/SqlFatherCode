namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("bloguserrole")]
    public partial class BlogUserRole
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("roleid")]
        public int RoleId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }
    }
}
