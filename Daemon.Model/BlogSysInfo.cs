using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace Daemon.Model
{
    [Table("blogsysinfo")]
    public class BlogSysInfo
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("InfoId")]
        public string InfoId { get; set; }

        [Column("InfoValue")]
        public string InfoValue { get; set; }

    }
}
