namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("blogroleresource")]
    public partial class BlogRoleResource
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("RoleId")]
        public int RoleId{get;set;}

        [Column("ResourceId")]
        public int ResourceId { get; set; }
    }
}
