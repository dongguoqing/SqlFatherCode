using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    [Table("filedirectory")]
    public partial class FileDirectory : BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Superior")]
        public int Superior { get; set; }

        [Column("Seq")]
        public decimal Seq { get; set; }
    }
}
