using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    [Table("blogfile")]
    public class BlogFile : BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Ext")]
        public string Ext { get; set; }
        [Column("FileVersionId")]
        public string FileVersionId { get; set; }

        [Column("FilePath")]
        public string FilePath { get; set; }

        [Column("OutPath")]
        public string OutPath { get; set; }

        [Column("ServerFilePath")]
        public string ServerFilePath { get; set; }

        [Column("ServerOutPath")]
        public string ServerOutPath { get; set; }

        [Column("IsDelete")]
        public int IsDelete { get; set; }

        [Column("TreeId")]
        public int TreeId { get; set; }
    }
}
