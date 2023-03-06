namespace Daemon.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("blogdictionary")]
    public class BlogDictionary : BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }

        [Column("Value")]
        public string Value { get; set; }

        [Column("Remark")]
        public string Remark { get; set; }
        
        [Column("Meaning")]
        public string Meaning { get; set; }

    }
}
