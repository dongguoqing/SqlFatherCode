namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("resource")]
    public partial class Resource:BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("label")]
        public string Label { get; set; }

        [Column("url")]
        public string Url { get; set; }
        [Column("resourceType")]
        public int ResourceType { get; set; }
        [Column("superior")]
        public int Superior { get; set; }
        [Column("delete")]
        public int Delete { get; set; }
        [Column("remark")]
        public string Remark { get; set; }
        [Column("seq")]
        public decimal? Seq { get; set; }
    }
}
